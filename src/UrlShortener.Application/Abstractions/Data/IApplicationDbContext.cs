using System;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<UrlMapping> UrlMappings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}