# FixedLengthHelper

FixedLengthHelper is a library to work with fixed-length formats. Provides low-level operations that handle fixed-length formats.

FixedLengthHelper also supports multi-byte strings and surrogate pairs.

NuGet: [FixedLengthHelper](https://www.nuget.org/packages/FixedLengthHelper)

```pwsh
Install-Package FixedLengthHelper
```

## Introduction

Sample text:

```txt
00554Pedro Gomez          123423006022004
01732中村 充志        004350011052002
00112Ramiro Politti       000000001022000
00924Pablo Ramirez        033213024112002
```

Kanji characters are 3 bytes per character in UTF-8.

Sample code:

```cs
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
```

The following results are obtained:

```cmd
CustomerId: 554
Name      : Pedro Gomez
Balance   : 123423006022004

CustomerId: 1732
Name      : 中村 充志
Balance   : 4350011052002
...
```