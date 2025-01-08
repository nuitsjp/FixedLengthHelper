using System.Text;

namespace FixedLengthHelper.Test;

public static class StringExtensions
{
    public static int GetBytes(this string s, Encoding encoding)
    {
        var bytes = encoding.GetBytes(s);
        return encoding.GetByteCount(s);
    }

    public static Stream ToStream(this string s, Encoding encoding)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream, encoding);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}