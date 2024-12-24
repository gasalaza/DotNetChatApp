using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Infrastructure.Data;
using ChatApp.Infrastructure.Models;

namespace ChatApp.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Chat/Rooms
    [HttpGet("Rooms")]
    public async Task<IActionResult> GetChatRooms()
    {
        var rooms = await _context.ChatRooms.Select(r => r.Name).ToListAsync();
        return Ok(rooms);
    }

    // POST: api/Chat/Rooms
    [HttpPost("Rooms")]
    public async Task<IActionResult> CreateChatRoom([FromBody] string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
            return BadRequest("Room name cannot be empty.");

        var exists = await _context.ChatRooms.AnyAsync(r => r.Name == roomName);
        if (exists)
            return Conflict("Room already exists.");

        var room = new ChatRoom { Name = roomName };
        _context.ChatRooms.Add(room);
        await _context.SaveChangesAsync();

        return Ok("Room created successfully.");
    }
}
