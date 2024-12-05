using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TuringClothes.Database;
using TuringClothes.Model;
using TuringClothes.Repository;

namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class CartController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        public CartController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet("GetCart")]
        public async Task<ActionResult> GetCart()
        {
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                return Unauthorized("Invalid user ID.");
            }

            var cart = await _unitOfWork.CartRepository.GetCart(userIdLong);
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            return Ok(cart);
        }

        [Authorize]
        [HttpPut("AddItem")]
        public async Task<ActionResult> AddItem(long id, int quantity)
        {
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                return Unauthorized("Invalid user ID.");
            }

            await _unitOfWork.CartRepository.AddItemToCart(id, userIdLong, quantity);

            return Ok("Product added");
        }

        [Authorize]
        [HttpDelete("RemoveItem")]
        public async Task<ActionResult> RemoveItem(long id)
        {
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                return Unauthorized("Invalid user ID.");
            }

            await _unitOfWork.CartRepository.RemoveItemFromCart(id, userIdLong);

            return Ok(new { message = "Product Removed" });
        }

        [Authorize]
        [HttpPost("UpdateItem")]
        public async Task<ActionResult> UpdateCart(long id, int amount)
        {
            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                return Unauthorized("Invalid user ID.");
            }

            var result = await _unitOfWork.CartRepository.UpdateItemInCart(id, amount, userIdLong);

            if (!result)
            {
                return BadRequest("Failed to update the cart");
            }

            return Ok(new { message = "Cart updated" });
        }
    }
}
