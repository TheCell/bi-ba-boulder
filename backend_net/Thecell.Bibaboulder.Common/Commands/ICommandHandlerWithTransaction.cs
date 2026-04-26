using System.Threading.Tasks;

namespace Thecell.Bibaboulder.Common.Commands;

public interface ICommandHandlerWithExtraTransaction<in TCommand>
{
    Task HandleAsync(TCommand command);
}
