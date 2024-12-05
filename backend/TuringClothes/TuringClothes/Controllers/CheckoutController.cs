
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TuringClothes.Database;
using TuringClothes.Model;
using TuringClothes.Repository;
using TuringClothes.Dtos;
using TuringClothes.Services;


namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly Settings _settings;
        private readonly TemporaryOrderRepository _temporaryOrderRepository;
        private readonly UserRepository _userRepository;
        private readonly OrderRepository _orderRepository;
        private readonly EmailService _emailService;
        public CheckoutController(IOptions<Settings> options, TemporaryOrderRepository temporaryOrderRepository, OrderRepository orderRepository, UserRepository userRepository, EmailService emailService)
        {
            _settings = options.Value;
            _temporaryOrderRepository = temporaryOrderRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _emailService = new EmailService();

        }

        /*[HttpGet("products")]
        public IEnumerable<ProductDto[]> GetAllProducts()
        {
            return GetProducts();
        }
        */

        [HttpGet("GetAllProductsDto")]
        public async Task<ProductOrderDto[]> GetAllProducts(long temporaryOrderId)
        {
            return await GetProducts(temporaryOrderId);
        }

        [HttpGet("embedded")]
        public async Task<ActionResult> EmbededCheckout(long temporaryOrderId)
        {
            TemporaryOrder temporaryOrder = await _temporaryOrderRepository.GetTemporaryOrder(temporaryOrderId);
            User user = await _userRepository.GetUserById(temporaryOrder.UserId);
            ProductOrderDto[] products = await GetAllProducts(temporaryOrderId);
            List<SessionLineItemOptions> lineItems = new List<SessionLineItemOptions>();
            foreach (var product in products)
            {
                lineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        Currency = "eur",
                        UnitAmount = (long)(product.Price),
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = product.Name,
                            Description = product.Description,
                            Images = new List<string> { product.Image }
                        }
                    },
                    Quantity = product.Amount,
                });
            }
            SessionCreateOptions options = new SessionCreateOptions
            {
                UiMode = "embedded",
                Mode = "payment",
                PaymentMethodTypes = ["card"],
                LineItems = lineItems,
                CustomerEmail = user.Email,
                RedirectOnCompletion = "never"
            };

            SessionService service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Ok(new
            {
                sessionId = session.Id,
                clientSecret = session.ClientSecret
            });
        }

        [HttpGet("status/{sessionId}")]
        public async Task<ActionResult> SessionStatus(string sessionId, long temporaryOrderId)
        {
            SessionService sessionService = new SessionService();
            Session session = await sessionService.GetAsync(sessionId);
            var orderNew = new Order();
            if (session.PaymentStatus == "paid")
            {
                orderNew = await _orderRepository.CreateOrder(temporaryOrderId, session.PaymentMethodTypes.FirstOrDefault(), session.PaymentStatus, session.AmountTotal.Value, session.CustomerEmail);

                return Ok(new { order = orderNew.Id });
            }

            return BadRequest("No está pagado");
        }

        private async Task<ProductOrderDto[]> GetProducts(long temporaryOrderId)
        {
            TemporaryOrder temporaryOrder = await _temporaryOrderRepository.GetTemporaryOrder(temporaryOrderId);
            List<ProductOrderDto> productList = new List<ProductOrderDto>();
            foreach (var orderDetail in temporaryOrder.Details)
            {

                productList.Add(new ProductOrderDto
                {
                    Name = orderDetail.Product.Name,
                    Description = orderDetail.Product.Description,
                    Price = orderDetail.Product.Price,
                    Amount = orderDetail.Amount,
                    Image = orderDetail.Product.Image
                });


            }
            return productList.ToArray();
        }
        [HttpPost]
        [Route("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] EmailDto emailDto)
        {
            try
            {
                // Validación de entrada
                if (emailDto == null || string.IsNullOrEmpty(emailDto.To) || string.IsNullOrEmpty(emailDto.HtmlContent))
                {
                    return BadRequest("El correo o el contenido HTML son inválidos.");
                }

                // Enviar el correo
                await _emailService.SendEmailAsync(emailDto.To, "Asunto", emailDto.HtmlContent);

                return Ok("Correo enviado exitosamente");
            }
            catch (Exception ex)
            {
                // Registra el error
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                return StatusCode(500, $"Error al enviar el correo: {ex.Message}");
            }
        }



    }
}
