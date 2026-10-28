namespace Xpress.Application.DTOs.Reports;

public sealed record GenerateReportRequest(
    DateTime? From,
    DateTime? To,
    string? Status,
    string? CustomerId
);
