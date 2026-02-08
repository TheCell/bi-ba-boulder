using System.Threading.Tasks;
using System.Transactions;
using Thecell.Bibaboulder.Common.Commands;

namespace Thecell.Bibaboulder.Common.Decorators;

public class TransactionScopeDecorator<TCommand> : ICommandHandler<TCommand>
{
    private readonly ICommandHandler<TCommand> _decorated;

    public TransactionScopeDecorator(ICommandHandler<TCommand> decorated)
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
