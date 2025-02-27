﻿using MediatR;

namespace CatalogSupport.SharedKernel.Query;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>;
