using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoeStore.Entities.Models;
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
        private readonly IWebHostEnvironment env;

        public ProductsController(IProductRepo repo, IMapper mapper, IWebHostEnvironment env)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.env = env;
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



        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto)
        {
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image File is required");
                return BadRequest(ModelState);
            }

            // Ensure the images folder exists
            string imagesFolder = Path.Combine(env.WebRootPath, "images", "products");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            // Generate a unique file name
            string extension = Path.GetExtension(productDto.ImageFile.FileName);
            string imageFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";

            // Define full file path
            string filePath = Path.Combine(imagesFolder, imageFileName);

            // Save file asynchronously
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await productDto.ImageFile.CopyToAsync(stream);
            }

            // Save in the Database
            var productDomain = mapper.Map<Product>(productDto);
            productDomain.ImageFileName = $"/images/products/{imageFileName}"; // Save relative path

            productDomain = await repo.CreateProductAsync(productDomain);
            productDto = mapper.Map<ProductDto>(productDomain);

            return Ok(productDto);
        }


    }
}
