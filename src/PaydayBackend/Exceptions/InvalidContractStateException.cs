using PaydayBackend.Models;

namespace PaydayBackend.Exceptions;

[Serializable]
public class InvalidContractStateException : Exception
{
    public int? ContractId { get; }
    public ContractState? State { get; }

    public InvalidContractStateException()
        : base("The contract is not in the appropriate state for this operation") { }

    public InvalidContractStateException(int contractId, ContractState state, string message)
        : base(message)
    {
        ContractId = contractId;
        State = state;
    }

    public InvalidContractStateException(string message)
        : base(message) { }

    public InvalidContractStateException(string message, Exception inner)
        : base(message, inner) { }
}
