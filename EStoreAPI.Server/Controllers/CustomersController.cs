using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IEStoreRepo _Repo;

        public CustomersController(IEStoreRepo repo)
        {
            _Repo = repo;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<ICollection<Customer>>> GetCustomersAsync()
        {
            ICollection<Customer> customers = await _Repo.GetCustomersAsync();
            return Ok(customers);
        }
    }
}
