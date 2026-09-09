using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Exceptions;

[Serializable]
public class DuplicateEntityException : Exception
{
    public Entity? Entity { get; }

    public DuplicateEntityException()
        : base("There was an attempt to create an entity that already exists") { }

    public DuplicateEntityException(Entity entity)
        : base(BuildMessage(entity))
    {
        Entity = entity;
    }

    public DuplicateEntityException(string message)
        : base(message) { }

    public DuplicateEntityException(string message, Exception inner)
        : base(message, inner) { }

    private static string BuildMessage(Entity entity) =>
        $"There was an attempt to create an entity that already exists ({entity.GetType().Name}: ID#{entity.Id})";
}
