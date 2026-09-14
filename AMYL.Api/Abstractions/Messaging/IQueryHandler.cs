using AMYL.Api.Domain.Common;
using MediatR;

namespace AMYL.Api.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
