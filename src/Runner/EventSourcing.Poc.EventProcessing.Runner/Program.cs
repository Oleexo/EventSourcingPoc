using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace EventSourcing.Poc.EventProcessing.Runner {
    public class Program {
        private static void Main(string[] args) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.Development.json", true);
            var runner = new EventRunner(builder.Build());
            runner.Run();
            Console.WriteLine("Press Key to exit.");
            Console.ReadKey();
        }
    }
}