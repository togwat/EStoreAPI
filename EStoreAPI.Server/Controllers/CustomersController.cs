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

        // GET: api/Customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerByIdAsync(int id)
        {
            Customer? customer = await _Repo.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(customer);
            }
        }

        // GET: api/Customers/search
        [HttpGet("search")]
        public async Task<ActionResult<ICollection<Customer>>> GetCustomersByQueryAsync([FromQuery] string? query)
        {
            if (query == null)
            {
                return await GetCustomersAsync();
            }
            else
            {
                ICollection<Customer> customers = await _Repo.GetCustomersByQueryAsync(query);
                return Ok(customers);
            }
        }

        // POST: api/customers/add
        [HttpPost("add")]
        public async Task<ActionResult<Customer>> AddCustomerAsync(Customer customer)
        {
            Customer newCustomer = await _Repo.AddCustomerAsync(customer);

            return CreatedAtAction(nameof(GetCustomerByIdAsync), new { id = newCustomer.CustomerId }, newCustomer);
        }


        // PUT: api/customers/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateCustomerAsync(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return BadRequest();
            }

            try
            {
                await _Repo.UpdateCustomerAsync(customer);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
