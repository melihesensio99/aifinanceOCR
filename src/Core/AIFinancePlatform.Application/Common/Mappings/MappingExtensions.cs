using System.Linq;
using System.Threading.Tasks;
using AIFinancePlatform.Application.Common.Models;

namespace AIFinancePlatform.Application.Common.Mappings;

public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize) where TDestination : class
    {
        return PaginatedList<TDestination>.CreateAsync(queryable, pageNumber, pageSize);
    }
}
