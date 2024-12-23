using System.Data;
using FluentAssertions;

namespace FixedLengthHelper.Test;

public class ColumnTest
{
    [Fact]
    public void Basic()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2);

        // Act
        var column = options.Build();

        // Assert
        column.Ordinal.Should().Be(0);
        column.Name.Should().Be("column");
        column.OffsetBytes.Should().Be(1);
        column.LengthBytes.Should().Be(2);

        column.Convert("Hello").Should().Be("Hello");
    }

    [Fact]
    public void Trim()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .Trim();
        var column = options.Build();

        // Act
        var actual = column.Convert("  Hello  ");

        // Assert
        actual.Should().Be("Hello");
    }

    [Fact]
    public void TrimWithChars()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .Trim("012".ToCharArray());
        var column = options.Build();

        // Act
        var actual = column.Convert("012Hello012");
        
        // Assert
        actual.Should().Be("Hello");
    }

    [Fact]
    public void TrimStart()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .TrimStart();
        var column = options.Build();

        // Act
        var actual = column.Convert("  Hello  ");

        // Assert
        actual.Should().Be("Hello  ");
    }

    [Fact]
    public void TrimStartWithChars()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .TrimStart("012".ToCharArray());
        var column = options.Build();
        // Act
        var actual = column.Convert("012Hello012");

        // Assert
        actual.Should().Be("Hello012");
    }

    [Fact]
    public void TrimEnd()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .TrimEnd();
        var column = options.Build();
        // Act
        var actual = column.Convert("  Hello  ");
        // Assert
        actual.Should().Be("  Hello");
    }

    [Fact]
    public void TrimEndWithChars()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .TrimEnd("012".ToCharArray());
        var column = options.Build();
        // Act
        var actual = column.Convert("012Hello012");
        // Assert
        actual.Should().Be("012Hello");
    }

    [Fact]
    public void TreatEmptyStringAsNull()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .TreatEmptyStringAsNull();
        var column = options.Build();

        // Act
        var actual = column.Convert(string.Empty);

        // Assert
        actual.Should().Be(DBNull.Value);
    }

    [Fact]
    public void AsType_BIT()
    {
        // Arrange
        var options = new ColumnOptions(0, "column", 1, 2)
            .AsType(SqlDbType.Bit);
        var column = options.Build();

        // Act & Assert
        column.Convert("1").Should().Be(true);
        column.Convert("0").Should().Be(false);
        column.Convert("").Should().Be(DBNull.Value);
        var act = () => column.Convert("Hello");
        act.Should().Throw<InvalidCastException>();


    }
}