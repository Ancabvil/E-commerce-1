using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Model;
using TuringClothes.Repository;

namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemporaryOrderController : ControllerBase
    {

        private readonly UnitOfWork _unitOfWork;

        public TemporaryOrderController(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpPost("CreateTemporaryOrder")]
        public async Task<ActionResult<TemporaryOrder>> CreateTemporaryOrder([FromBody] ICollection<OrderDetailDto> orderDetailsDto)
        {
            if (orderDetailsDto == null || !orderDetailsDto.Any())
            {
                return BadRequest("Order details cannot be null or empty.");
            }

            var userId = User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var userIdLong))
            {
                return Unauthorized("Invalid user ID.");
            }

            var temporaryOrder = await _unitOfWork.TemporaryOrderRepository.CreateTemporaryOrder(orderDetailsDto, userIdLong);

            return Ok(temporaryOrder);
        }

        [HttpGet("TemporaryOrder")]
        public async Task<ActionResult<TemporaryOrder>> GetTemporaryOrder(long id)
        {
            var temporaryOrder = _unitOfWork.TemporaryOrderRepository.GetTemporaryOrder(id);
            return Ok(temporaryOrder);
        }

        [Authorize]
        [HttpPost("RefreshTemporaryOrders")]
        public async Task<ActionResult> RefreshTemporaryOrders(long temporaryOrderId)
        {
            var temporaryOrder = await _unitOfWork.TemporaryOrderRepository.GetTemporaryOrder(temporaryOrderId);
            if (temporaryOrder == null)
            {
                return NotFound("Temporary order no existe.");
            }
            temporaryOrder.ExpirationTime = DateTime.UtcNow.AddMinutes(1);
            await _unitOfWork.SaveChangesAsync();
            return Ok("Temporary order expiration extendida.");
        }
    }
}
