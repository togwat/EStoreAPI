using EStoreAPI.Server.Data;
using EStoreAPI.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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
        public async Task<ActionResult<ICollection<Customer>>> GetAllCustomersAsync()
        {
            ICollection<Customer> customers = await _Repo.GetCustomersAsync();
            return Ok(customers);
        }

        // GET: api/Customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomerAsync(int id)
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

        // GET: api/Customers/search?query=
        [HttpGet("search")]
        public async Task<ActionResult<ICollection<Customer>>> SearchCustomersAsync([FromQuery] string? query)
        {
            if (query == null)
            {
                return await GetAllCustomersAsync();
            }
            else
            {
                ICollection<Customer> customers = await _Repo.GetCustomersByQueryAsync(query);
                return Ok(customers);
            }
        }

        // POST: api/Customers/create
        [HttpPost("create")]
        public async Task<ActionResult<Customer>> CreateCustomerAsync(Customer customer)
        {
            try
            {
                Customer newCustomer = await _Repo.AddCustomerAsync(customer);
                return CreatedAtAction(nameof(GetCustomerAsync), new { id = newCustomer.CustomerId }, newCustomer);
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }


        // PUT: api/Customers/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateCustomerWithIdAsync(int id, Customer customer)
        {
            // set up new customer
            customer.CustomerId = id;
            try
            {
                await _Repo.UpdateCustomerAsync(customer);
                return NoContent();
            }
            // customer not found
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            // invalid data
            catch (ValidationException)
            {
                return BadRequest();
            }
        }
    }
}
