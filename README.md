# HexConverter
High-Performance hexadecimal converter with support of `Span<T>` and buffering.

[![Nuget](https://img.shields.io/nuget/v/HexConverter)](https://www.nuget.org/packages/HexConverter/)

## Why?
Dotnet core implementation do not take advantage of `Span<T>` to slice the memory without allocations and don't use stackalloc/pooled memory to avoid heap allocations, making the code slow.

## Getting started
### Hexadecimal string to byte array
Using HexConverter
```csharp
string hexString = "01AAB1DC10DD";
bytes[] bytes = HexConverter.GetBytes(hexString);

/*bytes items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

### Byte array to hexadecimal string
```csharp
byte[] vals = { 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD };
string str = HexConverter.ToString(vals);
Console.WriteLine(str);

/*Output:
  01AAB1DC10DD
 */
```

