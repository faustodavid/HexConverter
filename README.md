# HexConverter
High-Performance hexadecimal converter with support of `Span<T>` and buffering.

[![Nuget](https://img.shields.io/nuget/v/HexConverter)](https://www.nuget.org/packages/HexConverter/)

## Installation

Available on [nuget](https://www.nuget.org/packages/HexConverter/)

	PM> Install-Package HexConverter

Requirements:
* System.Memory (>= 4.5.3)

## Why?
Dotnet core implementation do not take advantage of `Span<T>` to slice the memory without allocations and don't use stackalloc/pooled memory to avoid heap allocations, making the code slow.

## Getting started
### Hexadecimal string to byte array

*Using Linq*
```csharp
string hexString = "01AAB1DC10DD";
bytes[] bytes = Enumerable.Range(0, hexString.Length)
                                .Where(x => x % 2 == 0)
                                .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                                .ToArray()
/*bytes items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

As you can see you have allocations for each two char in the hexString because is doing a substring and you have an allocation for the byte array.

Using HexConverter
```csharp
string hexString = "01AAB1DC10DD";
bytes[] bytes = HexConverter.GetBytes(hexString);

/*bytes items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

Using HexConverter with pooled array to avoid heap allocations
```csharp
string hexString = "01AAB1DC10DD";
using var pooledBytes = HexConverter.GetBytesPooled(hexString);

/*bytes.ArraySegment items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

Using HexConverter with stackalloc buffer to work with small hex strings
```csharp
string hexString = "01AAB1DC10DD";
Span<byte> buffer = stackalloc byte[HexConverter.GetBytesCount(hexString)];
int count = HexConverter.GetBytes(hexString, buffer);

/*buffer items:
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

## BenchmarksLower is better
Lower is better
<img src="https://github.com/faustodavid/HexConverter/raw/master/perf/docs/results/BytesToHex.png" />


Lower is better
<img src="https://github.com/faustodavid/HexConverter/raw/master/perf/docs/results/HexToBytes.png" />
