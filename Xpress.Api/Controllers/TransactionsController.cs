using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xpress.Application.DTOs.Transactions;
using Xpress.Application.Interfaces;
using Xpress.Domain.Constants;

namespace Xpress.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize(Roles = Roles.UserOrAdmin)]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _service;

    public TransactionsController(ITransactionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetByCustomer([FromQuery] TransactionQuery query)
    {
        var result = await _service.GetByCustomerAsync(query.CustomerId, query.Page, query.PageSize);
        return Ok(result);
    }
}
