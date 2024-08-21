using System;
using System.IO;
using System.Text.Json;
namespace Talk.Server
{
    public class Config
    {
        public string ServerAddress { get; set; }
        public int Port { get; set; } // 属性名应与 JSON 中的键一致
    }
    public class Program
    {
        static void Main(string[] args)
        {
            string jsonString = File.ReadAllText("Config.json");
            var ipConfig = JsonSerializer.Deserialize<Config>(jsonString);
            SocketServer socketServer = new SocketServer(ipConfig.ServerAddress, ipConfig.Port);
            socketServer.Start(10);
            Console.WriteLine("Server is running. Press Enter to exit...");
            Console.ReadLine();

            // Optionally, you could add logic here to gracefully shut down the server
            socketServer.CloseAll();

        }
    }
}
