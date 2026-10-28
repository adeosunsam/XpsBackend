using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xpress.Application.DTOs.Customers;
using Xpress.Application.Interfaces;
using Xpress.Domain.Constants;
using Xpress.Domain.Entities;

namespace Xpress.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize(Roles = Roles.UserOrAdmin)]
public sealed class CustomersController : ControllerBase
{
    private readonly IApplicationDbContext _writeCtx;
    private readonly IReadApplicationDbContext _readCtx;

    public CustomersController(IApplicationDbContext writeCtx, IReadApplicationDbContext readCtx)
    {
        _writeCtx = writeCtx;
        _readCtx = readCtx;
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request)
    {
        var exists = await (
            from c in _readCtx.Customers
            where c.Email == request.Email
            select c
        ).AnyAsync();

        if (exists)
            return Problem("A customer with this email already exists.", statusCode: StatusCodes.Status409Conflict);

        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
        };

        _writeCtx.Customers.Add(customer);
        await _writeCtx.SaveChangesAsync();

        var dto = new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, dto);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await (
            from c in _readCtx.Customers
            where c.Id == id
            select c
        ).FirstOrDefaultAsync();

        if (customer is null)
            return NotFound();

        return Ok(new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.CreatedAt));
    }
}
