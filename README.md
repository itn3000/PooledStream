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

BenchmarkDotNet=v0.11.4, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4770 CPU 3.40GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3312639 Hz, Resolution=301.8741 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]     : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT
  Job-UYVIHQ : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  Job-OKLOIT : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT

IterationCount=3  WarmupCount=3  

```
|               Method |     Toolchain | DataSize | MaxLoop |        Mean |         Error |        StdDev | Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|--------------------- |-------------- |--------- |-------- |------------:|--------------:|--------------:|------:|--------:|------------:|------------:|------------:|--------------------:|
|     **NormalStreamTest** | **.NET Core 2.0** |      **100** |   **10000** |    **547.2 us** |    **239.991 us** |    **13.1547 us** |  **1.00** |    **0.00** |    **838.8672** |           **-** |           **-** |          **3437.63 KB** |
|    PooledStreamBench | .NET Core 2.0 |      100 |   10000 |    717.2 us |     57.219 us |     3.1364 us |  1.31 |    0.03 |    152.3438 |           - |           - |           625.13 KB |
| RecyclableStreamTest | .NET Core 2.0 |      100 |   10000 | 20,736.4 us |  3,475.612 us |   190.5100 us | 37.91 |    0.65 |   1062.5000 |     31.2500 |     31.2500 |          4510.44 KB |
|       ObjectPoolTest | .NET Core 2.0 |      100 |   10000 |  1,177.5 us |    467.010 us |    25.5984 us |  2.15 |    0.04 |    152.3438 |           - |           - |           625.13 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |      100 |   10000 |    582.2 us |    152.348 us |     8.3507 us |  1.00 |    0.00 |    840.8203 |      0.9766 |           - |          3437.63 KB |
|    PooledStreamBench | .NET Core 3.0 |      100 |   10000 |    502.8 us |      7.588 us |     0.4159 us |  0.86 |    0.01 |    152.3438 |      0.9766 |           - |           625.13 KB |
| RecyclableStreamTest | .NET Core 3.0 |      100 |   10000 | 16,097.3 us |  1,734.643 us |    95.0816 us | 27.65 |    0.35 |   1031.2500 |     93.7500 |     31.2500 |           4431.3 KB |
|       ObjectPoolTest | .NET Core 3.0 |      100 |   10000 |  1,217.7 us |    845.284 us |    46.3329 us |  2.09 |    0.11 |    152.3438 |      1.9531 |           - |           625.13 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     **NormalStreamTest** | **.NET Core 2.0** |     **1000** |   **10000** |  **1,127.8 us** |     **86.465 us** |     **4.7394 us** |  **1.00** |    **0.00** |   **2611.3281** |           **-** |           **-** |         **10704.13 KB** |
|    PooledStreamBench | .NET Core 2.0 |     1000 |   10000 |  1,072.4 us |    110.832 us |     6.0751 us |  0.95 |    0.00 |    152.3438 |           - |           - |              626 KB |
| RecyclableStreamTest | .NET Core 2.0 |     1000 |   10000 | 21,393.0 us |  9,297.220 us |   509.6120 us | 18.97 |    0.38 |   1062.5000 |     31.2500 |     31.2500 |          4511.31 KB |
|       ObjectPoolTest | .NET Core 2.0 |     1000 |   10000 |  1,341.7 us |    116.204 us |     6.3696 us |  1.19 |    0.01 |    152.3438 |           - |           - |              626 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |     1000 |   10000 |  1,313.7 us |    146.403 us |     8.0249 us |  1.00 |    0.00 |   2619.1406 |      1.9531 |           - |         10704.13 KB |
|    PooledStreamBench | .NET Core 3.0 |     1000 |   10000 |    695.2 us |    143.958 us |     7.8908 us |  0.53 |    0.01 |    152.3438 |      0.9766 |           - |              626 KB |
| RecyclableStreamTest | .NET Core 3.0 |     1000 |   10000 | 17,292.9 us |  3,992.965 us |   218.8679 us | 13.16 |    0.24 |   1031.2500 |     93.7500 |     31.2500 |          4432.18 KB |
|       ObjectPoolTest | .NET Core 3.0 |     1000 |   10000 |  1,339.0 us |     96.621 us |     5.2961 us |  1.02 |    0.00 |    152.3438 |      1.9531 |           - |              626 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     **NormalStreamTest** | **.NET Core 2.0** |    **50000** |   **10000** | **42,464.4 us** |  **5,354.767 us** |   **293.5129 us** |  **1.00** |    **0.00** | **119000.0000** |   **5583.3333** |           **-** |         **489267.6 KB** |
|    PooledStreamBench | .NET Core 2.0 |    50000 |   10000 | 18,481.9 us |    549.875 us |    30.1405 us |  0.44 |    0.00 |    156.2500 |           - |           - |           673.85 KB |
| RecyclableStreamTest | .NET Core 2.0 |    50000 |   10000 | 38,660.2 us |  5,480.035 us |   300.3792 us |  0.91 |    0.01 |   1071.4286 |           - |           - |          4559.16 KB |
|       ObjectPoolTest | .NET Core 2.0 |    50000 |   10000 | 19,214.0 us |  1,304.480 us |    71.5029 us |  0.45 |    0.00 |    156.2500 |           - |           - |           673.85 KB |
|                      |               |          |         |             |               |               |       |         |             |             |             |                     |
|     NormalStreamTest | .NET Core 3.0 |    50000 |   10000 | 55,819.0 us | 18,768.893 us | 1,028.7863 us |  1.00 |    0.00 | 119000.0000 |    100.0000 |           - |         489267.6 KB |
|    PooledStreamBench | .NET Core 3.0 |    50000 |   10000 | 19,005.9 us |  2,020.870 us |   110.7707 us |  0.34 |    0.01 |    156.2500 |     31.2500 |           - |           673.85 KB |
| RecyclableStreamTest | .NET Core 3.0 |    50000 |   10000 | 35,596.9 us | 23,828.634 us | 1,306.1278 us |  0.64 |    0.04 |   1000.0000 |     66.6667 |           - |          4480.03 KB |
|       ObjectPoolTest | .NET Core 3.0 |    50000 |   10000 | 19,276.8 us |  2,661.927 us |   145.9092 us |  0.35 |    0.01 |    156.2500 |     31.2500 |           - |           673.85 KB |

## Comparison of multithreaded performance

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4770 CPU 3.40GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3312639 Hz, Resolution=301.8741 ns, Timer=TSC
.NET Core SDK=3.0.100-preview3-010431
  [Host]     : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT
  Job-USTLPH : .NET Core 2.0.9 (CoreCLR 4.6.26614.01, CoreFX 4.6.26614.01), 64bit RyuJIT
  Job-NVQLSR : .NET Core 3.0.0-preview3-27503-5 (CoreCLR 4.6.27422.72, CoreFX 4.7.19.12807), 64bit RyuJIT

IterationCount=3  WarmupCount=3  

```
|                   Method |     Toolchain | ParallelNum | DataSize | MaxLoop |       Mean |       Error |    StdDev |  Ratio | RatioSD | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|------------------------- |-------------- |------------ |--------- |-------- |-----------:|------------:|----------:|-------:|--------:|------------:|------------:|------------:|--------------------:|
|     **NormalStreamParallel** | **.NET Core 2.0** |           **5** |     **1000** |   **10000** |   **1.223 ms** |   **0.1911 ms** | **0.0105 ms** |   **1.00** |    **0.00** |   **2611.3281** |           **-** |           **-** |         **10704.69 KB** |
|     PooledStreamParallel | .NET Core 2.0 |           5 |     1000 |   10000 |   3.738 ms |   0.5457 ms | 0.0299 ms |   3.06 |    0.05 |    636.7188 |           - |           - |             4.09 KB |
|       ObjectPoolParallel | .NET Core 2.0 |           5 |     1000 |   10000 |   7.824 ms |   4.5276 ms | 0.2482 ms |   6.40 |    0.24 |    757.8125 |           - |           - |          3126.57 KB |
| RecyclableStreamParallel | .NET Core 2.0 |           5 |     1000 |   10000 | 108.985 ms | 113.9751 ms | 6.2474 ms |  89.10 |    4.98 |   5200.0000 |           - |           - |         22011.83 KB |
|                          |               |             |          |         |            |             |           |        |         |             |             |             |                     |
|     NormalStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   1.519 ms |   0.5097 ms | 0.0279 ms |   1.00 |    0.00 |   2619.1406 |           - |           - |         10704.68 KB |
|     PooledStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   3.213 ms |   2.1407 ms | 0.1173 ms |   2.12 |    0.10 |    636.7188 |      3.9063 |           - |             3.54 KB |
|       ObjectPoolParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   8.005 ms |   2.4718 ms | 0.1355 ms |   5.27 |    0.05 |    757.8125 |      7.8125 |           - |          3126.56 KB |
| RecyclableStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |  93.885 ms |  90.2172 ms | 4.9451 ms |  61.87 |    4.31 |   5166.6667 |           - |           - |         21620.19 KB |
|                          |               |             |          |         |            |             |           |        |         |             |             |             |                     |
|     **NormalStreamParallel** | **.NET Core 2.0** |          **10** |     **1000** |   **10000** |   **1.222 ms** |   **0.5980 ms** | **0.0328 ms** |   **1.00** |    **0.00** |   **2611.3281** |           **-** |           **-** |         **10704.96 KB** |
|     PooledStreamParallel | .NET Core 2.0 |          10 |     1000 |   10000 |   3.203 ms |   0.7506 ms | 0.0411 ms |   2.62 |    0.10 |    636.7188 |           - |           - |              6.8 KB |
|       ObjectPoolParallel | .NET Core 2.0 |          10 |     1000 |   10000 |  15.600 ms |   0.0925 ms | 0.0051 ms |  12.77 |    0.34 |   1500.0000 |           - |           - |          6251.84 KB |
| RecyclableStreamParallel | .NET Core 2.0 |          10 |     1000 |   10000 | 212.558 ms |  19.6829 ms | 1.0789 ms | 173.97 |    5.53 |  10666.6667 |           - |           - |         43887.02 KB |
|                          |               |             |          |         |            |             |           |        |         |             |             |             |                     |
|     NormalStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   1.466 ms |   0.9149 ms | 0.0501 ms |   1.00 |    0.00 |   2619.1406 |           - |           - |         10704.95 KB |
|     PooledStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   2.884 ms |   0.7946 ms | 0.0436 ms |   1.97 |    0.04 |    636.7188 |      3.9063 |           - |             5.75 KB |
|       ObjectPoolParallel | .NET Core 3.0 |          10 |     1000 |   10000 |  15.111 ms |   1.7868 ms | 0.0979 ms |  10.31 |    0.29 |   1515.6250 |     15.6250 |           - |          6251.84 KB |
| RecyclableStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 | 175.536 ms |  53.7284 ms | 2.9450 ms | 119.80 |    4.79 |  10333.3333 |    333.3333 |           - |         43104.75 KB |
