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

**Using Linq**

As you can see you have allocations for each two char in the hexString because is doing a substring and you have an allocation for the byte array.
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

**Using HexConverter**

You get just the allocation for the final array.
```csharp
string hexString = "01AAB1DC10DD";
bytes[] bytes = HexConverter.GetBytes(hexString);

/*bytes items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

**Using HexConverter with pooled array to avoid heap allocations**

Zero allocation, and easy to use. Optimise for strings longer than 100 characters.
```csharp
string hexString = "01AAB1DC10DD";
using var pooledBytes = HexConverter.GetBytesPooled(hexString);

/*bytes.ArraySegment items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

**Using HexConverter with stackalloc buffer to work with small hex strings**

Zero allocation, and optimise for small hex string.
```csharp
string hexString = "01AAB1DC10DD";
Span<byte> buffer = stackalloc byte[HexConverter.GetBytesCount(hexString)];
int count = HexConverter.GetBytes(hexString, buffer);

/*buffer items:
{ 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD }
*/
```

### Byte array to hexadecimal string
With this linq implementation you have allocation for the intermidiate array, and a string per each byte and the a new one for the final string.
**Using Linq**
```csharp
byte[] vals = { 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD };
string str = string.Concat(vals.Select(b => b.ToString("x2")));
Console.WriteLine(str);

/*Output:
  01AAB1DC10DD
 */
```

**Using HexConverter**

You get just the allocation for the final string.
```csharp
byte[] vals = { 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD };
string str = HexConverter.GetString(vals);
Console.WriteLine(str);

/*Output:
  01AAB1DC10DD
 */
```

**Using HexConverter with stackalloc buffer to work with small hex strings**

Zero allocation, and easy to use. Optimise for array longer than 100 items.
```csharp
byte[] vals = { 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD };
using var pooledChars = HexConverter.GetCharsPooled(vals);
Console.WriteLine(pooledChars.ArraySegment.AsSpan());

/*Output:
  01AAB1DC10DD
 */
```

**Using HexConverter with stackalloc buffer to work with small byte array**

Zero allocation, and optimise for small byte arrays.
```csharp
byte[] vals = { 0x01, 0xAA, 0xB1, 0xDC, 0x10, 0xDD };
Span<byte> buffer = stackalloc byte[HexConverter.GetBytesCount(vals)];
HexConverter.GetChars(vals, buffer);
Console.WriteLine(buffer.AsSpan());

/*Output:
  01AAB1DC10DD
 */
```

## Benchmarks
Lower is better
<img src="https://github.com/faustodavid/HexConverter/raw/master/perf/docs/results/BytesToHex.png" />


Lower is better
<img src="https://github.com/faustodavid/HexConverter/raw/master/perf/docs/results/HexToBytes.png" />
