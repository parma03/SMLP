using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Shared.Analytics.Queries.GetCustomerInsights;

namespace Smlp.Application.Analytics.Queries;

public class GetCustomerInsightsQuery : IRequest<GetCustomerInsightsResponse> { }

public class GetCustomerInsightsQueryHandler : IRequestHandler<GetCustomerInsightsQuery, GetCustomerInsightsResponse>
{
    private readonly ISmlpDbContext _context;

    public GetCustomerInsightsQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetCustomerInsightsResponse> Handle(GetCustomerInsightsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Segmentation thresholds
        var cutoffNew = now.AddDays(-30);
        var cutoffActive = now.AddDays(-30);
        var cutoffLoyal = now.AddDays(-60);
        var cutoffAtRisk = now.AddDays(-45);
        var cutoffChurn = now.AddDays(-45);

        var customers = await _context.Customers
            .Where(c => !c.IsDeleted)
            .Select(c => new
            {
                c.Id,
                c.MemberCode,
                c.FullName,
                c.PhoneNumber,
                c.BirthDate,
                c.Tier,
                c.TotalPoints,
                c.LifetimePoints,
                c.TotalSpend,
                c.LastTransactionDate,
                c.Created
            })
            .ToListAsync(cancellationToken);

        // RFM-based segmentation
        var newCount = customers.Count(c => c.Created >= cutoffNew);
        var activeCount = customers.Count(c => c.LastTransactionDate.HasValue && c.LastTransactionDate >= cutoffActive);
        var loyalCount = customers.Count(c => c.LastTransactionDate.HasValue && c.LastTransactionDate >= cutoffLoyal && c.LifetimePoints >= 500);
        var atRiskCount = customers.Count(c => c.LastTransactionDate.HasValue
            && c.LastTransactionDate < cutoffAtRisk && c.LastTransactionDate >= now.AddDays(-90)
            && c.TotalSpend > 0);
        var inactiveCount = customers.Count(c => !c.LastTransactionDate.HasValue || c.LastTransactionDate < now.AddDays(-90));

        // Averages
        var avgSpend = customers.Any() ? customers.Average(c => (double)c.TotalSpend) : 0;
        var avgPoints = customers.Any() ? customers.Average(c => (double)c.TotalPoints) : 0;

        // Total transactions for retention calc
        var totalTx = await _context.Transactions.CountAsync(t => !t.IsDeleted, cancellationToken);
        var repeatTx = await _context.Transactions
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.CustomerId)
            .CountAsync(g => g.Count() > 1, cancellationToken);

        var retentionRate = totalTx > 0 ? (decimal)repeatTx / customers.Count * 100 : 0;

        // Top spenders
        var topSpenders = await _context.Customers
            .Where(c => !c.IsDeleted && c.TotalSpend > 0)
            .OrderByDescending(c => c.TotalSpend)
            .Take(10)
            .Select(c => new TopCustomerItem
            {
                Id = c.Id,
                MemberCode = c.MemberCode,
                FullName = c.FullName,
                Tier = c.Tier.ToString(),
                TotalSpend = c.TotalSpend,
                TotalPoints = c.TotalPoints,
                TransactionCount = c.Transactions.Count(t => !t.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        // Upcoming birthdays (within 14 days)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var birthdaysThisMonth = customers
            .Where(c => c.BirthDate.HasValue)
            .Select(c =>
            {
                var bd = c.BirthDate!.Value;
                var thisYearBd = new DateOnly(today.Year, bd.Month, bd.Day);
                if (thisYearBd < today)
                    thisYearBd = thisYearBd.AddYears(1);
                return new { Customer = c, NextBirthday = thisYearBd, DaysUntil = thisYearBd.DayNumber - today.DayNumber };
            })
            .Where(x => x.DaysUntil <= 14)
            .OrderBy(x => x.DaysUntil)
            .Select(x => new BirthdayCustomerItem
            {
                Id = x.Customer.Id,
                FullName = x.Customer.FullName,
                MemberCode = x.Customer.MemberCode,
                PhoneNumber = x.Customer.PhoneNumber,
                BirthDate = x.Customer.BirthDate,
                TotalPoints = x.Customer.TotalPoints
            })
            .ToList();

        // Churn risk: no purchase > 45 days but were active
        var churnRisk = customers
            .Where(c => c.LastTransactionDate.HasValue
                     && c.LastTransactionDate < cutoffChurn
                     && c.TotalSpend > 0)
            .OrderBy(c => c.LastTransactionDate)
            .Take(20)
            .Select(c => new ChurnRiskItem
            {
                Id = c.Id,
                FullName = c.FullName,
                MemberCode = c.MemberCode,
                PhoneNumber = c.PhoneNumber,
                Tier = c.Tier.ToString(),
                DaysSinceLastPurchase = (int)(now - c.LastTransactionDate!.Value).TotalDays,
                TotalPoints = c.TotalPoints
            })
            .ToList();

        return new GetCustomerInsightsResponse
        {
            NewCount = newCount,
            ActiveCount = activeCount,
            LoyalCount = loyalCount,
            AtRiskCount = atRiskCount,
            InactiveCount = inactiveCount,
            AvgTransactionValue = (decimal)avgSpend,
            AvgPointsPerCustomer = (decimal)avgPoints,
            RetentionRate = Math.Round(retentionRate, 2),
            TopSpenders = topSpenders,
            BirthdaysThisMonth = birthdaysThisMonth,
            ChurnRiskCustomers = churnRisk
        };
    }
}
