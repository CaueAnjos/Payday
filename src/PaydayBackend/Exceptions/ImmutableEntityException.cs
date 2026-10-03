namespace PaydayBackend.Exceptions;

[Serializable]
public class ImmutableEntityException : Exception
{
    public ImmutableEntityException()
        : base("There was an attempt to modify or delete an immutable entity") { }

    public ImmutableEntityException(string message)
        : base(message) { }

    public ImmutableEntityException(string message, Exception inner)
        : base(message, inner) { }
}
