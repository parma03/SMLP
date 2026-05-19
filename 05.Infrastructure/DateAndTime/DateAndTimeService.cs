using Smlp.Application.Services.DateAndTime;

namespace Smlp.InfrastructureDateAndTime;

public class DateAndTimeService : IDateAndTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
