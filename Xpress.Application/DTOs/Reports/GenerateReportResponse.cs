namespace Xpress.Application.DTOs.Reports;

public sealed record GenerateReportResponse(
    string JobId,
    string Status,
    string Message
);
