using AMYL.Api.Domain.Common;
using MediatR;

namespace AMYL.Api.Abstractions.Messaging;

/// <summary>
/// Non-generic marker for every command, regardless of response type.
/// Pipeline behaviours constrain on this so they apply to writes but not reads.
/// Constraining on <c>ICommand&lt;TResponse&gt;</c> instead would silently never match,
/// because MediatR closes a behaviour with TResponse = Result&lt;T&gt;.
/// </summary>
public interface IBaseCommand;

/// <summary>A write operation that returns no value.</summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>A write operation that returns <typeparamref name="TResponse"/>.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
