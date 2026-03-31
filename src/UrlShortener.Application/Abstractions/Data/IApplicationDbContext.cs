using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<UrlMapping> UrlMappings { get; }
    DbSet<UrlClick> UrlClicks { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}