using Benner.Messaging.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Benner.Messaging.CLI.Tests
{
    public class BaseBrokerTests
    {
        protected string ConfigAssemblyName { get; set; }
        protected string GetErrorMessage(string[] args)
        {
            var cliConfig = new CliConfiguration(args);
            cliConfig.Execute();
            return cliConfig.Exception.Message;
        }

        protected TOutput GetPropertyFromType<TOutput>(string propName, IBrokerConfig brokerConfig)
        {
            Type brokerType = Type.GetType(ConfigAssemblyName);

            Assert.AreEqual(brokerType, brokerConfig.GetType());

            PropertyInfo prop = brokerType.GetProperty(propName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return (TOutput)prop.GetValue(brokerConfig);
        }
    }
}
