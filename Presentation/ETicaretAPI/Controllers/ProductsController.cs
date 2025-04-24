using Application.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace ETicaretAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly private IProductWriteRepository _productWriteRepository;
        readonly private IProductReadRepository _productReadRepository;

        readonly private IOrderWriteRepository _orderWriteRepository;
        readonly private IOrderReadRepository _orderReadRepository;

        readonly private ICustomerWriteRepository _customerWriteRepository;
        public ProductsController(
            IProductWriteRepository productWriteRepository,
            IProductReadRepository productReadRepository,
            IOrderWriteRepository orderWriteRepository,
            ICustomerWriteRepository customerWriteRepository,
            IOrderReadRepository orderReadRepository)
        {
            _productWriteRepository = productWriteRepository;
            _productReadRepository = productReadRepository;
            _orderWriteRepository = orderWriteRepository;
            _customerWriteRepository = customerWriteRepository;
            _orderReadRepository = orderReadRepository;
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Get()
        {
           var list= _productReadRepository.GetAll();
            return Ok(list);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            Product product = new Product();
            product.Name = "Product 1";
            product.Stock = 15;
            product.Price = 150;
            await _productWriteRepository.AddAsync(product);
            await _productWriteRepository.SaveAsync();
            return StatusCode((int)HttpStatusCode.Created);
        }
    }
}
