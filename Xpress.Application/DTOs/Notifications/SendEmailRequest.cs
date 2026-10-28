namespace Xpress.Application.DTOs.Notifications;
public sealed record SendEmailRequest(string To, string Subject, string Body);
