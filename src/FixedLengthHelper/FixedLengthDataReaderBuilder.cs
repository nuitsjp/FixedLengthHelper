using System.Text;

namespace FixedLengthHelper;

public class FixedLengthDataReaderBuilder
{
    private readonly List<Column> _columns = new();

    public FixedLengthDataReader Build(Stream stream, Encoding encoding)
    {
        return new FixedLengthDataReader(
            new FixedLengthReader(new ByteStreamReader(stream), encoding),
            new FixedLengthDataReaderConfig(_columns));
    }
    public FixedLengthDataReaderBuilder AddColumn(int offsetBytes, int lengthBytes, TrimMode trimMode = TrimMode.None, char[]? trimChars = null)
    {
        _columns.Add(new Column(_columns.Count, null, offsetBytes, lengthBytes, trimMode, trimChars));
        return this;
    }
    public FixedLengthDataReaderBuilder AddColumn(string name, int offsetBytes, int lengthBytes, TrimMode trimMode = TrimMode.None, char[]? trimChars = null)
    {
        _columns.Add(new Column(_columns.Count, name, offsetBytes, lengthBytes, trimMode, trimChars));
        return this;
    }
}