using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
        private readonly List<string> listCategories = new List<string>()
        {
            "Phones", "Computers", "Accessories", "Printers", "Cameras", "Other"
        };

        public ProductsController(IProductRepo repo, IMapper mapper, IWebHostEnvironment env)
        {
            this.repo = repo;
            this.mapper = mapper;
            this.env = env;
        }


        [HttpGet("categories")]
        public  IActionResult GetCategories()
        {
            return Ok(listCategories);
        }



        [HttpGet]
        public async Task<IActionResult> GetAllProduct(
      [FromQuery] string? search = null,
      [FromQuery] string? sortBy = null,
      [FromQuery] bool desc = false,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10)
        {
            var (productsDomain, totalItems) = await repo.GetAllAsync(search, sortBy, desc, page, pageSize);
            var productsDto = mapper.Map<IEnumerable<ProductDto>>(productsDomain);

            var response = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Products = productsDto
            };

            return Ok(response);
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


        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromForm] ProductDto productDto)
        {

            if (!listCategories.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "Please select a valid category");
                return BadRequest(ModelState);
            }


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


        [Authorize(Roles = "admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromForm] ProductDto productDto)
        {

            if (!listCategories.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "Please select a valid category");
                return BadRequest(ModelState);
            }


            var existingProduct = await repo.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            string imageFileName = existingProduct.ImageFileName;

            if (productDto.ImageFile != null)
            {
                // Ensure the images folder exists
                string imagesFolder = Path.Combine(env.WebRootPath, "images", "products");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Generate a new unique filename
                string extension = Path.GetExtension(productDto.ImageFile.FileName);
                imageFileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";

                // Define full file path for saving
                string filePath = Path.Combine(imagesFolder, imageFileName);

                // Save the new image asynchronously
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(stream);
                }

                // Delete the old image if it exists
                if (!string.IsNullOrWhiteSpace(existingProduct.ImageFileName))
                {
                    string oldImagePath = Path.Combine(env.WebRootPath, existingProduct.ImageFileName.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
            }

            // Update product fields without overwriting the entire object
            mapper.Map(productDto, existingProduct);
            existingProduct.ImageFileName = $"/images/products/{imageFileName}"; // Store relative path

            await repo.UpdtadeProductAsync(id,existingProduct);

            productDto = mapper.Map<ProductDto>(existingProduct);

            return Ok(productDto);
        }


        [Authorize(Roles = "admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            var existingProduct = await repo.GetByIdAsync(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Construct the absolute path of the image
            if (!string.IsNullOrWhiteSpace(existingProduct.ImageFileName))
            {
                string imagePath = Path.Combine(env.WebRootPath, existingProduct.ImageFileName.TrimStart('/'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // Delete the product from the database
            await repo.DeleteAsync(id);

            return Ok(new { Message = "Product deleted successfully" });
        }


    }
}
