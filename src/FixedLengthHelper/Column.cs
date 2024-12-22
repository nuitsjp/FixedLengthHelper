using System.Data;

namespace FixedLengthHelper;

/// <summary>
/// Column definition.
/// </summary>
public record Column(
    int Ordinal,
    string Name,
    int OffsetBytes,
    int LengthBytes,
    SqlDbType? SqlDbType,
    TrimMode TrimMode,
    char[]? TrimChars,
    bool EmptyIsNull,
    Func<string, object> Convert);