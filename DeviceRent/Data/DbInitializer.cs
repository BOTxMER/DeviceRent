using Microsoft.EntityFrameworkCore;
using DeviceRent.Models;

namespace DeviceRent.Data
{
    public static class DbInitializer
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Login = "admin", Password = "admin", Role = "admin" },
                new User { Id = 2, Login = "user", Password = "123", Role = "user" }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order
                {
                    Id = 1,
                    StartDate = new DateTime(2008, 6, 21),
                    DeviceModel = "Red Square Keyrox TKL",
                    Hour = 24,
                    Client = "Джесси Пинкман",
                    Status = "В ожидании доставки",
                    DeliveryAddress = "ул. Сваговская",
                    PhoneNumber = "8800553535"
                },
                new Order
                {
                    Id = 2,
                    StartDate = new DateTime(2007, 2, 14),
                    DeviceModel = "Razer Baracuda X",
                    Hour = 12,
                    Client = "Волтер Вайт",
                    Status = "В ожидании доставки",
                    DeliveryAddress = "ул. Синие кириешки 37",
                    PhoneNumber = "1234567890"
                }
            );
        }
    }
}
