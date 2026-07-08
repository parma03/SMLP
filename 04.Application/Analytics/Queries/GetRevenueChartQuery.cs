using MediatR;
using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.Persistence;
using Smlp.Base.Transaction.Enums;
using Smlp.Shared.Analytics.Queries.GetRevenueChart;

namespace Smlp.Application.Analytics.Queries;

public class GetRevenueChartQuery : IRequest<GetRevenueChartResponse>
{
    public GetRevenueChartRequest Request { get; set; } = default!;
}

public class GetRevenueChartQueryHandler : IRequestHandler<GetRevenueChartQuery, GetRevenueChartResponse>
{
    private readonly ISmlpDbContext _context;

    public GetRevenueChartQueryHandler(ISmlpDbContext context) => _context = context;

    public async Task<GetRevenueChartResponse> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var year = req.Year;

        var baseQuery = _context.Transactions
            .AsNoTracking()
            .Where(t => !t.IsDeleted && t.Status == TransactionStatus.Completed && t.TransactionDate.Year == year);

        // Monthly revenue
        var monthly = await baseQuery
            .GroupBy(t => t.TransactionDate.Month)
            .Select(g => new
            {
                Month = g.Key,
                Amount = g.Sum(t => t.TotalAmount),
                Transactions = g.Count(),
                PointsEarned = g.Sum(t => t.PointsEarned)
            })
            .OrderBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var monthNames = new[] { "", "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };
        var monthlyRevenue = Enumerable.Range(1, 12).Select(m =>
        {
            var data = monthly.FirstOrDefault(x => x.Month == m);
            return new RevenueDataPoint
            {
                Label = monthNames[m],
                Amount = data?.Amount ?? 0,
                Transactions = data?.Transactions ?? 0,
                PointsEarned = data?.PointsEarned ?? 0
            };
        }).ToList();

        // Daily revenue for specific month (if provided)
        List<RevenueDataPoint> dailyRevenue = new();
        if (req.Month > 0 && req.Month <= 12)
        {
            var daily = await baseQuery
                .Where(t => t.TransactionDate.Month == req.Month)
                .GroupBy(t => t.TransactionDate.Day)
                .Select(g => new { Day = g.Key, Amount = g.Sum(t => t.TotalAmount), Transactions = g.Count(), PointsEarned = g.Sum(t => t.PointsEarned) })
                .OrderBy(x => x.Day)
                .ToListAsync(cancellationToken);

            var daysInMonth = DateTime.DaysInMonth(year, req.Month);
            dailyRevenue = Enumerable.Range(1, daysInMonth).Select(d =>
            {
                var data = daily.FirstOrDefault(x => x.Day == d);
                return new RevenueDataPoint
                {
                    Label = d.ToString(),
                    Amount = data?.Amount ?? 0,
                    Transactions = data?.Transactions ?? 0,
                    PointsEarned = data?.PointsEarned ?? 0
                };
            }).ToList();
        }

        // Payment method breakdown
        var paymentMethods = await baseQuery
            .GroupBy(t => t.PaymentMethod)
            .Select(g => new PaymentMethodBreakdown
            {
                Method = g.Key.ToString(),
                Amount = g.Sum(t => t.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(p => p.Amount)
            .ToListAsync(cancellationToken);

        return new GetRevenueChartResponse
        {
            MonthlyRevenue = monthlyRevenue,
            DailyRevenue = dailyRevenue,
            PaymentMethods = paymentMethods
        };
    }
}
