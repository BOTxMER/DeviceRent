using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DeviceRent.Data;

[Authorize]
[ApiController]
[Route("api/avatar")]
public class AvatarController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ApplicationContext _db;

    public AvatarController(IWebHostEnvironment env, ApplicationContext db)
    {
        _env = env;
        _db = db;
    }
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile avatar)
    {
        try
        {
            if (avatar == null) return BadRequest("Нет файла");

            var userIdClaim = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized("Нет userId");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Некорректный userId");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound("Пользователь не найден");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
            var avatarsDir = Path.Combine(_env.ContentRootPath, "wwwroot", "avatars");
            Directory.CreateDirectory(avatarsDir);
            var path = Path.Combine(avatarsDir, fileName);

            using (var stream = System.IO.File.Create(path))
                await avatar.CopyToAsync(stream);

            user.AvatarPath = $"/avatars/{fileName}";

            try
            {
                await _db.SaveChangesAsync();
                Console.WriteLine("Сохранение прошло успешно");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"DbUpdateException: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, "Ошибка сохранения: " + ex.InnerException?.Message ?? ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Ошибка: " + ex.Message);
            }

            return Ok(new { avatarUrl = user.AvatarPath });


            return Ok(new { avatarUrl = user.AvatarPath });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка Upload: {ex}");
            return StatusCode(500, ex.Message);
        }
    }



    [HttpGet("me")]
    public async Task<IActionResult> GetMyAvatar()
    {
        var userId = int.Parse(User.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        return Ok(new { avatarUrl = user.AvatarPath ?? "" });
    }

}
