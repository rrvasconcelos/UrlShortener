namespace UrlShortener.Application.Abstractions.IdGeneration;

public interface ISnowflakeIdGenerator
{
    long NextId();
}
