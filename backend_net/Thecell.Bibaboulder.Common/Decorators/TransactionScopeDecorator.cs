using System.Threading.Tasks;
using System.Transactions;
using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Common.Decorators;

public class TransactionScopeDecorator<TCommand> : ICommandHandlerWithExtraTransaction<TCommand>
{
    private readonly ICommandHandlerWithExtraTransaction<TCommand> _decorated;

    public TransactionScopeDecorator(ICommandHandlerWithExtraTransaction<TCommand> decorated)
    {
        _decorated = decorated;
    }

    public virtual async Task HandleAsync(TCommand command)
    {
        var isolationLevel = IsolationLevel.ReadCommitted;
        // if a transaction with a different isolation level is already running, continue with this isolation level
        // otherwise creating the TransactionScope will fail
        if (Transaction.Current != null && Transaction.Current.IsolationLevel != IsolationLevel.ReadCommitted)
        {
            isolationLevel = Transaction.Current.IsolationLevel;
        }

        using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = isolationLevel }, TransactionScopeAsyncFlowOption.Enabled);
        await _decorated.HandleAsync(command);
        scope.Complete();
    }
}
