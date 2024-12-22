using System.Text.Json;

namespace FixedLengthHelper;

/// <summary>
/// FixedLengthDataReader configuration.
/// </summary>
/// <param name="Columns"></param>
public record FixedLengthDataReaderConfig(IReadOnlyList<Column> Columns)
{
    /// <summary>
    /// Options for JsonSerializer.
    /// </summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    /// <summary>
    /// Deserializes the JSON string to FixedLengthDataReaderConfig.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static FixedLengthDataReaderConfig Deserialize(string json)
    {
        var jsonObject = JsonSerializer.Deserialize<JsonObject>(json, Options);
        if (jsonObject?.Columns == null) throw new InvalidOperationException("Invalid JSON structure");

        var columns = jsonObject.Columns.Select(kvp =>
        {
            var column = kvp.Value;
            // ReSharper disable once InconsistentNaming
            Func<string, bool> isDBNull = column.IsEmptyNull switch
            {
                true => string.IsNullOrEmpty,
                false => _ => false,
                null => _ => false
            };

            return new Column(
                column.Ordinal,
                kvp.Key,
                column.Offset,
                column.Length,
                null,
#if NET8_0_OR_GREATER
                Enum.Parse<TrimMode>(column.TrimMode),
#else
                (TrimMode)Enum.Parse(typeof(TrimMode), column.TrimMode),
#endif
                column.TrimChars?.ToCharArray(),
                true,
                s => s
            );
        }).ToList();

        return new FixedLengthDataReaderConfig(columns);
    }

    /// <summary>
    /// JSON object.
    /// </summary>
    /// <param name="Columns"></param>
    private record JsonObject(Dictionary<string, ColumnRaw>? Columns);

    /// <summary>
    /// Column definition.
    /// </summary>
    /// <param name="Ordinal"></param>
    /// <param name="Offset"></param>
    /// <param name="Length"></param>
    /// <param name="TrimMode"></param>
    /// <param name="TrimChars"></param>
    /// <param name="IsEmptyNull"></param>
    // ReSharper disable once ClassNeverInstantiated.Local
    private record ColumnRaw(
        int Ordinal,
        int Offset,
        int Length,
        string TrimMode,
        string? TrimChars,
        bool? IsEmptyNull
    );
}