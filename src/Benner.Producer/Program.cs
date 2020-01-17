using Benner.Messaging;
using Benner.Producer.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Benner.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                if (!string.IsNullOrWhiteSpace(e.Message))
                    Console.WriteLine($"ERRO:\r\n   {e.Message}");
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
