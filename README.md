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

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-4712MQ CPU 2.30GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=2240906 Hz, Resolution=446.2481 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]     : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT
  Job-FDOPZB : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  Job-ZGCVYE : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT

IterationCount=3  WarmupCount=3  

```
|               Method |     Toolchain | DataSize | MaxLoop |        Mean |         Error |        StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------------- |-------------- |--------- |-------- |------------:|--------------:|--------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|     **NormalStreamTest** | **.NET Core 2.0** |      **100** |   **10000** |    **648.7 us** |    **212.937 us** |    **11.6718 us** |  **1.00** |    **0.00** |   **1118.1641** |           **-** |           **-** |          **3437.63 KB** |
|    PooledStreamBench | .NET Core 2.0 |      100 |   10000 |    735.3 us |     76.117 us |     4.1722 us |  1.13 |    0.03 |    203.1250 |           - |           - |           625.13 KB |
| RecyclableStreamTest | .NET Core 2.0 |      100 |   10000 | 23,856.4 us |  1,607.937 us |    88.1364 us | 36.78 |    0.74 |   1406.2500 |     31.2500 |     31.2500 |          4510.44 KB |
|       ObjectPoolTest | .NET Core 2.0 |      100 |   10000 |  1,361.7 us |    524.661 us |    28.7585 us |  2.10 |    0.04 |    203.1250 |           - |           - |           625.13 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |      100 |   10000 |    646.7 us |    253.603 us |    13.9008 us |  1.00 |    0.00 |   1122.0703 |      0.9766 |           - |          3437.63 KB |
|    PooledStreamBench | .NET Core 3.0 |      100 |   10000 |    661.5 us |     96.021 us |     5.2632 us |  1.02 |    0.03 |    203.1250 |      0.9766 |           - |           625.13 KB |
| RecyclableStreamTest | .NET Core 3.0 |      100 |   10000 | 18,938.4 us |  7,226.362 us |   396.1013 us | 29.28 |    0.24 |   1375.0000 |     93.7500 |     31.2500 |           4431.3 KB |
|       ObjectPoolTest | .NET Core 3.0 |      100 |   10000 |  1,348.9 us |     99.456 us |     5.4515 us |  2.09 |    0.05 |    203.1250 |      1.9531 |           - |           625.13 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     **NormalStreamTest** | **.NET Core 2.0** |     **1000** |   **10000** |  **1,325.9 us** |     **98.045 us** |     **5.3742 us** |  **1.00** |    **0.00** |   **3482.4219** |           **-** |           **-** |         **10704.13 KB** |
|    PooledStreamBench | .NET Core 2.0 |     1000 |   10000 |  1,004.6 us |    195.528 us |    10.7175 us |  0.76 |    0.01 |    203.1250 |           - |           - |              626 KB |
| RecyclableStreamTest | .NET Core 2.0 |     1000 |   10000 | 24,079.6 us |  2,353.904 us |   129.0254 us | 18.16 |    0.17 |   1406.2500 |     31.2500 |     31.2500 |          4511.31 KB |
|       ObjectPoolTest | .NET Core 2.0 |     1000 |   10000 |  1,561.6 us |     28.349 us |     1.5539 us |  1.18 |    0.00 |    203.1250 |           - |           - |              626 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |     1000 |   10000 |  1,410.9 us |     63.591 us |     3.4856 us |  1.00 |    0.00 |   3494.1406 |      1.9531 |           - |         10704.13 KB |
|    PooledStreamBench | .NET Core 3.0 |     1000 |   10000 |    810.3 us |      7.601 us |     0.4167 us |  0.57 |    0.00 |    204.1016 |      0.9766 |           - |              626 KB |
| RecyclableStreamTest | .NET Core 3.0 |     1000 |   10000 | 19,404.5 us |    491.489 us |    26.9402 us | 13.75 |    0.05 |   1375.0000 |     93.7500 |     31.2500 |          4432.18 KB |
|       ObjectPoolTest | .NET Core 3.0 |     1000 |   10000 |  1,590.7 us |    239.852 us |    13.1471 us |  1.13 |    0.01 |    203.1250 |      1.9531 |           - |              626 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     **NormalStreamTest** | **.NET Core 2.0** |    **50000** |   **10000** | **49,973.9 us** |    **827.128 us** |    **45.3377 us** |  **1.00** |    **0.00** | **158700.0000** |           **-** |           **-** |         **489267.6 KB** |
|    PooledStreamBench | .NET Core 2.0 |    50000 |   10000 | 22,952.2 us |  2,156.559 us |   118.2083 us |  0.46 |    0.00 |    218.7500 |           - |           - |           673.85 KB |
| RecyclableStreamTest | .NET Core 2.0 |    50000 |   10000 | 45,257.4 us | 23,617.234 us | 1,294.5403 us |  0.91 |    0.03 |   1416.6667 |           - |           - |          4559.16 KB |
|       ObjectPoolTest | .NET Core 2.0 |    50000 |   10000 | 23,203.2 us |  5,065.169 us |   277.6390 us |  0.46 |    0.01 |    218.7500 |           - |           - |           673.85 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |    50000 |   10000 | 57,329.2 us |  9,018.936 us |   494.3583 us |  1.00 |    0.00 | 158666.6667 |    111.1111 |           - |         489267.6 KB |
|    PooledStreamBench | .NET Core 3.0 |    50000 |   10000 | 22,975.5 us |  2,237.392 us |   122.6390 us |  0.40 |    0.00 |    187.5000 |     31.2500 |           - |           673.85 KB |
| RecyclableStreamTest | .NET Core 3.0 |    50000 |   10000 | 40,464.6 us |  1,003.835 us |    55.0236 us |  0.71 |    0.01 |   1384.6154 |     76.9231 |           - |          4480.03 KB |
|       ObjectPoolTest | .NET Core 3.0 |    50000 |   10000 | 23,306.6 us |  5,183.999 us |   284.1525 us |  0.41 |    0.01 |    187.5000 |     31.2500 |           - |           673.85 KB |

## Comparison of multithreaded performance

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.590 (1803/April2018Update/Redstone4)
Intel Core i7-4712MQ CPU 2.30GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=2240906 Hz, Resolution=446.2481 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]     : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT
  Job-GQXIHS : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  Job-XJWIZV : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT

IterationCount=3  WarmupCount=3  

```
|                   Method |     Toolchain | ParallelNum | DataSize | MaxLoop |       Mean |      Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------- |-------------- |------------ |--------- |-------- |-----------:|-----------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
|     **NormalStreamParallel** | **.NET Core 2.0** |           **5** |     **1000** |   **10000** |   **1.387 ms** |  **0.5059 ms** | **0.0277 ms** |   **1.00** |    **0.00** |   **3482.4219** |           **-** |           **-** |         **10704.69 KB** |
|     PooledStreamParallel | .NET Core 2.0 |           5 |     1000 |   10000 |   4.328 ms |  0.7547 ms | 0.0414 ms |   3.12 |    0.09 |    851.5625 |           - |           - |             4.11 KB |
|       ObjectPoolParallel | .NET Core 2.0 |           5 |     1000 |   10000 |   9.033 ms |  3.8246 ms | 0.2096 ms |   6.51 |    0.06 |   1015.6250 |           - |           - |          3126.57 KB |
| RecyclableStreamParallel | .NET Core 2.0 |           5 |     1000 |   10000 | 121.768 ms |  6.4595 ms | 0.3541 ms |  87.80 |    1.69 |   7000.0000 |           - |           - |         22011.83 KB |
|                          |               |             |          |         |            |            |           |        |         |             |             |             |                     |
|     NormalStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   1.549 ms |  0.7769 ms | 0.0426 ms |   1.00 |    0.00 |   3494.1406 |      1.9531 |           - |         10704.68 KB |
|     PooledStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   3.696 ms |  2.8743 ms | 0.1576 ms |   2.39 |    0.14 |    851.5625 |      3.9063 |           - |             3.53 KB |
|       ObjectPoolParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   9.255 ms |  4.1239 ms | 0.2260 ms |   5.98 |    0.29 |   1015.6250 |     15.6250 |           - |          3126.56 KB |
| RecyclableStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |  95.083 ms | 10.3835 ms | 0.5692 ms |  61.40 |    1.96 |   7000.0000 |           - |           - |         21620.19 KB |
|                          |               |             |          |         |            |            |           |        |         |             |             |             |                     |
|     **NormalStreamParallel** | **.NET Core 2.0** |          **10** |     **1000** |   **10000** |   **1.417 ms** |  **0.6590 ms** | **0.0361 ms** |   **1.00** |    **0.00** |   **3482.4219** |           **-** |           **-** |         **10704.96 KB** |
|     PooledStreamParallel | .NET Core 2.0 |          10 |     1000 |   10000 |   3.877 ms |  0.3947 ms | 0.0216 ms |   2.74 |    0.06 |    847.6563 |           - |           - |             6.81 KB |
|       ObjectPoolParallel | .NET Core 2.0 |          10 |     1000 |   10000 |  18.106 ms | 10.2389 ms | 0.5612 ms |  12.78 |    0.55 |   2031.2500 |           - |           - |          6251.84 KB |
| RecyclableStreamParallel | .NET Core 2.0 |          10 |     1000 |   10000 | 244.738 ms | 30.9535 ms | 1.6967 ms | 172.79 |    4.20 |  14000.0000 |           - |           - |         43887.02 KB |
|                          |               |             |          |         |            |            |           |        |         |             |             |             |                     |
|     NormalStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   1.552 ms |  0.8917 ms | 0.0489 ms |   1.00 |    0.00 |   3494.1406 |      1.9531 |           - |         10704.95 KB |
|     PooledStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   3.467 ms |  0.8415 ms | 0.0461 ms |   2.24 |    0.05 |    847.6563 |           - |           - |             5.75 KB |
|       ObjectPoolParallel | .NET Core 3.0 |          10 |     1000 |   10000 |  18.621 ms | 10.5156 ms | 0.5764 ms |  12.01 |    0.67 |   2031.2500 |           - |           - |          6251.84 KB |
| RecyclableStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 | 187.789 ms | 22.2849 ms | 1.2215 ms | 121.08 |    3.37 |  14000.0000 |    333.3333 |           - |         43104.75 KB |
