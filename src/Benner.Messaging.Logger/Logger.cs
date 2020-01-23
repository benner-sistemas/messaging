using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Benner.Messaging.Logger
{
    public class Log
    {
        private const string FILE_NAME = "elasticsearch.json";
        private static Serilog.Core.Logger _logger;

        public static void ConfigureLog()
        {
            if (_logger == null)
            {
                var config = new LoggerConfiguration()
                    .WriteTo.Console(outputTemplate: "[{Timestamp:dd/MM/yyy HH:mm:ss} {Level:u3}]: {Message:lj}{NewLine}{Exception}");

                string basePath = Directory.GetCurrentDirectory();
                if (File.Exists(Path.Combine(basePath, FILE_NAME)))
                    config.ReadFrom.Configuration(new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile(FILE_NAME)
                        .Build());

                _logger = config.CreateLogger();
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
