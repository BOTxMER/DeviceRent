var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

app.UseCors(option => option
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader());

List<Order> repo = new List<Order>
{
    new Order(1, new DateOnly(2008, 6, 21), "keyrox tkl", 24, "Джесси Пинкман", "В ожидании доставки", "ул. Сваговская", "8800553535"),
    new Order(2, new DateOnly(2007, 2, 14), "Razer barakuda x", 12, "Волтер Вайт", "В ожидании доставки", "ул. Синие кириешки 37", "1234567890")
};

string message = "";

app.MapGet("/orders", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Id == param), message = buffer };
    return new { repo, message = buffer };
});

app.MapGet("/create", ([AsParameters] Order dto) => 
{
    repo.Add(dto);
    return Results.Ok();
});

app.MapGet("/update", ([AsParameters] UpdateOrderDTO dto) =>
{
    var order = repo.Find(x => x.Id == dto.Id);
    if (order == null)
        return Results.NotFound();
        
    if (dto.Status != order.Status && dto.Status != "")
    {
        order.Status = dto.Status;
        message += $"Статус заказа №{order.Id} изменен\n";
        if (order.Status == "Доставлено")
        {
            message += $"Заявка №{order.Id} завершена\n";
            order.EndDate = DateOnly.FromDateTime(DateTime.Now);
        }
    }
    if (dto.Diller != "")
        order.Diller = dto.Diller;
    if (dto.Comment != "")
        order.Comments.Add(dto.Comment);
        
    return Results.Ok();
});

app.MapGet("/add-review", ([AsParameters] ReviewDTO dto) =>
{
    var order = repo.Find(x => x.Id == dto.Id);
    if (order == null)
        return Results.NotFound();
    
    order.Review = new Review
    {
        Text = dto.Review,
        Rating = dto.Rating,
        Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
    };
    
    message += $"Добавлен отзыв к заказу №{order.Id}\n";
    return Results.Ok();
});

app.MapGet("/statistics", () => 
{
    int complete_count() => repo.FindAll(x => x.Status == "Доставлено").Count;
    
    Dictionary<string, int> get_device_rent_stat() =>
        repo.GroupBy(x => x.DeviceModel)
           .ToDictionary(g => g.Key, g => g.Count());
    
    double get_average_time_to_complete()
    {
        var completedOrders = repo.FindAll(x => x.Status == "Доставлено" && x.EndDate.HasValue);
        if (completedOrders.Count == 0) return 0;
        
        return completedOrders
            .Average(x => x.EndDate!.Value.DayNumber - x.StartDate.DayNumber);
    }

    return new 
    {
        complete_count = complete_count(),
        device_rent_stat = get_device_rent_stat(),
        average_time_to_complete = get_average_time_to_complete()
    };
});

app.Run();

public class Order
{
    public Order(int id, DateOnly startDate, string deviceModel, int hour, string client, string status, string deliveryAddress, string phoneNumber)
    {
        Id = id;
        StartDate = startDate;
        DeviceModel = deviceModel;
        Hour = hour;
        Client = client;
        Status = status;
        DeliveryAddress = deliveryAddress;
        PhoneNumber = phoneNumber;
        Comments = new List<string>();
    }

    public int Id { get; set; }
    public DateOnly StartDate { get; set; }
    public string DeviceModel { get; set; }
    public int Hour { get; set; }
    public string Client { get; set; }
    public string Status { get; set; }
    public DateOnly? EndDate { get; set; } = null;
    public string? Diller { get; set; } = "Не назначен";
    public string DeliveryAddress { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> Comments { get; set; }
    public Review? Review { get; set; } = null;
}

public record UpdateOrderDTO(int Id, string? Status = "", string? Diller = "", string? Comment = "");

public record ReviewDTO(int Id, string Review, int Rating);

public class Review
{
    public string Text { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Date { get; set; } = string.Empty;
}