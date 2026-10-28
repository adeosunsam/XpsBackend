using System.Text;
using Hangfire;
using Xpress.Application.DTOs.Reports;
using Xpress.Application.DTOs.Transactions;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.BackgroundJobs;

public sealed class TransactionReportJob
{
    private readonly IAuditService _auditService;
    private readonly ILog<TransactionReportJob> _logger;

    public TransactionReportJob(IAuditService auditService, ILog<TransactionReportJob> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    [AutomaticRetry(
        Attempts = 3,
        DelaysInSeconds = [60, 300, 900],
        OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task GenerateAndSendAsync(GenerateReportRequest request, string adminEmail)
    {
        try
        {
            _logger.Log($"Report job started. Admin: {adminEmail} | Period: {request.From:d} – {request.To:d}");

            var query = new TransactionExportQuery(
                request.From,
                request.To,
                request.Status,
                request.CustomerId);

            using var buffer = new MemoryStream();
            await using var writer = new StreamWriter(buffer, Encoding.UTF8, leaveOpen: true);

            await writer.WriteLineAsync("Id,Reference,CustomerId,Amount,Status,CreatedAt");

            var rowCount = 0;
            await foreach (var row in _auditService.StreamTransactionsAsync(query))
            {
                await writer.WriteLineAsync(
                    $"{row.Id},{row.Reference},{row.CustomerId},{row.Amount:F2},{row.Status},{row.CreatedAt:O}");

                rowCount++;

                if (rowCount % 10_000 == 0)
                    _logger.Log($"Report progress: {rowCount:N0} rows written");
            }

            await writer.FlushAsync();

            _logger.Log($"Report complete — {rowCount:N0} rows for {adminEmail}");

            // send out mail to the requestor containing the report.
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }
}
