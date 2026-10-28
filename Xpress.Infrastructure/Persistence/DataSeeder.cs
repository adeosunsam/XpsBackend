using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xpress.Domain.Constants;
using Xpress.Domain.Entities;
using Xpress.Infrastructure.Identity;

namespace Xpress.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        await SeedUsersAsync(userManager);
        await SeedCustomersAsync(context);
        await SeedLoansAsync(context);
        await SeedTransactionsAsync(context);
    }

    // ── Users ────────────────────────────────────────────────────────────────

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        await CreateUserAsync(userManager,
            firstName: "System",
            lastName: "Admin",
            email: "admin@xpress.com",
            password: "Admin@12345!",
            role: Roles.Admin);

        await CreateUserAsync(userManager,
            firstName: "John",
            lastName: "Doe",
            email: "user@xpress.com",
            password: "User@12345!",
            role: Roles.User);
    }

    private static async Task CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string firstName, string lastName, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = email,
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    // ── Customers ────────────────────────────────────────────────────────────

    private static async Task SeedCustomersAsync(ApplicationDbContext context)
    {
        if (await context.Customers.AnyAsync())
            return;

        context.Customers.AddRange(
            new Customer { Name = "Alex Johnson",    Email = "alex.johnson@email.com",    Phone = "+1-555-0101", CreatedAt = DateTime.UtcNow.AddDays(-60) },
            new Customer { Name = "Sarah Williams",  Email = "sarah.williams@email.com",  Phone = "+1-555-0102", CreatedAt = DateTime.UtcNow.AddDays(-45) },
            new Customer { Name = "Michael Brown",   Email = "michael.brown@email.com",   Phone = "+1-555-0103", CreatedAt = DateTime.UtcNow.AddDays(-30) }
        );

        await context.SaveChangesAsync();
    }

    // ── Loans ────────────────────────────────────────────────────────────────

    private static async Task SeedLoansAsync(ApplicationDbContext context)
    {
        if (await context.Loans.AnyAsync())
            return;

        var customers = await context.Customers.OrderBy(c => c.Id).ToListAsync();
        if (customers.Count == 0)
            return;

        context.Loans.AddRange(
            new Loan { CustomerId = customers[0].Id, Amount = 50_000m, TermMonths = 12, Status = "Approved", CreatedAt = DateTime.UtcNow.AddDays(-55), },
            new Loan { CustomerId = customers[0].Id, Amount = 25_000m, TermMonths = 6,  Status = "Pending",  CreatedAt = DateTime.UtcNow.AddDays(-10) },
            new Loan { CustomerId = customers[1].Id, Amount = 100_000m, TermMonths = 24, Status = "Approved", CreatedAt = DateTime.UtcNow.AddDays(-40) },
            new Loan { CustomerId = customers[1].Id, Amount = 20_000m,  TermMonths = 6,  Status = "Rejected", CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Loan { CustomerId = customers[2].Id, Amount = 15_000m, TermMonths = 3, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-5) }
        );

        await context.SaveChangesAsync();
    }

    // ── Transactions ─────────────────────────────────────────────────────────

    private static async Task SeedTransactionsAsync(ApplicationDbContext context)
    {
        if (await context.Transactions.AnyAsync())
            return;

        var customers = await context.Customers.OrderBy(c => c.Id).ToListAsync();
        var loans = await context.Loans.OrderBy(l => l.Id).ToListAsync();

        if (customers.Count == 0 || loans.Count == 0)
            return;

        var loan1 = loans[0]; // Customer 1 — Approved 50k
        var loan3 = loans[2]; // Customer 2 — Approved 100k

        var c1 = customers[0].Id.ToString();
        var c2 = customers[1].Id.ToString();
        var c3 = customers[2].Id.ToString();

        context.Transactions.AddRange(

            // ── Customer 1 — loan repayments (loan1) ─────────────────────────
            new Transaction { Reference = "TRX-001", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-50), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-002", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-43), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-003", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-36), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-004", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-29), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-005", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-22), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-006", CustomerId = c1, Amount = 4_500m,  Status = "Failed",    CreatedAt = DateTime.UtcNow.AddDays(-15), LoanId = loan1.Id },
            new Transaction { Reference = "TRX-007", CustomerId = c1, Amount = 4_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-14), LoanId = loan1.Id },

            // ── Customer 1 — standalone ───────────────────────────────────────
            new Transaction { Reference = "TRX-008", CustomerId = c1, Amount = 1_200m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-25) },
            new Transaction { Reference = "TRX-009", CustomerId = c1, Amount = 3_500m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new Transaction { Reference = "TRX-010", CustomerId = c1, Amount = 750m,    Status = "Pending",   CreatedAt = DateTime.UtcNow.AddDays(-2)  },

            // ── Customer 2 — loan repayments (loan3) ─────────────────────────
            new Transaction { Reference = "TRX-011", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-38), LoanId = loan3.Id },
            new Transaction { Reference = "TRX-012", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-31), LoanId = loan3.Id },
            new Transaction { Reference = "TRX-013", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-24), LoanId = loan3.Id },
            new Transaction { Reference = "TRX-014", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-17), LoanId = loan3.Id },
            new Transaction { Reference = "TRX-015", CustomerId = c2, Amount = 5_000m,  Status = "Failed",    CreatedAt = DateTime.UtcNow.AddDays(-10), LoanId = loan3.Id },
            new Transaction { Reference = "TRX-016", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-9),  LoanId = loan3.Id },
            new Transaction { Reference = "TRX-017", CustomerId = c2, Amount = 5_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-3),  LoanId = loan3.Id },

            // ── Customer 2 — standalone ───────────────────────────────────────
            new Transaction { Reference = "TRX-018", CustomerId = c2, Amount = 800m,    Status = "Failed",    CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Transaction { Reference = "TRX-019", CustomerId = c2, Amount = 800m,    Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-19) },
            new Transaction { Reference = "TRX-020", CustomerId = c2, Amount = 2_300m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-12) },
            new Transaction { Reference = "TRX-021", CustomerId = c2, Amount = 15_000m, Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-6)  },
            new Transaction { Reference = "TRX-022", CustomerId = c2, Amount = 450m,    Status = "Pending",   CreatedAt = DateTime.UtcNow.AddDays(-1)  },

            // ── Customer 3 — standalone (pending loan, no repayments yet) ─────
            new Transaction { Reference = "TRX-023", CustomerId = c3, Amount = 500m,    Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-4)  },
            new Transaction { Reference = "TRX-024", CustomerId = c3, Amount = 1_000m,  Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-3)  },
            new Transaction { Reference = "TRX-025", CustomerId = c3, Amount = 250m,    Status = "Failed",    CreatedAt = DateTime.UtcNow.AddDays(-2)  },
            new Transaction { Reference = "TRX-026", CustomerId = c3, Amount = 250m,    Status = "Completed", CreatedAt = DateTime.UtcNow.AddDays(-1)  }
        );

        await context.SaveChangesAsync();
    }
}
