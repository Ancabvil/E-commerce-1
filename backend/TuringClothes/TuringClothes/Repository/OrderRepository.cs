
using Microsoft.EntityFrameworkCore;
using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public class OrderRepository
    {
        private readonly MyDatabase _myDatabase;

        public OrderRepository(MyDatabase myDatabase)
        {
            _myDatabase = myDatabase;
        }

        public async Task<Order> CreateOrder(long id, string paymentMethod, string? status, long total, string email)

        {
            var temporaryOrder = await _myDatabase.TemporaryOrders.Include(d => d.Details).ThenInclude(p => p.Product).FirstOrDefaultAsync(t => t.Id == id);
            Console.WriteLine(temporaryOrder.Id);

            if (temporaryOrder == null)
            {
                throw new Exception($"La orden temporal {id} no existe.");
            }

            if (temporaryOrder.Details == null || !temporaryOrder.Details.Any())
            {
                throw new Exception($"La orden temporal {id} no tiene detalles.");
            }

            try
            {
                var newOrder = new Order
                {
                    UserId = temporaryOrder.UserId,
                    PaymentMethod = paymentMethod,
                    TransactionStatus = paymentMethod == "card" ? status : null,
                    TotalPrice = total,
                    Email = email,
                    OrderDetails = new List<OrderDetail>()
                };
                _myDatabase.Orders.Add(newOrder);

                foreach (var temporaryOrderDetail in temporaryOrder.Details)
                {
                    var newOrderDetail = new OrderDetail
                    {
                        OrderId = newOrder.Id,
                        Product = temporaryOrderDetail.Product,
                        ProductId = temporaryOrderDetail.ProductID,
                        Amount = temporaryOrderDetail.Amount,
                        ProductPrice = temporaryOrderDetail.Product.Price,
                    };
                    newOrder.OrderDetails.Add(newOrderDetail);
                }
                await _myDatabase.SaveChangesAsync();
                return newOrder;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando la orden: {ex.Message}", ex);
            }


        }

        public async Task<Order> GetOrderById(long orderId)
        {
            await _myDatabase.SaveChangesAsync();
            return await _myDatabase.Orders.Include(p => p.OrderDetails).ThenInclude(p => p.Product).FirstOrDefaultAsync(o => o.Id == orderId);

        }

        internal async Task CreateOrder(long temporaryOrderId, string paymentMethod, int totalPriceEur, string email)
        {
            throw new NotImplementedException();
        }
    }
}
