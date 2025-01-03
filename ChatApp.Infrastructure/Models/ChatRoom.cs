﻿using System.Collections.Generic;

namespace ChatApp.Infrastructure.Models;

public class ChatRoom
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Message> Messages { get; set; }
}
