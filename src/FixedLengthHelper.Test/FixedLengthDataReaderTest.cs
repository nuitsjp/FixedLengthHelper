﻿using System.Text;
using FluentAssertions;

namespace FixedLengthHelper.Test;

public class FixedLengthDataReaderTest
{
#if NET8_0_OR_GREATER
    static FixedLengthDataReaderTest()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
#endif

    [Fact]
    public void FieldCount()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .AddColumn("CustomerId", 0, 5)
            .AddColumn("Name", 5, 21)
            .AddColumn("Balance", 26, 15)
            .Build(stream, Encoding.UTF8);

        // Act & Assert
        reader.FieldCount.Should().Be(3);
    }

    [Fact]
    public void Depth()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);

        // Act & Assert
        reader.Depth.Should().Be(0);
    }

    [Fact]
    public void IsClosed()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);

        // Act & Assert
        reader.IsClosed.Should().BeFalse();
        reader.Dispose();
        reader.IsClosed.Should().BeTrue();
    }

    [Fact]
    public void RecordsAffected()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);

        // Act & Assert
        reader.RecordsAffected.Should().Be(0);
    }

    [Fact]
    public void This_ByOrdinal()
    {
        // Arrange
        var stream = new MemoryStream(
            """
                00554Pedro Gomez          123423006022004
                01732中村 充志        004350011052002
                00112Ramiro Politti       000000001022000
                00924Pablo Ramirez        033213024112002
                """u8.ToArray());

        // Act
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .AddColumn(0, 5)
            .AddColumn(5, 21)
            .AddColumn(26, 15)
            .Build(stream, Encoding.UTF8);

        // Assert
        reader.Read().Should().BeTrue();
        reader[0].Should().Be("00554");
        reader[1].Should().Be("Pedro Gomez          ");
        reader[2].Should().Be("123423006022004");

        reader.Read().Should().BeTrue();
        reader[0].Should().Be("01732");
        reader[1].Should().Be("中村 充志        ");
        reader[2].Should().Be("004350011052002");

        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeFalse();
    }

    [Fact]
    public void This_ByName()
    {
        // Arrange
        var stream = new MemoryStream(
            """
                00554Pedro Gomez          123423006022004
                01732中村 充志        004350011052002
                00112Ramiro Politti       000000001022000
                00924Pablo Ramirez        033213024112002
                """u8.ToArray());

        // Act
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .AddColumn("CustomerId", 0, 5)
            .AddColumn("Name", 5, 21)
            .AddColumn("Balance", 26, 15)
            .Build(stream, Encoding.UTF8);

        // Assert
        reader.Read().Should().BeTrue();
        reader["CustomerId"].Should().Be("00554");
        reader["Name"].Should().Be("Pedro Gomez          ");
        reader["Balance"].Should().Be("123423006022004");

        reader.Read().Should().BeTrue();
        reader["CustomerId"].Should().Be("01732");
        reader["Name"].Should().Be("中村 充志        ");
        reader["Balance"].Should().Be("004350011052002");

        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeFalse();
    }

    [Fact]
    public void GetFieldType()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);
        // Act & Assert
        reader.GetFieldType(0).Should().Be(typeof(string));
    }

    public class GetValue
    {
        [Fact]
        public void Normality()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                    00554Pedro Gomez          123423006022004
                    01732中村 充志        004350011052002
                    00112Ramiro Politti       000000001022000
                    00924Pablo Ramirez        033213024112002
                    """u8.ToArray());

            // Act
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .AddColumn(0, 5)
                .AddColumn(5, 21)
                .AddColumn(26, 15)
                .Build(stream, Encoding.UTF8);

            // Assert
            reader.Read().Should().BeTrue();
            reader.GetValue(0).Should().Be("00554");
            reader.GetValue(1).Should().Be("Pedro Gomez          ");
            reader.GetValue(2).Should().Be("123423006022004");

            reader.Read().Should().BeTrue();
            reader.GetValue(0).Should().Be("01732");
            reader.GetValue(1).Should().Be("中村 充志        ");
            reader.GetValue(2).Should().Be("004350011052002");

            reader.Read().Should().BeTrue();
            reader.Read().Should().BeTrue();
            reader.Read().Should().BeFalse();
        }

        [Fact]
        public void WithTrim()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                    00554 Pedro Gomez         123423006022004
                    01732中村 充志        004350011052002
                    00112Ramiro Politti       000000001022000
                    00924Pablo Ramirez        033213024112002
                    """u8.ToArray());

            // Act
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .AddColumn(0, 5, TrimMode.TrimStart, ['0', '2', '4'])
                .AddColumn(5, 21, TrimMode.TrimEnd)
                .AddColumn(26, 15, TrimMode.Trim, ['0', '1', '2', '4'])
                .Build(stream, Encoding.UTF8);

            // Assert
            reader.Read().Should().BeTrue();
            reader.GetValue(0).Should().Be("554");
            reader.GetValue(1).Should().Be(" Pedro Gomez");
            reader.GetValue(2).Should().Be("3423006");

            reader.Read().Should().BeTrue();
            reader.GetValue(0).Should().Be("1732");
            reader.GetValue(1).Should().Be("中村 充志");
            reader.GetValue(2).Should().Be("35001105");

            reader.Read().Should().BeTrue();
            reader.Read().Should().BeTrue();
            reader.Read().Should().BeFalse();
        }

        [Fact]
        public void WhenEmpty()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                                              123423006022004
                    """u8.ToArray());

            // Act
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .AddColumn(0, 5, TrimMode.Trim, isEmptyNull: true)
                .AddColumn(5, 21, TrimMode.Trim)
                .AddColumn(26, 10, s => true)
                .AddColumn("Balance", 36, 5, s => true)
                .Build(stream, Encoding.UTF8);

            // Assert
            reader.Read().Should().BeTrue();
            reader.GetValue(0).Should().Be(DBNull.Value);
            reader.GetValue(1).Should().Be(string.Empty);
            reader.GetValue(2).Should().Be(DBNull.Value);
            reader.GetValue(3).Should().Be(DBNull.Value);
        }

        [Fact]
        public void WhenNotExist()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                    00554Pedro Gomez          123423006022004
                    """u8.ToArray());
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .Build(stream, Encoding.UTF8);

            // Act
            reader.Read().Should().BeTrue();
            // ReSharper disable once AccessToDisposedClosure
            var act = () => reader.GetValue(0);

            // Assert
            act.Should().Throw<IndexOutOfRangeException>();
        }
    }

    public class GetOrdinal
    {
        [Fact]
        public void Normality()
        {
            // Arrange
            var stream = new MemoryStream(""u8.ToArray());
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .AddColumn("CustomerId", 0, 5)
                .AddColumn("Name", 5, 21)
                .AddColumn("Balance", 26, 15)
                .Build(stream, Encoding.UTF8);

            // Act & Assert
            reader.GetOrdinal("CustomerId").Should().Be(0);
            reader.GetOrdinal("Name").Should().Be(1);
            reader.GetOrdinal("Balance").Should().Be(2);
        }

        [Fact]
        public void WhenNotExist()
        {
            // Arrange
            var stream = new MemoryStream(""u8.ToArray());
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .Build(stream, Encoding.UTF8);

            // Act
            // ReSharper disable once AccessToDisposedClosure
            var act = () => reader.GetOrdinal("CustomerId");

            // Assert
            act.Should().Throw<IndexOutOfRangeException>();
        }
    }

    [Fact]
    public void Close()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);
        // Act & Assert
        reader.IsClosed.Should().BeFalse();
        reader.Close();
        reader.IsClosed.Should().BeTrue();
        reader.Close();
    }

