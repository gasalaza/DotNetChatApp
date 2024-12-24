// ChatBot/Program.cs
using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ChatBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build configuration
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var rabbitMQConfig = config.GetSection("RabbitMQ");
            var hubConfig = config.GetSection("SignalR");

            string hostName = rabbitMQConfig["HostName"];
            string queueName = rabbitMQConfig["QueueName"];
            string hubUrl = hubConfig["HubUrl"];
            string accessToken = hubConfig["AccessToken"];

            // Initialize SignalR connection
            var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                })
                .WithAutomaticReconnect()
                .Build();

            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Console.WriteLine($"[{user}]: {message}");
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Bot connected to SignalR Hub.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to SignalR Hub: {ex.Message}");
                return;
            }

            // Initialize RabbitMQ connection
            var factory = new ConnectionFactory() { HostName = hostName };
            using (var connectionRabbit = await factory.CreateConnectionAsync())
            using (var channel = await connectionRabbit.CreateChannelAsync())
            {
                await channel.ExchangeDeclareAsync(exchange: "stock_commands",
                    type: ExchangeType.Fanout);

                // declare a server-named queue
                QueueDeclareOk queueDeclareResult = await channel.QueueDeclareAsync();
                string queue = queueDeclareResult.QueueName;
                await channel.QueueBindAsync(queue: queue, exchange: "logs", routingKey: string.Empty);

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += (model, ea) =>
                {
                    byte[] body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] {message}");
                    return Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queueName, autoAck: true, consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        static string GetStockQuote(string stockCode)
        {
            var random = new Random();
            var price = random.Next(100, 500) + random.NextDouble();
            return price.ToString("F2");
        }
    }
}
