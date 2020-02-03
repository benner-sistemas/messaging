using System;
using System.IO;
using System.Linq;
using Benner.Messaging.Configuration;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Benner.Messaging.Logger
{
    public static class Log
    {
        private const string ConsoleTemplate = "[{Timestamp:dd/MM/yyy HH:mm:ss} {Level:u3}]: {Message:lj}{NewLine}{Exception}";
        private static Serilog.Core.Logger _logger;
        private static LoggerConfiguration _loggerConfig = new LoggerConfiguration();
        private static LogJson _configFile;

        public static void ConfigureLog()
        {
            if (_logger == null)
            {
                try
                {
                    // must be configured first so it can log everything
                    ConfigureConsole();

                    LoadConfigFile();
                    if (_configFile != null)
                    {
                        LogEventLevel level = GetLoggingLevel();
                        _loggerConfig.MinimumLevel.Is(level);

                        if (_configFile.EnableElasticSearch)
                            ConfigureElastic();
                    }
                }
                finally
                {
                    _logger = _loggerConfig.CreateLogger();
                }
            }
        }

        private static LogEventLevel GetLoggingLevel()
        {
            if (string.IsNullOrWhiteSpace(_configFile.MinimumLogLevel))
                return LogEventLevel.Information;

            return Enum.Parse<LogEventLevel>(_configFile.MinimumLogLevel, true);
        }

        private static void LoadConfigFile()
        {
            if (_configFile == null)
                _configFile = JsonConfiguration.LoadConfiguration<LogJson>();
        }

        private static void ConfigureConsole()
        {
            _loggerConfig.WriteTo.Console(outputTemplate: ConsoleTemplate);
        }

        private static void ConfigureElastic()
        {
            if (_configFile.ElasticSearch == null)
                throw new Exception($"Propriedade 'ElasticSearch' deve estar configurada no arquivo {new LogJson().FileName}");

            if (_configFile.ElasticSearch?.NodeUris == null || _configFile.ElasticSearch?.NodeUris?.Count == 0)
                throw new Exception($"O arquivo '{new LogJson().FileName}' não possui endereços de ElasticSearch configurados.");

            var esOpts = new ElasticsearchSinkOptions(_configFile.ElasticSearch.GetUris())
            {
                AutoRegisterTemplate = true,
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                RegisterTemplateFailure = RegisterTemplateRecovery.FailSink
            };

            _loggerConfig.WriteTo.Elasticsearch(esOpts);
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
