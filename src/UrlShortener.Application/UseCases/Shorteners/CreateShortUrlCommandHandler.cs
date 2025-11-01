using Microsoft.Extensions.Logging;
using UrlShortener.Application.Abstractions.Cache;
using UrlShortener.Application.Abstractions.Data;
using UrlShortener.Application.Abstractions.Messaging;
using UrlShortener.Application.Abstractions.ShortCode;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Errors;
using UrlShortener.Domain.ValueObjects;
using UrlShortener.SharedKernel.Results;

namespace UrlShortener.Application.UseCases.Shorteners;

public class CreateShortUrlCommandHandler(
    IShortCodeGenerator codeGenerator,
    IApplicationDbContext dbContext,
    ICacheService cacheService,
    ILogger<CreateShortUrlCommandHandler> logger)
    : ICommandHandler<CreateShortUrlCommand, UrlResponse>
{
    public async Task<Result<UrlResponse>> Handle(CreateShortUrlCommand command, CancellationToken cancellationToken)
    {
        // 1. Validação e Criação do Value Object
        var longUrlResult = LongUrl.Create(command.LongUrl.ToString());

        // --- 2. VERIFICAÇÃO DE DUPLICIDADE (CACHE-FIRST) ---
        // Chave baseada na LongUrl (para evitar duplicidade)
        var redisKeyLongUrl = $"long:{longUrlResult.Value}";

        var existingShortCode = await cacheService.GetStringAsync(redisKeyLongUrl);

        if (existingShortCode != null)
        {
            // Cache Hit: A URL Longa já foi encurtada. Retorna o ShortCode existente.
            logger.LogInformation("Short code for URL: {LongUrl} already exists in cache.", longUrlResult.Value);
            return Result.Success(new UrlResponse { ShortCode = existingShortCode });
        }

        // --- 3. CRIAÇÃO NO DB (Cache Miss) ---
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 3.1. Criação da Entidade e Persistência Inicial (para obter o ID)
            var urlMapping = UrlMapping.Create(longUrlResult);

            await dbContext.UrlMappings.AddAsync(urlMapping, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken); // ID é gerado aqui

            // 3.2. Geração do ShortCode (Obscurecimento)
            var shortCode = codeGenerator.Encode(urlMapping.Id);

            if (shortCode is null || string.IsNullOrWhiteSpace(shortCode))
            {
                logger.LogError("Failed to generate short code for URL mapping ID: {UrlMappingId}", urlMapping.Id);
                return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
            }

            // 3.3. Atualização da Entidade e Persistência Final
            urlMapping.AddShortCode(ShortCode.Create(shortCode));
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // --- 4. POPULAÇÃO DO CACHE ---
            // Salva o par LongUrl -> ShortCode no Cache para futuras verificações de duplicidade
            await cacheService.SetStringAsync(redisKeyLongUrl, shortCode, TimeSpan.FromDays(30));

            return Result.Success(new UrlResponse
            {
                ShortCode = urlMapping.ShortCode!.Value
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Failed to create short URL for {LongUrl}", longUrlResult.Value);
            return Result.Failure<UrlResponse>(UrlMappingErrors.CreationFailed());
        }
    }
}