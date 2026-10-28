using Microsoft.AspNetCore.Mvc;
using Xpress.Savings.DTOs;
using Xpress.Savings.Services;

namespace Xpress.Savings.Controllers;

[ApiController]
[Route("api/savings/accounts")]
public sealed class SavingsAccountsController : ControllerBase
{
    private readonly ISavingsAccountService _service;

    public SavingsAccountsController(ISavingsAccountService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(SavingsAccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateSavingsAccountRequest request)
    {
        var account = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SavingsAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _service.GetByIdAsync(id);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("by-loan/{loanId:int}")]
    [ProducesResponseType(typeof(SavingsAccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLoan(int loanId)
    {
        var account = await _service.GetByLoanAsync(loanId);
        return account is null ? NotFound() : Ok(account);
    }
}
