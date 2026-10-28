using Hangfire;
using Microsoft.EntityFrameworkCore;
using Xpress.Application.DTOs.Reports;
using Xpress.Application.DTOs.Transactions;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.BackgroundJobs;
using Xpress.Infrastructure.Logger;

namespace Xpress.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly IReadApplicationDbContext _readCtx;
    private readonly IBackgroundJobClient _jobs;
    private readonly ILog<AuditService> _logger;

    public AuditService(
        IReadApplicationDbContext readCtx,
        IBackgroundJobClient jobs,
        ILog<AuditService> logger)
    {
        _readCtx = readCtx;
        _jobs = jobs;
        _logger = logger;
    }

    public GenerateReportResponse ScheduleTransactionReport(GenerateReportRequest request, string adminEmail)
    {
        try
        {
            var jobId = _jobs.Enqueue<TransactionReportJob>(
                job => job.GenerateAndSendAsync(request, adminEmail));

            _logger.Log($"Transaction report scheduled. JobId: {jobId} | Admin: {adminEmail}");

            return new GenerateReportResponse(
                jobId,
                "Scheduled",
                $"Your report is being generated. It will be sent to {adminEmail} when ready.");
        }
        catch (Exception ex)
        {
            _logger.Log(ex);
            throw;
        }
    }

    public async IAsyncEnumerable<TransactionExportDto> StreamTransactionsAsync(TransactionExportQuery query)
    {
        var results = (
            from t in _readCtx.Transactions
            where (query.CustomerId == null || t.CustomerId == query.CustomerId)
               && (query.Status == null || t.Status == query.Status)
               && (query.From == null || t.CreatedAt >= query.From)
               && (query.To == null || t.CreatedAt <= query.To)
            orderby t.CreatedAt descending
            select new TransactionExportDto(t.Id, t.Reference, t.CustomerId, t.Amount, t.Status, t.CreatedAt)
        ).AsAsyncEnumerable();

        await foreach (var row in results)
        {
            yield return row;
        }
    }
}
