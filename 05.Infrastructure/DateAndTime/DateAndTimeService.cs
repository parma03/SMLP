using Psmp.Application.Services.DateAndTime;

namespace Psmp.InfrastructureDateAndTime;

public class DateAndTimeService : IDateAndTimeService
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}
