# Overview

[![NuGet version](https://badge.fury.io/nu/PooledStream.svg)](https://badge.fury.io/nu/PooledStream)

[![Build status](https://ci.appveyor.com/api/projects/status/yhm2jto1ed0q0x0y/branch/master?svg=true)](https://ci.appveyor.com/project/itn3000/pooledstream-kviqp/branch/master)

this library aims to efficient MemoryStream for large data.

**STILL UNDER DEVELOPMENT**

# Usage

You can add reference as [NuGet package](https://www.nuget.org/packages/PooledStream/).
Once you add the reference, you can use PooledStream.PooledMemoryStream.

## Code Examples of PooledMemoryStream

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

## Code examples of PooledMemoryBufferWriter

### Basic

```csharp
// using PooledStream;

// create instance
using var bw = new PooledMemoryBufferWriter<byte>();
var sp = bw.GetSpan(8);
sp.Fill(1);
bw.Advance(8);
// get buffer as Span<byte>, you MUST NOT use it outside of bufferwriter lifetime.
var rsp = bw.ToSpanUnsafe();
/// output "1,1,1,1,1,1,1,1"
Console.WriteLine("{0}", string.Join(',', rsp.ToArray()));
// reset buffer status
bw.Reset();
rsp = bw.ToSpanUnsafe();
// output empty line
Console.WriteLine("{0}", string.Join(',', rsp.ToArray()));
```

### Reusing(Advanced usage)

```csharp
// using Microsoft.Extensions.ObjectPool;
// using PooledStream;

// initializing object pool instance.This should be singleton.
// PooledMemoryBufferWriter is not threadsafe.
ObjectPool<PooledMemoryBufferWriter> pool = ObjectPool.Create<PooledMemoryBufferWriter>();
var bw = pool.Get();
try
{
  // initializing buffer
  bw.Reset();
  // using buffer writer...
  ...
}
finally
{
  // returning instance
  pool.Return(bw);
}
```

* you MUST initialize buffer by `Reset()` at first
* you MUST return instance to ObjectPool after using it

# Micro benchmark result(powered by [BenchmarkDotNet](http://benchmarkdotnet.org/))

## Comparison of single thread performance

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1466 (21H2)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-CJIONZ : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-ERJOQR : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT

IterationCount=3  WarmupCount=3  

```
|               Method |        Job |     Toolchain | DataSize | MaxLoop |        Mean |        Error |    StdDev | Ratio | RatioSD |      Gen 0 |      Gen 1 |   Gen 2 |  Allocated |
|--------------------- |----------- |-------------- |--------- |-------- |------------:|-------------:|----------:|------:|--------:|-----------:|-----------:|--------:|-----------:|
|     **NormalStreamTest** | **Job-CJIONZ** |      **.NET 6.0** |      **100** |   **10000** |    **352.1 μs** |    **260.08 μs** |  **14.26 μs** |  **1.00** |    **0.00** |   **548.3398** |     **0.4883** |       **-** |   **3,360 KB** |
|    PooledStreamBench | Job-CJIONZ |      .NET 6.0 |      100 |   10000 |    362.2 μs |     61.72 μs |   3.38 μs |  1.03 |    0.03 |    88.8672 |          - |       - |     547 KB |
| RecyclableStreamTest | Job-CJIONZ |      .NET 6.0 |      100 |   10000 | 12,224.8 μs |    283.90 μs |  15.56 μs | 34.76 |    1.36 |   687.5000 |    46.8750 | 31.2500 |   4,353 KB |
|       ObjectPoolTest | Job-CJIONZ |      .NET 6.0 |      100 |   10000 |    754.0 μs |    562.71 μs |  30.84 μs |  2.15 |    0.17 |   101.5625 |          - |       - |     625 KB |
|                      |            |               |          |         |             |              |           |       |         |            |            |         |            |
|     NormalStreamTest | Job-ERJOQR | .NET Core 3.1 |      100 |   10000 |    388.8 μs |    198.45 μs |  10.88 μs |  1.00 |    0.00 |   561.0352 |     0.9766 |       - |   3,438 KB |
|    PooledStreamBench | Job-ERJOQR | .NET Core 3.1 |      100 |   10000 |    403.8 μs |     76.37 μs |   4.19 μs |  1.04 |    0.04 |   101.5625 |          - |       - |     625 KB |
| RecyclableStreamTest | Job-ERJOQR | .NET Core 3.1 |      100 |   10000 | 12,498.6 μs |  1,357.66 μs |  74.42 μs | 32.16 |    0.71 |   703.1250 |    46.8750 | 31.2500 |   4,431 KB |
|       ObjectPoolTest | Job-ERJOQR | .NET Core 3.1 |      100 |   10000 |    785.7 μs |    179.92 μs |   9.86 μs |  2.02 |    0.08 |   101.5625 |          - |       - |     625 KB |
|                      |            |               |          |         |             |              |           |       |         |            |            |         |            |
|     **NormalStreamTest** | **Job-CJIONZ** |      **.NET 6.0** |     **1000** |   **10000** |    **732.8 μs** |    **264.67 μs** |  **14.51 μs** |  **1.00** |    **0.00** |  **1734.3750** |    **10.7422** |       **-** |  **10,626 KB** |
|    PooledStreamBench | Job-CJIONZ |      .NET 6.0 |     1000 |   10000 |    513.3 μs |    465.38 μs |  25.51 μs |  0.70 |    0.05 |    88.8672 |          - |       - |     548 KB |
| RecyclableStreamTest | Job-CJIONZ |      .NET 6.0 |     1000 |   10000 | 11,926.3 μs |    816.69 μs |  44.77 μs | 16.28 |    0.33 |   687.5000 |    46.8750 | 31.2500 |   4,354 KB |
|       ObjectPoolTest | Job-CJIONZ |      .NET 6.0 |     1000 |   10000 |    839.0 μs |    191.49 μs |  10.50 μs |  1.15 |    0.03 |   101.5625 |          - |       - |     626 KB |
|                      |            |               |          |         |             |              |           |       |         |            |            |         |            |
|     NormalStreamTest | Job-ERJOQR | .NET Core 3.1 |     1000 |   10000 |    769.3 μs |     81.86 μs |   4.49 μs |  1.00 |    0.00 |  1747.0703 |    10.7422 |       - |  10,704 KB |
|    PooledStreamBench | Job-ERJOQR | .NET Core 3.1 |     1000 |   10000 |    586.8 μs |    495.79 μs |  27.18 μs |  0.76 |    0.04 |   101.5625 |          - |       - |     626 KB |
| RecyclableStreamTest | Job-ERJOQR | .NET Core 3.1 |     1000 |   10000 | 12,806.1 μs |  2,418.51 μs | 132.57 μs | 16.65 |    0.26 |   703.1250 |    46.8750 | 31.2500 |   4,432 KB |
|       ObjectPoolTest | Job-ERJOQR | .NET Core 3.1 |     1000 |   10000 |    914.8 μs |     73.68 μs |   4.04 μs |  1.19 |    0.01 |   101.5625 |          - |       - |     626 KB |
|                      |            |               |          |         |             |              |           |       |         |            |            |         |            |
|     **NormalStreamTest** | **Job-CJIONZ** |      **.NET 6.0** |    **50000** |   **10000** | **27,280.8 μs** |  **2,486.59 μs** | **136.30 μs** |  **1.00** |    **0.00** | **79312.5000** | **10312.5000** |       **-** | **489,190 KB** |
|    PooledStreamBench | Job-CJIONZ |      .NET 6.0 |    50000 |   10000 |  9,201.1 μs |  2,050.28 μs | 112.38 μs |  0.34 |    0.01 |    93.7500 |          - |       - |     596 KB |
| RecyclableStreamTest | Job-CJIONZ |      .NET 6.0 |    50000 |   10000 | 22,549.4 μs |  7,507.94 μs | 411.54 μs |  0.83 |    0.01 |   687.5000 |   125.0000 | 31.2500 |   4,402 KB |
|       ObjectPoolTest | Job-CJIONZ |      .NET 6.0 |    50000 |   10000 | 10,864.5 μs |  1,082.43 μs |  59.33 μs |  0.40 |    0.00 |   109.3750 |    15.6250 |       - |     674 KB |
|                      |            |               |          |         |             |              |           |       |         |            |            |         |            |
|     NormalStreamTest | Job-ERJOQR | .NET Core 3.1 |    50000 |   10000 | 27,684.2 μs |  2,307.66 μs | 126.49 μs |  1.00 |    0.00 | 79343.7500 | 10437.5000 | 31.2500 | 489,268 KB |
|    PooledStreamBench | Job-ERJOQR | .NET Core 3.1 |    50000 |   10000 |  9,232.5 μs |  2,070.70 μs | 113.50 μs |  0.33 |    0.01 |   109.3750 |    15.6250 |       - |     674 KB |
| RecyclableStreamTest | Job-ERJOQR | .NET Core 3.1 |    50000 |   10000 | 23,480.2 μs | 13,114.22 μs | 718.83 μs |  0.85 |    0.03 |   687.5000 |   125.0000 | 31.2500 |   4,480 KB |
|       ObjectPoolTest | Job-ERJOQR | .NET Core 3.1 |    50000 |   10000 | 10,904.2 μs |  1,730.91 μs |  94.88 μs |  0.39 |    0.00 |   109.3750 |    15.6250 |       - |     674 KB |

## Comparison of multithreaded performance

``` ini

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1466 (21H2)
Intel Core i7-9700 CPU 3.00GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-DVURFQ : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-ZKBPJE : .NET Core 3.1.22 (CoreCLR 4.700.21.56803, CoreFX 4.700.21.57101), X64 RyuJIT

IterationCount=3  WarmupCount=3  

```
|                   Method |        Job |     Toolchain | ParallelNum | DataSize | MaxLoop |         Mean |       Error |    StdDev |  Ratio | RatioSD |     Gen 0 |   Gen 1 | Allocated |
|------------------------- |----------- |-------------- |------------ |--------- |-------- |-------------:|------------:|----------:|-------:|--------:|----------:|--------:|----------:|
|     **NormalStreamParallel** | **Job-DVURFQ** |      **.NET 6.0** |           **5** |     **1000** |   **10000** |     **832.3 μs** |    **91.85 μs** |   **5.03 μs** |   **1.00** |    **0.00** | **1734.3750** | **11.7188** |     **10 MB** |
|     PooledStreamParallel | Job-DVURFQ |      .NET 6.0 |           5 |     1000 |   10000 |   1,461.6 μs | 1,099.12 μs |  60.25 μs |   1.76 |    0.08 |  410.1563 |  3.9063 |      2 MB |
|       ObjectPoolParallel | Job-DVURFQ |      .NET 6.0 |           5 |     1000 |   10000 |   4,915.4 μs | 1,044.02 μs |  57.23 μs |   5.91 |    0.10 |  507.8125 |       - |      3 MB |
| RecyclableStreamParallel | Job-DVURFQ |      .NET 6.0 |           5 |     1000 |   10000 |  59,377.8 μs | 2,245.30 μs | 123.07 μs |  71.35 |    0.39 | 3444.4444 |       - |     21 MB |
|                          |            |               |             |          |         |              |             |           |        |         |           |         |           |
|     NormalStreamParallel | Job-ZKBPJE | .NET Core 3.1 |           5 |     1000 |   10000 |     883.1 μs |   308.71 μs |  16.92 μs |   1.00 |    0.00 | 1747.0703 | 11.7188 |     10 MB |
|     PooledStreamParallel | Job-ZKBPJE | .NET Core 3.1 |           5 |     1000 |   10000 |   2,520.1 μs |   982.67 μs |  53.86 μs |   2.85 |    0.07 |  421.8750 |  3.9063 |      3 MB |
|       ObjectPoolParallel | Job-ZKBPJE | .NET Core 3.1 |           5 |     1000 |   10000 |   5,142.7 μs |   271.51 μs |  14.88 μs |   5.82 |    0.10 |  507.8125 |       - |      3 MB |
| RecyclableStreamParallel | Job-ZKBPJE | .NET Core 3.1 |           5 |     1000 |   10000 |  63,350.5 μs | 3,453.86 μs | 189.32 μs |  71.75 |    1.56 | 3500.0000 |       - |     21 MB |
|                          |            |               |             |          |         |              |             |           |        |         |           |         |           |
|     **NormalStreamParallel** | **Job-DVURFQ** |      **.NET 6.0** |          **10** |     **1000** |   **10000** |     **831.3 μs** |   **318.09 μs** |  **17.44 μs** |   **1.00** |    **0.00** | **1734.3750** |  **9.7656** |     **10 MB** |
|     PooledStreamParallel | Job-DVURFQ |      .NET 6.0 |          10 |     1000 |   10000 |   1,528.2 μs | 2,194.38 μs | 120.28 μs |   1.84 |    0.11 |  410.1563 |  5.8594 |      2 MB |
|       ObjectPoolParallel | Job-DVURFQ |      .NET 6.0 |          10 |     1000 |   10000 |   9,440.5 μs |   984.44 μs |  53.96 μs |  11.36 |    0.30 | 1015.6250 |       - |      6 MB |
| RecyclableStreamParallel | Job-DVURFQ |      .NET 6.0 |          10 |     1000 |   10000 | 118,121.0 μs | 4,079.13 μs | 223.59 μs | 142.14 |    2.73 | 6800.0000 |       - |     41 MB |
|                          |            |               |             |          |         |              |             |           |        |         |           |         |           |
|     NormalStreamParallel | Job-ZKBPJE | .NET Core 3.1 |          10 |     1000 |   10000 |     839.9 μs |    91.10 μs |   4.99 μs |   1.00 |    0.00 | 1747.0703 | 11.7188 |     10 MB |
|     PooledStreamParallel | Job-ZKBPJE | .NET Core 3.1 |          10 |     1000 |   10000 |   2,078.9 μs |    72.45 μs |   3.97 μs |   2.48 |    0.01 |  421.8750 |  3.9063 |      3 MB |
|       ObjectPoolParallel | Job-ZKBPJE | .NET Core 3.1 |          10 |     1000 |   10000 |  11,123.8 μs | 5,348.25 μs | 293.16 μs |  13.24 |    0.32 | 1015.6250 |       - |      6 MB |
| RecyclableStreamParallel | Job-ZKBPJE | .NET Core 3.1 |          10 |     1000 |   10000 | 126,562.0 μs | 6,505.48 μs | 356.59 μs | 150.68 |    0.85 | 7000.0000 |       - |     42 MB |
