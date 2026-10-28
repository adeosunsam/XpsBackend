## 1. User Authentication & Authorization

JWT Bearer authentication with ASP.NET Core Identity. On login/register the server signs a token containing the user ID, email, and role claims. Every subsequent request validates the token signature, issuer, and audience before the request reaches a controller.

Role-based access is enforced at the controller level using `[Authorize(Roles = ...)]`. Three roles are in play:

| Role | Access |
|---|---|
| `Admin` | All endpoints including `/api/reports`, `/api/customers` (POST), `/hangfire` |
| `User` | `/api/loans`, `/api/transactions` |
| Public | `/api/auth/register`, `/api/auth/login` |

**Key files:** `AuthService.cs` · `TokenService.cs` · `RoleSeeder.cs` · `AuthController.cs`

---

## 2. Database Query Optimization

All read queries use a dedicated `ReadApplicationDbContext` registered with `QueryTrackingBehavior.NoTracking` — no change tracking overhead on reads. Write operations go through `ApplicationDbContext`.

The paginated transaction query avoids a common N+1 mistake — `Count()` is run as a separate query rather than embedded inside a `Select()` projection (which would generate a correlated subquery per row).

Loan-with-transactions retrieval uses a LINQ GROUP JOIN that EF Core translates into a single `LEFT JOIN` — no navigation property required, no separate round-trip per loan.

```sql
DECLARE @customerId int = 1;

SELECT [l].[Id], [l].[CustomerId], [l].[Amount], [l].[TermMonths], [l].[Status], [l].[CreatedAt],
       [t].[Id], [t].[Reference], [t].[Amount], [t].[Status], [t].[CreatedAt]
FROM [Loans] AS [l]
LEFT JOIN [Transactions] AS [t] ON [l].[Id] = [t].[LoanId]
WHERE [l].[CustomerId] = @customerId
ORDER BY [l].[CreatedAt] DESC, [l].[Id], [t].[CreatedAt] DESC
```

**Key files:** `TransactionService.cs` · `LoanService.cs` · `PersistenceExtensions.cs` · `TransactionConfiguration.cs`

---

## 3. Web API Rate Limiting

ASP.NET Core's built-in `FixedWindowRateLimiter` with two chained partitioned limiters applied globally:

- **User limiter** — partitions by authenticated user ID; anonymous requests bypass it
- **IP limiter** — partitions by `RemoteIpAddress`; null IP gets `NoLimiter` (no shared bucket a bot could exhaust for other clients)

A null IP is never dropped into a shared fallback bucket — if the IP is unavailable the request bypasses rate limiting rather than being blocked by another client's traffic.

`UseForwardedHeaders` runs before rate limiting so `RemoteIpAddress` reflects the real client IP when behind a reverse proxy, not the proxy's IP.

**Key files:** `RateLimitingExtensions.cs` · `RateLimitingOptions.cs` · `FixedWindowPolicyOptions.cs`

---

## 4. Background Job Processing — Hangfire

When an admin requests a transaction report, the API returns `202 Accepted` immediately with a job ID. Hangfire picks up the job on a background thread.

The report generator streams rows from the database using `AsAsyncEnumerable()` — rows flow from the SQL Server cursor one at a time directly into a `MemoryStream`. This keeps server memory flat regardless of dataset size (millions of rows) and prevents the DB from being hammered with a single massive result set that could spike RAM and bring the service down.

Once the CSV is built, the job sends it to the requesting admin's email. On failure, Hangfire retries with exponential backoff:

```
Attempt 1 → wait 1 min → retry
Attempt 2 → wait 5 min → retry
Attempt 3 → wait 15 min → retry
Attempt 4 → marked Failed
```

**Key files:** `TransactionReportJob.cs` · `AuditService.cs` · `ReportsController.cs`

---

## 5. Microservices Communication — REST

Two independent services, each with its own SQL Server database:

| Service | Database | Responsibility |
|---|---|---|
| `Xpress.Api` (port 7001) | `XpressBackendDb` | Loans, customers, auth |
| `Xpress.Savings` (port 7267) | `XpressSavingsDb` | Savings accounts |

When a loan is created, `XpressBackend` calls `Xpress.Savings` via a typed `HttpClient` registered with `IHttpClientFactory`. If the savings service fails to provision an account, the loan is removed (compensating transaction) and the caller receives a meaningful error.

**Security — three-layer gate on every inter-service call:**

1. `X-Api-Key` — shared secret, validates identity
2. `X-Service-Name` — caller must be on the `AllowedServices` whitelist in config
3. `X-Signature` — HMAC-SHA256 of `METHOD\nPATH\nTIMESTAMP\nBODY` using the API key as the signing secret; timestamp prevents replay attacks (requests older than 5 minutes are rejected)

The `HmacSigningHandler` (a `DelegatingHandler`) attaches all three headers automatically on every outgoing request — the calling service never manually adds them.

**Key files:** `SavingsServiceClient.cs` · `HmacSigningHandler.cs` · `ApiKeyMiddleware.cs` · `SavingsAccountService.cs`
