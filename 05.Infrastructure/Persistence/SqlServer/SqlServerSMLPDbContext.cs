using Microsoft.EntityFrameworkCore;
using Smlp.Application.Services.CurrentUser;
using Smlp.Application.Services.DateAndTime;
using Smlp.Application.Services.DomainEvent;
using Smlp.Infrastructure.Persistence.Common.Extensions;
using Smlp.Infrastructure.Persistence.SqlServer.Configuration;

namespace Smlp.Infrastructure.Persistence.SqlServer;

public class SqlServerSmlpDbContext : SmlpDbContext
{
    public SqlServerSmlpDbContext(
        DbContextOptions<SqlServerSmlpDbContext> options,
        ICurrentUserService currentUser,
        IDateAndTimeService dateTime,
        IDomainEventService domainEvent) : base(options)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _domainEvent = domainEvent;
    }

    public SqlServerSmlpDbContext(DbContextOptions<SqlServerSmlpDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromNameSpace(typeof(AuditConfiguration).Namespace!);
        base.OnModelCreating(builder);
    }
}