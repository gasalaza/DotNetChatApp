using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatApp.Infrastructure.Data;
using ChatApp.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ChatHub(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SendMessage(string chatRoom, string message)
        {
            var userName = Context?.User?.Identity?.Name ?? "UnknownUser";

            if (message.StartsWith("/"))
            {
                // Example: the command is "/stock=AMZN"
                var commandParts = message.Split('=', 2);
                if (commandParts.Length == 2)
                {
                    var command = commandParts[0].ToLower();
                    var parameter = commandParts[1];

                    if (command == "/stock")
                    {
                        // Publish to RabbitMQ
                        PublishToRabbitMQAsync(chatRoom, userName, parameter);

                        // Notify the caller
                        await Clients.Caller.SendAsync("ReceiveMessage", "System",
                            $"Processing stock command for: {parameter}");
                        return;
                    }
                }

                // Command not recognized
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "Unrecognized command.");
                return;
            }

            // Save the message to the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            if (user == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "User not found.");
                return;
            }

            var chatRoomEntity = await _context.ChatRooms.Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Name == chatRoom);

            if (chatRoomEntity == null)
            {
                chatRoomEntity = new ChatRoom { Name = chatRoom };
                _context.ChatRooms.Add(chatRoomEntity);
            }

            var chatMessage = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                SenderId = user.Id,
                ChatRoom = chatRoomEntity
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Broadcast the message to the group
            await Clients.Group(chatRoom).SendAsync("ReceiveMessage", userName, message);

            // Otherwise, handle normal chat message
            // (for example, save to DB then broadcast)
        }

        private async Task PublishToRabbitMQAsync(string chatRoom, string userName, string stockCode)
        {
            // Minimal, synchronous approach
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };


            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(exchange: "stock_commands", type: ExchangeType.Fanout);

            var message = $"{chatRoom} {userName} {stockCode}";
            var body = Encoding.UTF8.GetBytes(message);
            await channel.BasicPublishAsync(exchange: "logs", routingKey: string.Empty, body: body);
        }

        public async Task JoinRoom(string chatRoom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatRoom);

            // Load last 50 messages from DB as an example
            var messages = await _context.Messages
                .Where(m => m.ChatRoom.Name == chatRoom)
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();

            foreach (var msg in messages)
            {
                var sender = await _context.Users.FindAsync(msg.SenderId);
                await Clients.Caller.SendAsync("ReceiveMessage", sender?.UserName ?? "UnknownUser", msg.Content);
            }

            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"You have joined the chat room: {chatRoom}");
        }

        public async Task LeaveRoom(string chatRoom)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatRoom);
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"You have left the chat room: {chatRoom}");
        }
    }
}
