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

### Reusing Stream(Advanced Usage)

for avoiding allocation completely, [Microsoft.Extensions.ObjectPool](https://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/) is useful.
code example:

```csharp
// using Microsoft.Extensions.ObjectPool;
// using PooledStream;

// initializing object pool instance.This should be singleton.
ObjectPool<PooledMemoryStream> pool = ObjectPool.Create<PooledMemoryStream>();
var stm = pool.Get();
// initializing stream(buffer initialization included)
stm.SetLength(0);
try
{
  // using stream...
  ...
}
finally
{
  // returning instance
  pool.Return(stm);
}
```

#### WARNING

* **You must call `SetLength(0)` at first, and call `Return(stm)` in the end for preventing memory leak.**
* **`PooledMemoryStream` is not threadsafe, so you must not share instance across threads**

# Micro benchmark result(powered by [BenchmarkDotNet](http://benchmarkdotnet.org/))

## Comparison of single thread performance

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4770 CPU 3.40GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3312643 Hz, Resolution=301.8738 ns, Timer=TSC
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-XHTYBG : .NET Core 2.1.13 (CoreCLR 4.6.28008.01, CoreFX 4.6.28008.01), X64 RyuJIT
  Job-GARYQQ : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

IterationCount=3  WarmupCount=3  

```
|               Method |     Toolchain | DataSize | MaxLoop |        Mean |       Error |      StdDev | Ratio | RatioSD |       Gen 0 |     Gen 1 |   Gen 2 |    Allocated |
|--------------------- |-------------- |--------- |-------- |------------:|------------:|------------:|------:|--------:|------------:|----------:|--------:|-------------:|
|     **NormalStreamTest** | **.NET Core 2.1** |      **100** |   **10000** |    **594.9 us** |    **566.6 us** |    **31.05 us** |  **1.00** |    **0.00** |    **838.8672** |         **-** |       **-** |   **3437.63 KB** |
|    PooledStreamBench | .NET Core 2.1 |      100 |   10000 |    624.5 us |    374.8 us |    20.54 us |  1.05 |    0.03 |    152.3438 |         - |       - |    625.13 KB |
| RecyclableStreamTest | .NET Core 2.1 |      100 |   10000 | 22,279.9 us |  7,740.3 us |   424.27 us | 37.51 |    1.86 |   1062.5000 |   31.2500 | 31.2500 |   4509.43 KB |
|       ObjectPoolTest | .NET Core 2.1 |      100 |   10000 |  1,213.3 us |  1,083.3 us |    59.38 us |  2.04 |    0.16 |    152.3438 |         - |       - |    625.13 KB |
|                      |               |          |         |             |             |             |       |         |             |           |         |              |
|     NormalStreamTest | .NET Core 3.0 |      100 |   10000 |    534.3 us |    251.6 us |    13.79 us |  1.00 |    0.00 |    840.8203 |         - |       - |   3437.63 KB |
|    PooledStreamBench | .NET Core 3.0 |      100 |   10000 |    569.7 us |    276.6 us |    15.16 us |  1.07 |    0.02 |    152.3438 |         - |       - |    625.13 KB |
| RecyclableStreamTest | .NET Core 3.0 |      100 |   10000 | 16,857.0 us | 17,255.1 us |   945.81 us | 31.59 |    2.58 |   1031.2500 |   31.2500 | 31.2500 |    4431.3 KB |
|       ObjectPoolTest | .NET Core 3.0 |      100 |   10000 |  1,070.7 us |  1,282.0 us |    70.27 us |  2.00 |    0.09 |    152.3438 |         - |       - |    625.13 KB |
|                      |               |          |         |             |             |             |       |         |             |           |         |              |
|     **NormalStreamTest** | **.NET Core 2.1** |     **1000** |   **10000** |  **1,227.5 us** |  **2,856.2 us** |   **156.56 us** |  **1.00** |    **0.00** |   **2611.3281** |         **-** |       **-** |  **10704.13 KB** |
|    PooledStreamBench | .NET Core 2.1 |     1000 |   10000 |    890.6 us |    810.8 us |    44.45 us |  0.73 |    0.10 |    152.3438 |         - |       - |       626 KB |
| RecyclableStreamTest | .NET Core 2.1 |     1000 |   10000 | 23,016.1 us | 13,867.5 us |   760.13 us | 18.99 |    2.83 |   1062.5000 |   31.2500 | 31.2500 |    4510.3 KB |
|       ObjectPoolTest | .NET Core 2.1 |     1000 |   10000 |  1,435.7 us |    295.8 us |    16.21 us |  1.18 |    0.13 |    152.3438 |         - |       - |       626 KB |
|                      |               |          |         |             |             |             |       |         |             |           |         |              |
|     NormalStreamTest | .NET Core 3.0 |     1000 |   10000 |  1,073.5 us |    868.1 us |    47.58 us |  1.00 |    0.00 |   2619.1406 |         - |       - |  10704.13 KB |
|    PooledStreamBench | .NET Core 3.0 |     1000 |   10000 |    833.5 us |    472.9 us |    25.92 us |  0.78 |    0.06 |    152.3438 |         - |       - |       626 KB |
| RecyclableStreamTest | .NET Core 3.0 |     1000 |   10000 | 16,644.2 us |  2,705.4 us |   148.29 us | 15.52 |    0.54 |   1031.2500 |   31.2500 | 31.2500 |   4432.18 KB |
|       ObjectPoolTest | .NET Core 3.0 |     1000 |   10000 |  1,235.9 us |    448.9 us |    24.61 us |  1.15 |    0.06 |    152.3438 |         - |       - |    626.01 KB |
|                      |               |          |         |             |             |             |       |         |             |           |         |              |
|     **NormalStreamTest** | **.NET Core 2.1** |    **50000** |   **10000** | **44,963.8 us** | **48,970.5 us** | **2,684.24 us** |  **1.00** |    **0.00** | **119000.0000** | **6090.9091** |       **-** |  **489267.6 KB** |
|    PooledStreamBench | .NET Core 2.1 |    50000 |   10000 | 18,804.6 us |    865.1 us |    47.42 us |  0.42 |    0.02 |    156.2500 |         - |       - |    673.85 KB |
| RecyclableStreamTest | .NET Core 2.1 |    50000 |   10000 | 39,927.1 us |  5,089.0 us |   278.95 us |  0.89 |    0.06 |   1076.9231 |         - |       - |   4558.16 KB |
|       ObjectPoolTest | .NET Core 2.1 |    50000 |   10000 | 19,251.4 us |    358.7 us |    19.66 us |  0.43 |    0.03 |    156.2500 |         - |       - |    673.85 KB |
|                      |               |          |         |             |             |             |       |         |             |           |         |              |
|     NormalStreamTest | .NET Core 3.0 |    50000 |   10000 | 45,709.8 us | 57,887.2 us | 3,172.99 us |  1.00 |    0.00 | 119000.0000 | 4166.6667 |       - | 489268.05 KB |
|    PooledStreamBench | .NET Core 3.0 |    50000 |   10000 | 18,791.7 us |  2,900.4 us |   158.98 us |  0.41 |    0.03 |    156.2500 |         - |       - |    673.86 KB |
| RecyclableStreamTest | .NET Core 3.0 |    50000 |   10000 | 35,318.4 us |  8,499.6 us |   465.89 us |  0.77 |    0.05 |   1000.0000 |   66.6667 |       - |   4480.03 KB |
|       ObjectPoolTest | .NET Core 3.0 |    50000 |   10000 | 18,578.5 us |    642.9 us |    35.24 us |  0.41 |    0.03 |    156.2500 |         - |       - |    673.85 KB |

## Comparison of multithreaded performance

``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 8.1 (6.3.9600.0)
Intel Core i7-4770 CPU 3.40GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
Frequency=3312643 Hz, Resolution=301.8738 ns, Timer=TSC
.NET Core SDK=3.0.100
  [Host]     : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT
  Job-HFLIQV : .NET Core 2.1.13 (CoreCLR 4.6.28008.01, CoreFX 4.6.28008.01), X64 RyuJIT
  Job-JJMYBO : .NET Core 3.0.0 (CoreCLR 4.700.19.46205, CoreFX 4.700.19.46214), X64 RyuJIT

IterationCount=3  WarmupCount=3  

```
|                   Method |     Toolchain | ParallelNum | DataSize | MaxLoop |       Mean |      Error |    StdDev |  Ratio | RatioSD |      Gen 0 | Gen 1 | Gen 2 |   Allocated |
|------------------------- |-------------- |------------ |--------- |-------- |-----------:|-----------:|----------:|-------:|--------:|-----------:|------:|------:|------------:|
|     **NormalStreamParallel** | **.NET Core 2.1** |           **5** |     **1000** |   **10000** |   **1.194 ms** |  **0.2631 ms** | **0.0144 ms** |   **1.00** |    **0.00** |  **2611.3281** |     **-** |     **-** | **10704.69 KB** |
|     PooledStreamParallel | .NET Core 2.1 |           5 |     1000 |   10000 |   3.563 ms |  1.7474 ms | 0.0958 ms |   2.99 |    0.10 |   636.7188 |     - |     - |     3.55 KB |
|       ObjectPoolParallel | .NET Core 2.1 |           5 |     1000 |   10000 |   8.777 ms |  5.3239 ms | 0.2918 ms |   7.36 |    0.34 |          - |     - |     - |  3126.57 KB |
| RecyclableStreamParallel | .NET Core 2.1 |           5 |     1000 |   10000 | 112.561 ms | 61.1552 ms | 3.3521 ms |  94.30 |    2.62 |  5200.0000 |     - |     - | 22010.82 KB |
|                          |               |             |          |         |            |            |           |        |         |            |       |       |             |
|     NormalStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   1.171 ms |  0.2942 ms | 0.0161 ms |   1.00 |    0.00 |  2619.1406 |     - |     - | 10704.67 KB |
|     PooledStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   3.101 ms |  1.9374 ms | 0.1062 ms |   2.65 |    0.09 |   636.7188 |     - |     - |  2580.41 KB |
|       ObjectPoolParallel | .NET Core 3.0 |           5 |     1000 |   10000 |   6.728 ms |  0.3479 ms | 0.0191 ms |   5.75 |    0.09 |   757.8125 |     - |     - |  3126.55 KB |
| RecyclableStreamParallel | .NET Core 3.0 |           5 |     1000 |   10000 |  85.958 ms |  2.1704 ms | 0.1190 ms |  73.44 |    1.09 |  5166.6667 |     - |     - | 21620.26 KB |
|                          |               |             |          |         |            |            |           |        |         |            |       |       |             |
|     **NormalStreamParallel** | **.NET Core 2.1** |          **10** |     **1000** |   **10000** |   **1.270 ms** |  **0.3971 ms** | **0.0218 ms** |   **1.00** |    **0.00** |  **2611.3281** |     **-** |     **-** | **10704.96 KB** |
|     PooledStreamParallel | .NET Core 2.1 |          10 |     1000 |   10000 |   3.150 ms |  0.2103 ms | 0.0115 ms |   2.48 |    0.03 |   636.7188 |     - |     - |     5.78 KB |
|       ObjectPoolParallel | .NET Core 2.1 |          10 |     1000 |   10000 |  15.877 ms |  6.3941 ms | 0.3505 ms |  12.50 |    0.07 |  1500.0000 |     - |     - |  6251.84 KB |
| RecyclableStreamParallel | .NET Core 2.1 |          10 |     1000 |   10000 | 220.645 ms | 72.7076 ms | 3.9853 ms | 173.73 |    5.86 | 10666.6667 |     - |     - | 43886.01 KB |
|                          |               |             |          |         |            |            |           |        |         |            |       |       |             |
|     NormalStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   1.221 ms |  1.2462 ms | 0.0683 ms |   1.00 |    0.00 |  2619.1406 |     - |     - | 10704.95 KB |
|     PooledStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 |   2.739 ms |  0.4759 ms | 0.0261 ms |   2.25 |    0.13 |   636.7188 |     - |     - |  2581.34 KB |
|       ObjectPoolParallel | .NET Core 3.0 |          10 |     1000 |   10000 |  13.437 ms |  0.6758 ms | 0.0370 ms |  11.03 |    0.59 |  1515.6250 |     - |     - |  6251.83 KB |
| RecyclableStreamParallel | .NET Core 3.0 |          10 |     1000 |   10000 | 160.428 ms | 10.2610 ms | 0.5624 ms | 131.64 |    7.13 | 10500.0000 |     - |     - |  43105.3 KB |
