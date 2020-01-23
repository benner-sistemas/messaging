using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Benner.Messaging.Logger
{
    public class Log
    {
        private static Serilog.Core.Logger _logger;

        static Log()
        {
            ConfigureLog();
        }

        private static void ConfigureLog()
        {
            if (_logger == null)
            {
                var elasticConfig = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("elasticsearch.json")
                    .Build();

                _logger = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:dd/MM/yyy HH:mm:ss} {Level:u3}]: {Message:lj}{NewLine}{Exception}")
                    .ReadFrom.Configuration(elasticConfig)
                    .CreateLogger();
            }
        }

        public static void Information(string message)
        {
            _logger.Information(message);
        }

        public static void Information(string message, params object[] propertyValues)
        {
            _logger.Information(message, propertyValues);
        }

        public static void Error(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }

        public static void Error(Exception exception, string message, params object[] propertyValues)
        {
            _logger.Error(exception, message, propertyValues);
        }
    }
}
