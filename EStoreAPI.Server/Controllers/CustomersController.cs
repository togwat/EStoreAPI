using EStoreAPI.Server.Services;
using EStoreAPI.Server.Models;
using EStoreAPI.Server.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EStoreAPI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;

        public CustomersController(ICustomerService service)
        {
            _service = service;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<ICollection<Customer>>> GetAllCustomersAsync()
        {
            ICollection<Customer> customers = await _service.GetAllCustomersAsync();
            return Ok(customers);
        }

        // GET: api/Customers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer?>> GetCustomerAsync(int id)
        {
            Customer? customer = await _service.GetCustomerAsync(id);
            return customer is null ? NotFound() : Ok(customer);
        }

        // GET: api/Customers/search?query=
        [HttpGet("search")]
        public async Task<ActionResult<ICollection<Customer>>> SearchCustomersAsync([FromQuery] string? query)
        {
            ICollection<Customer> customers = await _service.SearchCustomersAsync(query);
            return Ok(customers);
        }

        // POST: api/Customers/create
        [HttpPost("create")]
        public async Task<ActionResult<Customer>> CreateCustomerAsync(CustomerDTO dto)
        {
            try
            {
                Customer newCustomer = await _service.CreateCustomerAsync(dto);
                return CreatedAtAction("GetCustomer", new { id = newCustomer.CustomerId }, newCustomer);
            }
            catch (ValidationException)
            {
                return BadRequest();
            }
        }

        // POST: api/Customers/create-bulk
        [HttpPost("create-bulk")]
        public async Task<ActionResult<ICollection<Customer>>> CreateCustomersAsync(ICollection<CustomerDTO> dtos)
        {
            try
            {
                ICollection<Customer> newCustomers = await _service.CreateCustomersAsync(dtos);
                // fake GetAllCustomers to return newly created customers
                return CreatedAtAction("GetAllCustomers", null, newCustomers);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Customers/update/{id}
        [HttpPut("update/{id}")]
        public async Task<ActionResult> UpdateCustomerAsync(int id, CustomerDTO dto)
        {
            try
            {
                await _service.UpdateCustomerAsync(id, dto);
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
