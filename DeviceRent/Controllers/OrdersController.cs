using DeviceRent.Data;
using DeviceRent.DTOs;
using DeviceRent.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DeviceRent.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private static string _message = "";

        public OrdersController(ApplicationContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(int param = 0)
        {
            var buffer = _message;
            _message = "";

            IQueryable<Order> query = _db.Orders.Include(o => o.Review);

            if (param != 0)
                query = query.Where(x => x.Id == param);

            var orders = await query.ToListAsync();
            return Ok(new { repo = orders, message = buffer });
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] Order dto)
        {
            _db.Orders.Add(dto);
            await _db.SaveChangesAsync();
            return Ok(new { id = dto.Id });
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateOrderDTO dto)
        {
            var order = await _db.Orders.FindAsync(dto.Id);
            if (order == null) return NotFound();

            if (dto.Status != order.Status && !string.IsNullOrEmpty(dto.Status))
            {
                order.Status = dto.Status;
                _message += $"Статус заказа №{order.Id} изменен\n";

                if (order.Status == "Доставлено")
                {
                    _message += $"Заявка №{order.Id} завершена\n";
                    order.EndDate = DateTime.Now;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Diller))
                order.Diller = dto.Diller;

            if (!string.IsNullOrWhiteSpace(dto.Comment))
                order.Comments.Add(dto.Comment);

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var allOrders = await _db.Orders.ToListAsync();
            int completeCount = allOrders.Count(x => x.Status == "Доставлено");

            var deviceStat = allOrders
                .GroupBy(x => x.DeviceModel)
                .ToDictionary(g => g.Key, g => g.Count());

            double avgTime = allOrders
                .Where(x => x.Status == "Доставлено" && x.EndDate.HasValue)
                .DefaultIfEmpty()
                .Average(x => x == null ? 0 : (x.EndDate!.Value - x.StartDate).TotalDays);

            return Ok(new
            {
                complete_count = completeCount,
                device_rent_stat = deviceStat,
                average_time_to_complete = avgTime
            });
        }
    }
}
