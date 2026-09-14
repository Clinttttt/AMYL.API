namespace AMYL.Api.Domain.Common;

/// <summary>
/// Lets generic code build a failed result of the caller's own type at compile time.
/// <para>
/// A pipeline behaviour knows only <c>TResponse</c>, and <c>Result&lt;T&gt;.ValidationFailure</c>
/// cannot be called without knowing <c>T</c>. The static abstract member closes that gap:
/// <c>TResponse.ValidationFailure(errors)</c> resolves to the right factory at compile time,
/// replacing what used to need reflection.
/// </para>
/// </summary>
/// <typeparam name="TSelf">The implementing result type itself.</typeparam>
public interface IValidationResult<TSelf>
    where TSelf : Result, IValidationResult<TSelf>
{
    static abstract TSelf ValidationFailure(IReadOnlyDictionary<string, string[]> errors);
}
