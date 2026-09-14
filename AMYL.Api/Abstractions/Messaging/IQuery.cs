using AMYL.Api.Domain.Common;
using MediatR;

namespace AMYL.Api.Abstractions.Messaging;

/// <summary>A read operation. Queries are never validated by the pipeline and never mutate state.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
