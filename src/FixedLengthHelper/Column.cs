namespace FixedLengthHelper;

public record Column(
    int Ordinal,
    string? Name,
    int OffsetBytes,
    int LengthBytes,
    TrimMode TrimMode,
    char[]? TrimChars,
    // ReSharper disable once InconsistentNaming
    Func<string, bool> IsDBNull);
