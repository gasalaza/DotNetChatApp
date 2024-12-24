using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ChatClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var hubConfig = config.GetSection("SignalR");
            string hubUrl = hubConfig["HubUrl"];
            string accessToken = hubConfig["AccessToken"];

            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .WithAutomaticReconnect()
                .Build();

            // Manejar mensajes recibidos
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"[{user}]: {message}");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connected to hub Chat");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error to hub Chat: {ex.Message}");
                return;
            }

            Console.Write("Type name of room to join ");
            string chatRoom = Console.ReadLine();
            await connection.InvokeAsync("JoinRoom", chatRoom);

            Console.WriteLine("Start sending messages Type '/exit' to  exit");
            while (true)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "/exit")
                {
                    await connection.InvokeAsync("LeaveRoom", chatRoom);
                    break;
                }

                await connection.InvokeAsync("SendMessage", chatRoom, message);
            }

            await connection.StopAsync();
            Console.WriteLine("Disconnected.");
        }
    }
}
