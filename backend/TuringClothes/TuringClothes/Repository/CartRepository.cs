using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuringClothes.Controllers;
using TuringClothes.Database;
using TuringClothes.Model;


namespace TuringClothes.Repository
{
    public class CartRepository
    {
        private readonly MyDatabase _myDatabase;
        public CartRepository(MyDatabase myDatabase)
        {
            _myDatabase = myDatabase;
        }

        public async Task<Cart> GetCart(long id)
        {
            var cart = await _myDatabase.Carts.Include(d => d.Details).ThenInclude(p => p.Product).FirstOrDefaultAsync(u => u.UserId == id);

            return cart;
        }

        public async Task<Cart> AddItemToCart(long productId, long userId, int quantity)
        {
            var cart = await GetCart(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Details = new List<CartDetail>()
                };
                _myDatabase.Carts.Add(cart);
            }
            var existingDetails = cart.Details.FirstOrDefault(d => d.ProductId == productId);
            Product product = await _myDatabase.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (existingDetails != null)
            {
                existingDetails.Amount += quantity;
            }
            else
            {
                var cartDetails = new CartDetail
                {
                    ProductId = productId,
                    Amount = quantity,
                    Product = product
                };
                cart.Details.Add(cartDetails);
            }

            await _myDatabase.SaveChangesAsync();

            return cart;
        }

        public async Task<Cart> RemoveItemFromCart(long productId, long userId)
        {
            var cart = await GetCart(userId);

            var cartDetails = cart.Details.FirstOrDefault(d => d.ProductId == productId);
            if (cartDetails != null)
            {
                cart.Details.Remove(cartDetails);

            }

            await _myDatabase.SaveChangesAsync();
            return cart;
        }

        public async Task<bool> UpdateItemInCart(long productId, int amount, long userId)
        {
            var cart = await GetCart(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Details = new List<CartDetail>()
                };
                _myDatabase.Carts.Add(cart);
            }
            Product product = await _myDatabase.Products.FirstOrDefaultAsync(p => p.Id == productId);
            var cartDetails = cart.Details.FirstOrDefault(d => d.ProductId == productId);

            if (cartDetails == null)
            {
                if (amount > 0)
                {
                    var newDetails = new CartDetail
                    {
                        Amount = amount,
                        ProductId = productId,
                    };
                    cart.Details.Add(newDetails);
                }
            }
            else
            {

                /*
                 * ESTO LO HE TENIDO QUE MODIFICAR PARA PODER CONTROLAR QUE NO TE VACILEN CON EL STOCK
                 */

                switch (amount)
                {
                    case 0:

                        cart.Details.Remove(cartDetails);
                        break;

                    case > 0 when amount > product.Stock:

                        cartDetails.Amount = (int)product.Stock;
                        break;

                    default:

                        if (cartDetails.Amount != amount)
                        {
                            cartDetails.Amount = amount;
                        }
                        break;
                }
            }

            await _myDatabase.SaveChangesAsync();

            return true;

        }
    }
}