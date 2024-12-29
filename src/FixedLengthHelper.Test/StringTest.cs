using FluentAssertions;

namespace FixedLengthHelper.Test;

public class StringTest
{
    [Fact]
    public void TrimWhenTrimCharsIsNull()
    {
        " 123 ".Trim(null)
            .Should().Be("123");
    }

    [Fact]
    public void TrimWhenTrimCharsNotContainsSpace()
    {
        " 123 ".Trim('1')
            .Should().Be(" 123 ");
    }
}