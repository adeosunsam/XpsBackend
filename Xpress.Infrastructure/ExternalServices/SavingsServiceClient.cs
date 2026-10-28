using System.Net;
using System.Net.Http.Json;
using Xpress.Application.DTOs.Savings;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.ExternalServices;

public sealed class SavingsServiceClient : ISavingsServiceClient
{
    private readonly HttpClient _http;
    private readonly ILog<SavingsServiceClient> _logger;

    public SavingsServiceClient(HttpClient http, ILog<SavingsServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<SavingsAccountDto> CreateAccountAsync(int loanId, int customerId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/savings/accounts",
                new CreateSavingsAccountRequest(loanId, customerId));

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Savings service returned {(int)response.StatusCode} while creating account for loan {loanId}.");

            return await response.Content.ReadFromJsonAsync<SavingsAccountDto>()
                ?? throw new InvalidOperationException("Savings service returned an empty response.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.Log(ex);
            throw new TimeoutException("Savings service timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.Log(ex);
            throw new InvalidOperationException("Savings service is unavailable.", ex);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    public async Task<SavingsAccountDto?> GetAccountByLoanAsync(int loanId)
    {
        try
        {
            var response = await _http.GetAsync($"api/savings/accounts/by-loan/{loanId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(
                    $"Savings service returned {(int)response.StatusCode}.");

            return await response.Content.ReadFromJsonAsync<SavingsAccountDto>();
        }
        catch (TaskCanceledException ex)
        {
            _logger.Log(ex);
            throw new TimeoutException("Savings service timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.Log(ex);
            throw new InvalidOperationException("Savings service is unavailable.", ex);
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }
}
