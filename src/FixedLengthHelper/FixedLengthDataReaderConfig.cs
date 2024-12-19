using System.Text.Json;

namespace FixedLengthHelper;

public record FixedLengthDataReaderConfig(IReadOnlyList<Column> Columns)
{
    public static FixedLengthDataReaderConfig Parse(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonObject = JsonSerializer.Deserialize<JsonObject>(json, options);
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
#if NET8_0_OR_GREATER
                Enum.Parse<TrimMode>(column.TrimMode),
#else
                (TrimMode)Enum.Parse(typeof(TrimMode), column.TrimMode),
#endif
                column.TrimChars?.ToCharArray(),
                isDBNull
            );
        }).ToList();

        return new FixedLengthDataReaderConfig(columns);
    }

    private record JsonObject(Dictionary<string, ColumnRaw>? Columns);

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