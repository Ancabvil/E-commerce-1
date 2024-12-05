using Microsoft.AspNetCore.Mvc;
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Model;
using TuringClothes.Repository;


namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;

        public OrderController (UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("CreateOrder")]
        public async Task<Order> CreateOrder(OrderDto orderDto)
        {
            var order = await _unitOfWork.OrderRepository.CreateOrder(orderDto.OrderId, orderDto.PaymentMethod, orderDto.Status, orderDto.Total, orderDto.Email);
            return order;
        }

        [HttpGet("GetOrderById")]
        public async Task<Order> GetOrderById(long orderId)
        {
            return await _unitOfWork.OrderRepository.GetOrderById(orderId);
        }

    }
}
