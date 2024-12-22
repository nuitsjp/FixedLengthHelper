using System.Text.Json;
using FluentAssertions;

namespace FixedLengthHelper.Test;

public static class FixedLengthDataReaderConfigTest
{
    public class Parse
    {
        [Fact]
        public void Normality()
        {
            const string json = """
                                {
                                  "Columns": {
                                    "Id": {
                                      "Ordinal": 0,
                                      "Offset": 0,
                                      "Length": 5,
                                      "TrimMode": "TrimStart",
                                      "TrimChars": " 0123",
                                      "IsEmptyNull": true
                                    },
                                    "Name": {
                                      "Ordinal": 1,
                                      "Offset": 5,
                                      "Length": 10,
                                      "TrimMode": "TrimEnd"
                                    },
                                    "Description": {
                                      "Ordinal": 2,
                                      "Offset": 15,
                                      "Length": 20,
                                      "TrimMode": "Trim",
                                      "IsEmptyNull": false
                                    }
                                  }
                                }
                                """;

            var config = FixedLengthDataReaderConfig.Deserialize(json);

            config.Should().NotBeNull();
            config.Columns.Should().HaveCount(3);

            var idColumn = config.Columns[0];
            idColumn.Ordinal.Should().Be(0);
            idColumn.Name.Should().Be("Id");
            idColumn.OffsetBytes.Should().Be(0);
            idColumn.LengthBytes.Should().Be(5);
            idColumn.TrimMode.Should().Be(TrimMode.TrimStart);
            idColumn.TrimChars.Should().Equal(' ', '0', '1', '2', '3');
            //idColumn.IsDBNull(string.Empty).Should().BeTrue();
            //idColumn.IsDBNull("abc").Should().BeFalse();

            var nameColumn = config.Columns[1];
            nameColumn.Ordinal.Should().Be(1);
            nameColumn.Name.Should().Be("Name");
            nameColumn.OffsetBytes.Should().Be(5);
            nameColumn.LengthBytes.Should().Be(10);
            nameColumn.TrimMode.Should().Be(TrimMode.TrimEnd);
            nameColumn.TrimChars.Should().BeNull();
            //nameColumn.IsDBNull(string.Empty).Should().BeFalse();

            var descriptionColumn = config.Columns[2];
            descriptionColumn.Ordinal.Should().Be(2);
            descriptionColumn.Name.Should().Be("Description");
            descriptionColumn.OffsetBytes.Should().Be(15);
            descriptionColumn.LengthBytes.Should().Be(20);
            descriptionColumn.TrimMode.Should().Be(TrimMode.Trim);
            //descriptionColumn.TrimChars.Should().BeNull();
            //descriptionColumn.IsDBNull(" ").Should().BeFalse();
        }

        [Fact]
        public void InvalidJson()
        {
            const string json = "invalid json";
            Action act = () => FixedLengthDataReaderConfig.Deserialize(json);
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void MissingColumns()
        {
            const string json = "{}";
            Action act = () => FixedLengthDataReaderConfig.Deserialize(json);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}