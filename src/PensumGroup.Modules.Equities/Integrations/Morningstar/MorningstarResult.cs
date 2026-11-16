namespace PensumGroup.Modules.Equities.Integrations.Morningstar;

public enum MorningstarResultStatus
{
    Success,
    NotFound,
    Failure,
}

public sealed class MorningstarResult<T>
{
    private MorningstarResult(MorningstarResultStatus status, T? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public MorningstarResultStatus Status { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static MorningstarResult<T> Success(T value) => new(MorningstarResultStatus.Success, value, null);

    public static MorningstarResult<T> NotFound() => new(MorningstarResultStatus.NotFound, default, null);

    public static MorningstarResult<T> Failure(string error) => new(MorningstarResultStatus.Failure, default, error);
}
