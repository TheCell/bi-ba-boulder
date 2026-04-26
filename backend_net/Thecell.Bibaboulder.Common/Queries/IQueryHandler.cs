using System.Threading.Tasks;

namespace Thecell.Bibaboulder.Common.Queries;

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query);
}