#if NET8_0_OR_GREATER
    [Fact]
    public async Task DisposeAsync()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);
        // Act & Assert
        reader.IsClosed.Should().BeFalse();
        await reader.DisposeAsync();
        reader.IsClosed.Should().BeTrue();
        await reader.DisposeAsync();
    }
#endif

    // ReSharper disable once InconsistentNaming
    public class IsDBNull
    {
        [Fact]
        public void Normality()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                                              123423006022004
                    """u8.ToArray());

            // Act
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .AddColumn(0, 5, TrimMode.Trim, isEmptyNull: true)
                .AddColumn(5, 21, TrimMode.Trim)
                .AddColumn(26, 10, s => true)
                .AddColumn("Balance", 36, 5, s => true)
                .Build(stream, Encoding.UTF8);

            // Assert
            reader.Read().Should().BeTrue();
            reader.IsDBNull(0).Should().BeTrue();
            reader.IsDBNull(1).Should().BeFalse();
            reader.IsDBNull(2).Should().BeTrue();
            reader.IsDBNull(3).Should().BeTrue();
        }

        [Fact]
        public void WhenNotExist()
        {
            // Arrange
            var stream = new MemoryStream(
                """
                    00554Pedro Gomez          123423006022004
                    """u8.ToArray());
            using var reader = FixedLengthDataReader
                .CreateBuilder()
                .Build(stream, Encoding.UTF8);

            // Act
            reader.Read().Should().BeTrue();
            // ReSharper disable once AccessToDisposedClosure
            var act = () => reader.IsDBNull(0);

            // Assert
            act.Should().Throw<IndexOutOfRangeException>();
        }

    }

    [Fact]
    public void NotSupported()
    {
        // Arrange
        var stream = new MemoryStream(""u8.ToArray());
        using var reader = FixedLengthDataReader
            .CreateBuilder()
            .Build(stream, Encoding.UTF8);

        // Act & Assert
        // ReSharper disable AccessToDisposedClosure
        ((Action)(() => reader.GetName(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetDataTypeName(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetValues(new object[1]))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetBoolean(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetByte(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetBytes(0, 0, new byte[1], 0, 1))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetChar(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetChars(0, 0, new char[1], 0, 1))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetGuid(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetInt16(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetInt32(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetInt64(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetFloat(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetDouble(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetString(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetDecimal(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetDateTime(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetData(0))).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.GetSchemaTable())).Should().Throw<NotSupportedException>();
        ((Action)(() => reader.NextResult())).Should().Throw<NotSupportedException>();
        // ReSharper restore AccessToDisposedClosure
    }
}