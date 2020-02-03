using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Benner.Messaging.Configuration
{
    public class DirectoryHelper
    {
        public static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.LocalPath).Directory.FullName;
        }
    }
}
