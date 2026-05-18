using Microsoft.EntityFrameworkCore;
using Psmp.Application.Services.CurrentUser;
using Psmp.Application.Services.DateAndTime;
using Psmp.Application.Services.DomainEvent;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Infrastructure.Persistence.SqlServer.Configuration;

namespace Psmp.Infrastructure.Persistence.SqlServer;

public class SqlServerSMLPDbContext : PsmpDbContext
{
    public SqlServerSMLPDbContext(
        DbContextOptions<SqlServerSMLPDbContext> options,
        ICurrentUserService currentUser,
        IDateAndTimeService dateTime,
        IDomainEventService domainEvent) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _domainEvent = domainEvent;
    }

    public SqlServerSMLPDbContext(DbContextOptions<SqlServerSMLPDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromNameSpace(typeof(AuditConfiguration).Namespace!);

        base.OnModelCreating(builder);
    }
}
