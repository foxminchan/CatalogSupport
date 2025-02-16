using MediatR;

namespace CatalogSupport.SharedKernel.Query;

public interface IQuery<out TResponse> : IRequest<TResponse>;
