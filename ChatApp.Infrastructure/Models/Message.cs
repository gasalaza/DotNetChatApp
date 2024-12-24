using System;

namespace ChatApp.Infrastructure.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
    public string SenderId { get; set; }
    public User Sender { get; set; }
    public int ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; }
}
