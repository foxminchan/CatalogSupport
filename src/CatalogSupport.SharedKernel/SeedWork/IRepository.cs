namespace CatalogSupport.SharedKernel.SeedWork;

public interface IRepository
{
    IUnitOfWork UnitOfWork { get; }
}
