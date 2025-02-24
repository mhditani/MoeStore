using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoeStore.Entities.Models.DTO;
using MoeStore.Services.Repository.IRepository;

namespace MoeStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepo repo;
        private readonly IMapper mapper;

        public ProductsController(IProductRepo repo, IMapper mapper)
        {
            this.repo = repo;
            this.mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            var productsDomain = await repo.GetAllAsync();
            var productsDto = mapper.Map<IEnumerable<ProductDto>>(productsDomain);
            return Ok(productsDto);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct([FromRoute] int id)
        {
            var productDomain = await repo.GetByIdAsync(id);
            if (productDomain == null)
            {
                return NotFound();
            }
            return Ok(mapper.Map<ProductDto>(productDomain));
        }

    }
}
