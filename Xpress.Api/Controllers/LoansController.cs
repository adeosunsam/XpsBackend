using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xpress.Application.DTOs.Loans;
using Xpress.Application.Interfaces;
using Xpress.Domain.Constants;

namespace Xpress.Api.Controllers;

[ApiController]
[Route("api/loans")]
[Authorize(Roles = Roles.UserOrAdmin)]
public sealed class LoansController : ControllerBase
{
    private readonly ILoanService _loans;

    public LoansController(ILoanService loans) => _loans = loans;

    [HttpPost]
    [ProducesResponseType(typeof(CreateLoanResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLoanRequest request)
    {
        var loan = await _loans.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = loan.Id },
            new CreateLoanResponse(loan.Id, loan.Status, "Loan created successfully."));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LoanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var loan = await _loans.GetByIdAsync(id);
        return loan is null ? NotFound() : Ok(loan);
    }

    [HttpGet("by-customer/{customerId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<LoanWithTransactionsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWithTransactions(int customerId)
    {
        var result = await _loans.GetWithTransactionsAsync(customerId);
        return Ok(result);
    }
}
