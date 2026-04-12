using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Smlp.Shared.Audits.Options;
using Smlp.Shared.Documents.Options;
using Smlp.Shared.Emails.Options;

namespace Smlp.Shared
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            #region Essential Options

            services.AddAuditOptions(configuration);

            #endregion Essential Options

            #region Business Options

            services.AddDocumentOptions(configuration);
            services.AddEmailOptions(configuration);

            #endregion Business Options

            return services;
        }
    }
}
