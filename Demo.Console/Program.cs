using System;
using System.Diagnostics;

namespace Demo.Console
{
    public class Program
    {
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
}


