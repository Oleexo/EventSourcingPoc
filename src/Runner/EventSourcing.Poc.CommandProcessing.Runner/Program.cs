using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EventSourcing.Poc.CommandProcessing.Runner
{
    class Program
    {
        static void Main(string[] args) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Development.json", optional: true);
            var runner = new Runner(builder.Build());
            runner.Run();
            Console.WriteLine("Press Key to exit.");
            Console.ReadKey();
        }
    }
}