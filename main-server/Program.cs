using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MainServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseKestrel((context, options) =>
                        {
                            var portEnv = Environment.GetEnvironmentVariable("ASPNETCORE_PORT");

                            if (string.IsNullOrWhiteSpace(portEnv))
                                throw new ArgumentException("Expose port is not defined.");

                            var port = int.Parse(portEnv);
                            options.Listen(IPAddress.Any, port);

                            options.AddServerHeader = false;
                        })
                        .UseStartup<Startup>();
                });
    }
}
