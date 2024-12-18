using System.Text;
using FixedLengthHelper;

var reader = new FixedLengthReader("Sample.txt", Encoding.UTF8);

while (await reader.ReadAsync())
{
    Console.WriteLine($"CustomerId: {reader.GetField(0, 5, TrimMode.TrimStart, '0')}");
    Console.WriteLine($"Name      : {reader.GetField(5, 21, TrimMode.TrimEnd)}");
    Console.WriteLine($"Balance   : {reader.GetField(26, 15, TrimMode.TrimStart, '0')}");
    Console.WriteLine();
}