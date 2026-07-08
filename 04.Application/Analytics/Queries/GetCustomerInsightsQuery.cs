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
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Load customers with transaction counts
        var customers = await _context.Customers
            .AsNoTracking()
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
                c.CreatedAt,
                TransactionCount = c.Transactions.Count(t => !t.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        // ------- RFM scoring per customer -------
        // R: hari sejak transaksi terakhir (lebih kecil = lebih baik, skor 1-5)
        // F: frekuensi transaksi (lebih besar = lebih baik, skor 1-5)
        // M: total spend (lebih besar = lebih baik, skor 1-5)
        var maxSpend = customers.Any(c => c.TotalSpend > 0) ? customers.Max(c => c.TotalSpend) : 1m;

        int RScore(DateTimeOffset? lastTx) =>
            lastTx.HasValue ? (int)(now - lastTx.Value).TotalDays switch
            {
                <= 7 => 5,
                <= 30 => 4,
                <= 60 => 3,
                <= 90 => 2,
                _ => 1
            } : 1;

        int FScore(int txCount) => txCount switch
        {
            >= 20 => 5,
            >= 10 => 4,
            >= 5 => 3,
            >= 2 => 2,
            _ => 1
        };

        int MScore(decimal spend) => maxSpend > 0
            ? (int)Math.Ceiling((double)(spend / maxSpend) * 5)
            : 1;

        // ------- RFM-based segmentation -------
        // Champions: R>=4, F>=4, M>=4
        // Loyal:     R>=3, F>=3, M>=3
        // Promising: R>=4, F<=2
        // At Risk:   R<=2, F>=3
        // Lost:      R==1, F<=2
        // Others:    semua yang tidak masuk kategori di atas

        int championCount = 0, loyalCount = 0, promisingCount = 0, atRiskCount = 0, lostCount = 0, othersCount = 0;
        int newCount = 0, activeCount = 0, inactiveCount = 0;

        var cutoffNew = now.AddDays(-30);
        var cutoffActive = now.AddDays(-30);

        foreach (var c in customers)
        {
            var r = RScore(c.LastTransactionDate);
            var f = FScore(c.TransactionCount);
            var m = MScore(c.TotalSpend);

            // Segmentasi RFM advanced
            if (r >= 4 && f >= 4 && m >= 4)
                championCount++;
            else if (r >= 3 && f >= 3 && m >= 3)
                loyalCount++;
            else if (r >= 4 && f <= 2)
                promisingCount++;
            else if (r <= 2 && f >= 3)
                atRiskCount++;
            else if (r == 1 && f <= 2)
                lostCount++;
            else
                othersCount++;

            // Simple segmentation (untuk KPI)
            if (c.CreatedAt >= cutoffNew) newCount++;
            if (c.LastTransactionDate.HasValue && c.LastTransactionDate >= cutoffActive) activeCount++;
            if (!c.LastTransactionDate.HasValue || c.LastTransactionDate < now.AddDays(-90)) inactiveCount++;
        }

        var totalCustomers = customers.Count;

        var rfmSegments = new List<RfmSegmentItem>
        {
            new() {
                Segment = "Champions",
                Label = "🌟 Champions",
                Description = "Beli sering, banyak, dan baru-baru ini",
                Count = championCount,
                Color = "success",
                Icon = "🌟",
                ActionRecommendation = "Hadiahi, minta review & referral"
            },
            new() {
                Segment = "Loyal",
                Label = "💎 Pelanggan Loyal",
                Description = "Konsisten beli dengan spend tinggi",
                Count = loyalCount,
                Color = "info",
                Icon = "💎",
                ActionRecommendation = "Tawarkan tier upgrade & program VIP"
            },
            new() {
                Segment = "Promising",
                Label = "🚀 Pelanggan Baru Potensial",
                Description = "Baru aktif tapi belum sering beli",
                Count = promisingCount,
                Color = "primary",
                Icon = "🚀",
                ActionRecommendation = "Onboarding reward & edukasi program poin"
            },
            new() {
                Segment = "AtRisk",
                Label = "⚠️ Berisiko Churn",
                Description = "Dulu aktif tapi mulai jarang beli",
                Count = atRiskCount,
                Color = "warning",
                Icon = "⚠️",
                ActionRecommendation = "Kirim penawaran comeback & reminder poin"
            },
            new() {
                Segment = "Lost",
                Label = "😴 Pelanggan Hilang",
                Description = "Sudah lama tidak bertransaksi",
                Count = lostCount,
                Color = "error",
                Icon = "😴",
                ActionRecommendation = "Kampanye win-back dengan diskon eksklusif"
            },
            new() {
                Segment = "Others",
                Label = "📋 Lainnya",
                Description = "Belum masuk kategori utama",
                Count = othersCount,
                Color = "default",
                Icon = "📋",
                ActionRecommendation = "Analisis lebih lanjut pola transaksi"
            },
        };

        // Averages
        var avgSpend = customers.Any() ? customers.Average(c => (double)c.TotalSpend) : 0;
        var avgPoints = customers.Any() ? customers.Average(c => (double)c.TotalPoints) : 0;

        // Total transactions for retention calc
        var totalTx = await _context.Transactions.AsNoTracking().CountAsync(t => !t.IsDeleted, cancellationToken);
        var repeatTx = await _context.Transactions
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.CustomerId)
            .CountAsync(g => g.Count() > 1, cancellationToken);

        var retentionRate = totalCustomers > 0 ? (decimal)repeatTx / totalCustomers * 100 : 0;

        // Top spenders dengan Rank
        var topSpendersRaw = await _context.Customers
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.TotalSpend > 0)
            .OrderByDescending(c => c.TotalSpend)
            .Take(10)
            .Select(c => new
            {
                c.Id,
                c.MemberCode,
                c.FullName,
                c.Tier,
                c.TotalSpend,
                c.TotalPoints,
                TransactionCount = c.Transactions.Count(t => !t.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var topSpenders = topSpendersRaw.Select((c, i) => new TopCustomerItem
        {
            Id = c.Id,
            MemberCode = c.MemberCode,
            FullName = c.FullName,
            Tier = c.Tier.ToString(),
            TotalSpend = c.TotalSpend,
            TotalPoints = c.TotalPoints,
            TransactionCount = c.TransactionCount,
            Rank = i + 1
        }).ToList();

        // Upcoming birthdays (within 14 days)
        var birthdaysThisMonth = customers
            .Where(c => c.BirthDate.HasValue)
            .Select(c =>
            {
                var bd = c.BirthDate!.Value;
                var thisYearBd = new DateOnly(today.Year, bd.Month, bd.Day);
                if (thisYearBd < today)
                    thisYearBd = thisYearBd.AddYears(1);
                var daysUntil = thisYearBd.DayNumber - today.DayNumber;
                return new { Customer = c, NextBirthday = thisYearBd, DaysUntil = daysUntil };
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
                TotalPoints = x.Customer.TotalPoints,
                Tier = x.Customer.Tier.ToString(),
                DaysUntilBirthday = x.DaysUntil
            })
            .ToList();

        // Churn risk: no purchase > 45 days but were active (using RFM AtRisk + Lost)
        var cutoffChurn = now.AddDays(-45);
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
            TotalCustomers = totalCustomers,
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
            ChurnRiskCustomers = churnRisk,
            RfmSegments = rfmSegments,
        };
    }
}
