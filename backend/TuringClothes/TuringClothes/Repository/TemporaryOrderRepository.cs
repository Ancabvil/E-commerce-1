using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Model;

namespace TuringClothes.Repository
{
    public class TemporaryOrderRepository : Repository<TemporaryOrder, long>
    {

        public TemporaryOrderRepository(MyDatabase myDatabase) : base(myDatabase)
        {
            _myDatabase = myDatabase;
        }

        public async Task<TemporaryOrder> CreateTemporaryOrder(ICollection<OrderDetailDto> orderDetailDto, long userId)
        {
            using var transaction = await _myDatabase.Database.BeginTransactionAsync();
            try
            {

                var temporaryOrder = new TemporaryOrder
                {
                    UserId = userId,
                    HexEthereumPrice = "",
                    Details = new List<TemporaryOrderDetail>()
                };

                _myDatabase.TemporaryOrders.Add(temporaryOrder);

                var noStockProducts = new List<OrderDetailDto>();

                foreach (var orderDetail in orderDetailDto)
                {
                    var product = await _myDatabase.Products.FirstOrDefaultAsync(p => p.Id == orderDetail.ProductId);
                    if (product == null)
                    {
                        throw new Exception($"El producto con la ID {orderDetail.ProductId} no existe.");
                    }

                    if (product.Stock >= orderDetail.Amount)
                    {
                        var newOrderDetail = new TemporaryOrderDetail
                        {
                            ProductID = product.Id,
                            Amount = orderDetail.Amount,
                            TemporaryOrderID = temporaryOrder.Id,
                            Product = product
                        };
                        temporaryOrder.TotalPriceEur += (newOrderDetail.Product.Price * newOrderDetail.Amount);

                        temporaryOrder.Details.Add(newOrderDetail);

                        product.Stock -= orderDetail.Amount;
                        _myDatabase.Products.Update(product);
                    }
                    else
                    {
                        noStockProducts.Add(orderDetail);
                    }


                }

                if (noStockProducts.Any())
                {
                    _myDatabase.Remove(temporaryOrder);
                    await _myDatabase.SaveChangesAsync();

                    var productNames = string.Join(", ", noStockProducts.Select(x => x.ProductId));
                    throw new Exception($"Los siguientes productos no tienen stock: {productNames}");

                }

                await _myDatabase.SaveChangesAsync();
                await transaction.CommitAsync();
                return temporaryOrder;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TemporaryOrder> GetTemporaryOrder(long id)
        {
            return await _myDatabase.TemporaryOrders.Include(x => x.Details)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(n => n.Id == id);
        }
    }
}
