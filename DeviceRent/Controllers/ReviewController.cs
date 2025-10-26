using DeviceRent.Data;
using DeviceRent.DTOs;
using DeviceRent.Models;
using Microsoft.AspNetCore.Mvc;

namespace DeviceRent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private static string _message = "";

        public ReviewController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO dto)
        {
            var order = await _db.Orders.FindAsync(dto.Id);
            if (order == null) return NotFound();

            var review = new Review
            {
                Text = dto.Review,
                Rating = dto.Rating,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                OrderId = order.Id
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            _message += $"Добавлен отзыв к заказу №{order.Id}\n";
            return Ok();
        }
    }
}
