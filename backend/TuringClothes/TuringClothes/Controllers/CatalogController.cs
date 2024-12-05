using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using TuringClothes.Database;
using TuringClothes.Dtos;
using TuringClothes.Model;
using TuringClothes.Pagination;
using TuringClothes.Repository;
using TuringClothes.Services;



namespace TuringClothes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly CatalogService _catalogService;
        
        public CatalogController(UnitOfWork unitOfWork, CatalogService catalogService)
        {
            _unitOfWork = unitOfWork;
            _catalogService = catalogService;
        }



        [HttpGet("GetProduct")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            try
            {
                var result = await _unitOfWork.ProductRepository.GetProductById(id);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }


        [AllowAnonymous]
        [HttpGet ("ProductosPaginados")]
        public  ActionResult<PagedResults<Product>> GetPagination([FromQuery] PaginationParams paginationQuery)
        {
            var results = _catalogService.GetPaginationCatalog(paginationQuery);
            return Ok(results);
        }
       
        

    }
}