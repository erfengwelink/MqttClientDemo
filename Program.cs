using Serilog;
using Serilog.Sinks.File;
using System;
using System.Windows.Forms;

namespace MqttClientDemo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 配置Serilog日志
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs\\mqtt_client.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("应用程序启动");
                // 配置应用程序的视觉样式
                ApplicationConfiguration.Initialize();
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "应用程序崩溃");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}