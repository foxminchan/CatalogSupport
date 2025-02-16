using Microsoft.EntityFrameworkCore;

namespace CatalogSupport.SharedKernel.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> GetQuery<T>(
        IQueryable<T> inputQuery,
        Specification<T> specification
    )
        where T : class
    {
        var query = inputQuery;

        if (specification.Criteria is not null)
        {
            query = query.Where(specification.Criteria);
        }

        query = specification.IncludesExpression.Aggregate(
            query,
            (current, include) => current.Include(include)
        );

        if (specification.OrderByExpression is not null)
        {
            query = query.OrderBy(specification.OrderByExpression);
        }
        else if (specification.OrderByDescendingExpression is not null)
        {
            query = query.OrderByDescending(specification.OrderByDescendingExpression);
        }

        if (specification.Skip != 0)
        {
            query = query.Skip(specification.Skip);
        }

        if (specification.Take != 0)
        {
            query = query.Take(specification.Take);
        }

        return query;
    }
}
