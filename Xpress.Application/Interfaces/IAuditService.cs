using Xpress.Application.DTOs.Reports;
using Xpress.Application.DTOs.Transactions;

namespace Xpress.Application.Interfaces;

public interface IAuditService
{
    IAsyncEnumerable<TransactionExportDto> StreamTransactionsAsync(TransactionExportQuery query);
    GenerateReportResponse ScheduleTransactionReport(GenerateReportRequest request, string adminEmail);
}
