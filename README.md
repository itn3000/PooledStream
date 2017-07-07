# Overview

[![NuGet version](https://badge.fury.io/nu/PooledStream.svg)](https://badge.fury.io/nu/PooledStream)

[![Build status](https://ci.appveyor.com/api/projects/status/yhm2jto1ed0q0x0y/branch/master?svg=true)](https://ci.appveyor.com/project/itn3000/pooledstream-kviqp/branch/master)

this library aims to efficient MemoryStream for large data.

**STILL UNDER DEVELOPMENT**

# Usage

You can add reference as [NuGet package](https://www.nuget.org/packages/PooledStream/).
Once you add the reference, you can use PooledStream.PooledMemoryStream.

## Code Examples

### Basic usage

```csharp
// you can create stream with no parameter(using default setting).
// you can specify ArrayPool instance and initial capacity.
using(var stm = new PooledStream.PooledMemoryStream())
{
  // you can ensure internal buffer length before write to stream.
  stm.Reserve(1024);
  var wdata = new byte[128];
  // write stream
  stm.Write(wdata, 0, wdata.Length);
  // you can get written data by ToArray()
  var data = stm.ToArray();
  // or you can get ArraySegment without allocate new data.
  // WARNING: you must not use the returned ArraySegment after dispose Stream 
  var segment = stm.ToUnsafeArraySegment();
}
```

### Readonly stream

```csharp
var data = new byte[128];
// if you specify bytearray to constructor parameter,
// PooledMemoryStream will be created as readonly stream.
using(var stm = new PooledStream.PooledMemoryStream(data))
{
  var buf = new byte[32];
  stm.Read(buf, 0, buf.Length);
  // if you try to write, InvalidOperationException will be thrown
  stm.Write(buf, 0, buf.Length);
}
```

# Micro benchmark result(powered by [BenchmarkDotNet](http://benchmarkdotnet.org/))

## Comparison of single thread performance

``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3312643 Hz, Resolution=301.8738 ns, Timer=TSC
dotnet cli version=2.0.0-preview2-006497
  [Host]     : .NET Core 4.6.00001.0, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.00001.0, 64bit RyuJIT


```
 |               Method | DataSize | MaxLoop |        Mean |      Error |     StdDev | Scaled | ScaledSD |       Gen 0 |     Gen 1 |   Gen 2 |   Allocated |
 |--------------------- |--------- |-------- |------------:|-----------:|-----------:|-------:|---------:|------------:|----------:|--------:|------------:|
 |     **NormalStreamTest** |      **100** |   **10000** |    **541.5 us** |   **2.272 us** |   **2.126 us** |   **1.00** |     **0.00** |    **838.8672** |         **-** |       **-** |  **3437.63 KB** |
 |    PooledStreamBench |      100 |   10000 |    612.1 us |  10.134 us |  10.407 us |   1.13 |     0.02 |    170.8984 |         - |       - |   703.25 KB |
 | RecyclableStreamTest |      100 |   10000 | 21,536.6 us | 276.913 us | 259.025 us |  39.77 |     0.49 |   1062.5000 |   31.2500 | 31.2500 |  4510.44 KB |
 |       ObjectPoolTest |      100 |   10000 |  1,198.0 us |  18.856 us |  17.637 us |   2.21 |     0.03 |    152.3438 |         - |       - |   625.13 KB |
 |     **NormalStreamTest** |     **1000** |   **10000** |  **1,147.0 us** |   **7.268 us** |   **6.443 us** |   **1.00** |     **0.00** |   **2611.3281** |         **-** |       **-** | **10704.13 KB** |
 |    PooledStreamBench |     1000 |   10000 |    839.7 us |  11.962 us |  11.190 us |   0.73 |     0.01 |    171.8750 |         - |       - |   704.13 KB |
 | RecyclableStreamTest |     1000 |   10000 | 21,669.4 us | 286.048 us | 267.570 us |  18.89 |     0.25 |   1062.5000 |   31.2500 | 31.2500 |  4511.31 KB |
 |       ObjectPoolTest |     1000 |   10000 |  1,318.9 us |   3.252 us |   2.539 us |   1.15 |     0.01 |    152.3438 |         - |       - |      626 KB |
 |     **NormalStreamTest** |    **50000** |   **10000** | **43,009.7 us** | **147.280 us** | **137.766 us** |   **1.00** |     **0.00** | **119000.0000** | **3125.0000** |       **-** | **489267.6 KB** |
 |    PooledStreamBench |    50000 |   10000 | 19,792.9 us | 230.006 us | 215.148 us |   0.46 |     0.01 |    156.2500 |         - |       - |   751.98 KB |
 | RecyclableStreamTest |    50000 |   10000 | 40,295.7 us | 242.423 us | 214.902 us |   0.94 |     0.01 |   1062.5000 |         - |       - |  4559.16 KB |
 |       ObjectPoolTest |    50000 |   10000 | 19,891.6 us | 176.917 us | 165.489 us |   0.46 |     0.00 |    156.2500 |         - |       - |   673.85 KB |

## Comparison of multithreaded performance

``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3312643 Hz, Resolution=301.8738 ns, Timer=TSC
dotnet cli version=2.0.0-preview2-006497
  [Host]     : .NET Core 4.6.00001.0, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.00001.0, 64bit RyuJIT


```
 |                   Method | ParallelNum | DataSize | MaxLoop |       Mean |     Error |    StdDev | Scaled | ScaledSD |      Gen 0 |   Allocated |
 |------------------------- |------------ |--------- |-------- |-----------:|----------:|----------:|-------:|---------:|-----------:|------------:|
 |     **NormalStreamParallel** |           **5** |     **1000** |   **10000** |   **1.183 ms** | **0.0091 ms** | **0.0081 ms** |   **1.00** |     **0.00** |  **2611.3281** | **10704.69 KB** |
 |     PooledStreamParallel |           5 |     1000 |   10000 |   3.695 ms | 0.0370 ms | 0.0328 ms |   3.12 |     0.03 |   652.3438 |     4.12 KB |
 |       ObjectPoolParallel |           5 |     1000 |   10000 |   7.776 ms | 0.1162 ms | 0.1030 ms |   6.57 |     0.09 |   757.8125 |  3126.57 KB |
 | RecyclableStreamParallel |           5 |     1000 |   10000 | 108.640 ms | 1.6470 ms | 1.5406 ms |  91.85 |     1.39 |  5312.5000 | 22011.83 KB |
 |     **NormalStreamParallel** |          **10** |     **1000** |   **10000** |   **1.186 ms** | **0.0059 ms** | **0.0052 ms** |   **1.00** |     **0.00** |  **2611.3281** | **10704.96 KB** |
 |     PooledStreamParallel |          10 |     1000 |   10000 |   3.317 ms | 0.0088 ms | 0.0082 ms |   2.80 |     0.01 |   652.3438 |     6.86 KB |
 |       ObjectPoolParallel |          10 |     1000 |   10000 |  15.258 ms | 0.0872 ms | 0.0728 ms |  12.86 |     0.08 |  1515.6250 |  6251.84 KB |
 | RecyclableStreamParallel |          10 |     1000 |   10000 | 215.123 ms | 1.0908 ms | 0.8517 ms | 181.33 |     1.03 | 10625.0000 | 43887.02 KB |
