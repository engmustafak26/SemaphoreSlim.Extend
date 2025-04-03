# SemaphoreSlim Extensions for Argument-Based Concurrency Control

## Overview

This library provides custom extension methods for `SemaphoreSlim` to enable concurrent execution control per specific argument passed at runtime. It is useful when you need to limit concurrency based on distinct values, such as user IDs, request types, or any other key.

## Features

- **Argument-Based Concurrency Control**: Ensures that tasks with the same key execute with limited concurrency while allowing unrelated tasks to proceed independently.
- **Thread-Safe Implementation**: Uses `SemaphoreSlim` internally with a dictionary to manage semaphores for different keys.
- **More Readable Code Than Original Semaphore**: Write your code as argument to `SemaphoreSlim` `WaitAsync` method without repeating try/finally block to release it.
- **Asynchronous Support**: Supports `async/await` to avoid blocking threads.
- **Automatic Cleanup**: Removes semaphores when no longer needed to prevent memory leaks.

## Installation

Add the library to your .NET project via NuGet (if published) or include the source files in your solution.

```shell
Install-Package SemaphoreSlim.Extend
```

## Usage

### Basic Example

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;



 public class Program
 {
  // Allow only one concurrent Execution per product_Id, user_Id, any unique argument
     static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);

     const string product1Id = "proudct1";
     const string product2Id = "proudct2";
     public static async Task Main(string[] args)
     {
         System.Console.WriteLine("");
         Print($"Start Time ");
         System.Console.WriteLine("");

         System.Console.WriteLine("                       *********************************************                  ");
         Stopwatch sw = Stopwatch.StartNew();
         var tasks = Enumerable.Range(0, 10).Select(x =>
         {
             if (x % 2 == 0)
             {
                 return semaphoreSlim.WaitAsync(product1Id, async () =>
                 {
                     Print($"ProductId = {product1Id}, {x.ToString()}");
                     await Task.Delay(1000);

                 });
             }
             else
             {

                 return semaphoreSlim.WaitAsync(product2Id, async () =>
                 {

                     Print($"ProductId = {product2Id}, {x.ToString()}");
                     await Task.Delay(1000);
                     return "Dummy Method Output";
                 });
             }
         }).ToArray();
         await Task.WhenAll(tasks);
         sw.Stop();
         Print($"End Time  ");
         System.Console.WriteLine("                       *********************************************                  ");
         System.Console.WriteLine("");

         System.Console.WriteLine($"Elapse Time for 10 requests with 2 different productIds = {sw.Elapsed}");
         System.Console.WriteLine("");



     }


     private static Timer timer = new((data) => System.Console.WriteLine("\r\n----------- Allowed Concurrent Code Execution Per ProductId -----\r\n"), null, 0, 1000);
     private static void Print(string output)
     {
         System.Console.WriteLine(output + " ==> " + DateTime.Now.ToString());
     }

 }
```

## Benefits

- **Improved Performance**: Prevents overloading shared resources while allowing parallel execution.
- **Granular Control**: Limits concurrency at the argument level rather than globally.
- **Scalability**: Suitable for multi-user, multi-tenant, or high-throughput applications.

## Contributions

Contributions, issues, and feature requests are welcome. Please submit them via GitHub Issues.

## License

This project is licensed under the MIT License.
