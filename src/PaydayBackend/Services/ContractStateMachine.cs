using PaydayBackend.Exceptions;
using PaydayBackend.Models;

namespace PaydayBackend.Services;

/// <summary>
/// Encodes the (currently implicit) Contract state machine:
///
///   Open            -- PaymentDate reached --&gt; AwaitingClosure
///   AwaitingClosure -- every participant signed --&gt; Closed
///
/// Nothing else in the codebase ever moves a contract out of `Open`, so the
/// `Open -&gt; AwaitingClosure` transition is applied lazily: whenever a contract is
/// about to be mutated (a payment or a close signature added), we first bring its
/// `State` up to date based on the current time.
/// </summary>
public static class ContractStateMachine
{
    /// <summary>
    /// Brings <paramref name="contract"/>.State up to date with the current time,
    /// mutating it in place if a transition is due. Does not persist anything.
    /// </summary>
    public static void SyncState(Contract contract)
    {
        if (contract.State == ContractState.Open && DateTime.UtcNow >= contract.PaymentDate)
            contract.State = ContractState.AwaitingClosure;
    }

    /// <summary>
    /// Throws unless new payments can currently be added to <paramref name="contract"/>.
    /// </summary>
    public static void EnsureCanAddPayment(Contract contract)
    {
        SyncState(contract);

        if (contract.State != ContractState.Open)
            throw new InvalidContractStateException(
                contract.Id,
                contract.State,
                $"Cannot add a payment to contract #{contract.Id}: it is no longer open (current state: {contract.State})."
            );
    }

    /// <summary>
    /// Throws unless <paramref name="contract"/> can currently be signed for closure.
    /// </summary>
    public static void EnsureCanSign(Contract contract)
    {
        SyncState(contract);

        if (contract.State != ContractState.AwaitingClosure)
            throw new InvalidContractStateException(
                contract.Id,
                contract.State,
                $"Cannot sign contract #{contract.Id}: it is not awaiting closure yet (current state: {contract.State})."
            );
    }

    /// <summary>
    /// Closes <paramref name="contract"/> (sets State to Closed and CloseDate to now)
    /// if every participant has already signed its closure.
    /// </summary>
    public static void TryClose(Contract contract)
    {
        if (contract.State != ContractState.AwaitingClosure || contract.Participants.Count == 0)
            return;

        var signedParticipantIds = contract.CloseSignatures.Select(s => s.OwnerId).ToHashSet();
        var allSigned = contract.Participants.All(p => signedParticipantIds.Contains(p.Id));

        if (!allSigned)
            return;

        contract.State = ContractState.Closed;
        contract.CloseDate = DateTime.UtcNow;
    }
}
