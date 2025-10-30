using UrlShortener.SharedKernel.Events;

namespace UrlShortener.SharedKernel.Entities;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; }
    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id) && GetType() == other.GetType();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, GetType());
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}