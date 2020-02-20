using System;
using System.Reflection;

namespace ESocket.Server.Launcher
{
    class ServerLauncher
    {
        static void Main(string[] args)
        {
            try
            {
                //初始化配置
                var config = InitConfig();
                //初始化Application
                InitApplication(config);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("程序异常...");
                Console.ReadKey();
            }
            Console.WriteLine("Press 'exit' to exit...");
            string str;
            while (true)
            {
                str = Console.ReadLine();
                if (str == "exit")
                    return;
            }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        private static Config InitConfig()
        {
            var config = Config.LoadConfig();
            if (config == null)
            {
                throw new Exception("Config Exception!");
            }
            return config;
        }

        /// <summary>
        /// 初始化Application
        /// </summary>
        private static void InitApplication(Config config)
        {
            try
            {
                string assemblyFileName = config.ApplicationConfig.AssemblyFileName;
                if (!assemblyFileName.EndsWith(".dll"))
                    assemblyFileName += ".dll";
                ApplicationBase application = (Activator.CreateInstance(Assembly.LoadFrom(System.IO.Path.Combine(
                    "..\\" + config.ApplicationConfig.BaseDirectory, assemblyFileName))
                    .GetType(config.ApplicationConfig.TypeWithNamespace), config) as ApplicationBase);

                Console.WriteLine("{0} Start...", application.GetType());
                application.Start();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
