using System.Linq.Expressions;

namespace CatalogSupport.SharedKernel.Specifications;

public abstract class Specification<T>(Expression<Func<T, bool>> criteria)
    where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; } = criteria;

    public List<Expression<Func<T, object>>> IncludesExpression { get; } = [];

    public Expression<Func<T, object>>? OrderByExpression { get; private set; }

    public Expression<Func<T, object>>? OrderByDescendingExpression { get; private set; }

    public int Take { get; private set; }

    public int Skip { get; private set; }

    protected void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        IncludesExpression.Add(includeExpression);
    }

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderByExpression = orderByExpression;
    }

    protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescendingExpression = orderByDescendingExpression;
    }

    protected void AddPagination(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }
}
