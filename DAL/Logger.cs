using DAL.Services;
using log4net;
using System;
using System.IO;

namespace DAL
{
    public class Logger : ILogger
    {
        private static readonly string LOG_CONFIG_FILE = @"log4net.config";

        public Logger(string fileName)
        {
            log4net.GlobalContext.Properties["LogName"] = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
            string executingAssemblyFqPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            executingAssemblyFqPath = (System.IO.Path.Combine(executingAssemblyFqPath, LOG_CONFIG_FILE));
            if (System.IO.File.Exists(executingAssemblyFqPath))
            {
                FileInfo fi = new FileInfo(executingAssemblyFqPath);
                log4net.Config.XmlConfigurator.Configure(fi);
            }
        }

        public ILog GetLogger(Type type)
        {
            return LogManager.GetLogger(type);
        }
        public ILog GetLogger<T>()
        {
            return LogManager.GetLogger(typeof(T));
        }
    }
}
