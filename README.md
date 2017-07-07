# Overview

this project is library which aims to efficient MemoryStream for large data.

**STILL UNDER DEVELOPMENT**

# Micro benchmark result(powered by [BenchmarkDotNet](http://benchmarkdotnet.org/))

``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 8.1 (6.3.9600)
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3312643 Hz, Resolution=301.8738 ns, Timer=TSC
dotnet cli version=2.0.0-preview2-006497
  [Host]     : .NET Core 4.6.00001.0, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.00001.0, 64bit RyuJIT


```
 |               Method | DataSize | MaxLoop |         Mean |        Error |       StdDev |       Median | Scaled | ScaledSD |       Gen 0 |       Gen 1 |       Gen 2 |    Allocated |
 |--------------------- |--------- |-------- |-------------:|-------------:|-------------:|-------------:|-------:|---------:|------------:|------------:|------------:|-------------:|
 |     **NormalStreamTest** |      **100** |   **10000** |     **530.9 us** |     **3.066 us** |     **2.560 us** |     **531.0 us** |   **1.00** |     **0.00** |    **838.8672** |           **-** |           **-** |   **3437.63 KB** |
 |    PooledStreamBench |      100 |   10000 |     657.3 us |     9.856 us |     8.737 us |     654.9 us |   1.24 |     0.02 |    170.8984 |           - |           - |    703.25 KB |
 | RecyclableStreamTest |      100 |   10000 |  21,652.7 us |   430.904 us |   798.709 us |  21,225.3 us |  40.79 |     1.50 |   1062.5000 |     31.2500 |     31.2500 |   4510.44 KB |
 |     **NormalStreamTest** |     **1000** |   **10000** |   **1,167.9 us** |    **23.153 us** |    **22.739 us** |   **1,160.4 us** |   **1.00** |     **0.00** |   **2611.3281** |           **-** |           **-** |  **10704.13 KB** |
 |    PooledStreamBench |     1000 |   10000 |     854.8 us |     4.704 us |     4.400 us |     855.3 us |   0.73 |     0.01 |    171.8750 |           - |           - |    704.13 KB |
 | RecyclableStreamTest |     1000 |   10000 |  21,670.9 us |   222.349 us |   185.672 us |  21,621.6 us |  18.56 |     0.38 |   1062.5000 |     31.2500 |     31.2500 |   4511.31 KB |
 |     **NormalStreamTest** |   **100000** |   **10000** | **192,641.2 us** | **3,706.515 us** | **3,640.293 us** | **193,404.1 us** |   **1.00** |     **0.00** | **312500.0000** | **312500.0000** | **312500.0000** | **977597.68 KB** |
 |    PooledStreamBench |   100000 |   10000 |  37,751.8 us |   233.532 us |   218.446 us |  37,740.3 us |   0.20 |     0.00 |    125.0000 |           - |           - |     800.8 KB |
 | RecyclableStreamTest |   100000 |   10000 |  58,598.4 us | 1,116.200 us |   932.077 us |  58,607.9 us |   0.30 |     0.01 |   1062.5000 |     62.5000 |     62.5000 |   4607.99 KB |
