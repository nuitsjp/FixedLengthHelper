namespace FixedLengthHelper;

public record Column(
    int Ordinal,
    string? Name,
    int OffsetBytes,
    int LengthBytes,
    TrimMode TrimMode,
    char[]? TrimChars
);