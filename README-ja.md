# FixedLengthHelper

FixedLengthHelperは固定長フォーマットを扱うためのライブラリです。固定長フォーマットを扱う低レベルの操作を提供します。

FixedLengthHelperはマルチバイト文字列やサロゲートペアもサポートしています。

NuGet: [FixedLengthHelper](https://www.nuget.org/packages/FixedLengthHelper)

```pwsh
Install-Package FixedLengthHelper
```

## 導入

サンプルテキスト:

```txt
00554Pedro Gomez          123423006022004
01732中村 充志        004350011052002
00112Ramiro Politti       000000001022000
00924Pablo Ramirez        033213024112002
```

漢字はUTF-8で1文字3バイトです。

サンプルコード:

```cs
using System.Text；
FixedLengthHelper；

var reader = new FixedLengthReader("Sample.txt", Encoding.UTF8)；

while (await reader.ReadAsync())
{
    Console.WriteLine($"CustomerId： {reader.GetField(0, 5, TrimMode.TrimStart, '0')}")；
    Console.WriteLine($「Name : {reader.GetField(5, 21, TrimMode.TrimEnd)}」)；
    Console.WriteLine($「Balance : {reader.GetField(26, 15, TrimMode.TrimStart, 『0』)}」)；
    Console.WriteLine()；
}
```

以下の結果が得られます:

```cmd
CustomerId: 554
Name      : Pedro Gomez
Balance   : 123423006022004

CustomerId: 1732
Name      : 中村 充志
Balance   : 4350011052002
...
```