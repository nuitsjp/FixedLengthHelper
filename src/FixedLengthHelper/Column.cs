namespace FixedLengthHelper;

/// <summary>
/// Column definition.
/// </summary>
/// <param name="Ordinal">Ordinal position of the column.</param>
/// <param name="Name">Name of the column.</param>
/// <param name="OffsetBytes">Offset in bytes from the beginning of the record.</param>
/// <param name="LengthBytes">Length of the column in bytes.</param>
/// <param name="TrimMode">Trim mode.</param>
/// <param name="TrimChars">Characters to trim.</param>
/// <param name="IsDBNull">Function to determine if the value is DBNull.</param>
public record Column(
    int Ordinal,
    string? Name,
    int OffsetBytes,
    int LengthBytes,
    TrimMode TrimMode,
    char[]? TrimChars,
    // ReSharper disable once InconsistentNaming
    Func<string, bool> IsDBNull);
