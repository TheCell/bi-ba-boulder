using System.Threading.Tasks;

namespace Thecell.Bibaboulder.Common.Commands;

public interface ICommandHandler<in TCommand>
{
    Task HandleAsync(TCommand command);
}
