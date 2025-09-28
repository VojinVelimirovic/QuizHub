using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuizHub.Services.DTOs.LiveRoom;
using QuizHub.Services.Interfaces;
using System.Security.Claims;

namespace QuizHub.Api.Controllers
{
    [ApiController]
    [Route("api/liverooms")]
    [Authorize]
    public class LiveRoomController : ControllerBase
    {
        private readonly ILiveRoomService _liveRoomService;
        private readonly ILogger<LiveRoomController> _logger;
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        public LiveRoomController(ILiveRoomService liveRoomService, ILogger<LiveRoomController> logger)
        {
            _liveRoomService = liveRoomService;
            _logger = logger;
        }

        private string GetUsername() => User.FindFirstValue(ClaimTypes.Name)!;

        /// <summary>
        /// Get all currently active rooms that haven’t started yet.
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveRooms()
        {
            var rooms = await _liveRoomService.GetActiveRoomsAsync();
            return Ok(rooms);
        }

        /// <summary>
        /// Create a new live room.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] LiveRoomCreateDto dto)
        {
            var userId = GetUserId();
            var room = await _liveRoomService.CreateRoomAsync(dto, userId);
            return Ok(room);
        }

        /// <summary>
        /// Get the lobby status of a room for the current user.
        /// </summary>
        [HttpGet("{roomCode}/lobby")]
        public async Task<IActionResult> GetLobby(string roomCode)
        {
            var userId = GetUserId(); ;
            var lobby = await _liveRoomService.GetLobbyStatusAsync(roomCode, userId);
            return Ok(lobby);
        }

        /// <summary>
        /// Join a room via API (optional, mostly for non-SignalR clients/testing)
        /// </summary>
        [HttpPost("{roomCode}/join")]
        public async Task<IActionResult> JoinRoom(string roomCode)
        {
            var userId = GetUserId();
            try
            {
                _logger.LogInformation("User {UserId} attempting to join room {RoomCode}", userId, roomCode);

                var lobby = await _liveRoomService.JoinRoomAsync(roomCode, userId);

                _logger.LogInformation("User {UserId} successfully joined room {RoomCode}", userId, roomCode);
                return Ok(lobby);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("User {UserId} failed to join room {RoomCode}: {ErrorMessage}", userId, roomCode, ex.Message);
                return BadRequest(new { error = ex.Message, code = "JOIN_FAILED" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when user {UserId} tried to join room {RoomCode}", userId, roomCode);
                return StatusCode(500, new { error = "An unexpected error occurred", code = "SERVER_ERROR" });
            }
        }

        /// <summary>
        /// Start a room (host only)
        /// </summary>
        [HttpPost("{roomCode}/start")]
        public async Task<IActionResult> StartRoom(string roomCode)
        {
            var userId = GetUserId();
            try
            {
                var success = await _liveRoomService.StartRoomAsync(roomCode, userId);
                return Ok(new { success });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting room {RoomCode}", roomCode);
                return StatusCode(500, new { error = "An error occurred while starting the room" });
            }
        }


        /// <summary>
        /// Leave a room via API (optional, mostly for non-SignalR clients/testing)
        /// </summary>
        [HttpPost("{roomCode}/leave")]
        public async Task<IActionResult> LeaveRoom(string roomCode)
        {
            var userId = GetUserId();
            var success = await _liveRoomService.LeaveRoomAsync(roomCode, userId);
            return Ok(new { success });
        }

        /// <summary>
        /// Forcefully end a room (admin/host).
        /// </summary>
        [HttpPost("{roomCode}/end")]
        public async Task<IActionResult> EndRoom(string roomCode)
        {
            var userId = GetUserId();
            var room = await _liveRoomService.GetLobbyStatusAsync(roomCode, userId);

            // Only host can end
            if (!room.Players.Any() || room.Players.First().UserId != userId)
                return Forbid("Only the host can end the room");

            var success = await _liveRoomService.EndRoomAsync(roomCode);
            return Ok(new { success });
        }
    }
}
