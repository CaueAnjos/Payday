using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Exceptions;

[Serializable]
public class EntityNotFoundException : Exception
{
    public Type? Entitytype { get; }
    public int? Id { get; }

    public EntityNotFoundException()
        : base("There was an attempt to find an entity that does not exist") { }

    public EntityNotFoundException(Type entityType, int id)
        : base(BuildMessage(entityType, id))
    {
        Entitytype = entityType;
        Id = id;
    }

    public EntityNotFoundException(string message)
        : base(message) { }

    public EntityNotFoundException(string message, Exception inner)
        : base(message, inner) { }

    private static string BuildMessage(Type entityType, int id) =>
        $"There was an attempt to find an entity that does not exist ({entityType.Name}: ID#{id})";
}
