using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Psmp.Application.Services.Persistence;
using Psmp.Domain.Entities.Applicants;
using Psmp.Infrastructure.Persistence.Common.Constants;
using Psmp.Infrastructure.Persistence.Common.Extensions;
using Psmp.Shared.Common.Constants;

namespace Psmp.Infrastructure.Persistence.SqlServer.Configuration;

public class ApplicantBankAccountConfiguration : IEntityTypeConfiguration<ApplicantBankAccount>
{
    public void Configure(EntityTypeBuilder<ApplicantBankAccount> builder)
    {
        builder.ToTable($"{nameof(TableGroupFor.Applicant)}.{nameof(IPsmpDbContext.ApplicantBankAccounts)}", nameof(Psmp));
        builder.ConfigureCreatableProperties();
        builder.ConfigureModifiableProperties();

        if (builder is not null)
        {
            builder.Property(e => e.AccountNumber).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));
            builder.Property(e => e.BankBranch).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.LongText));
            builder.Property(e => e.BankCity).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.BankCountry).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.BankName).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.AccountHolder).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.MediumText));
            builder.Property(e => e.Currency).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));
            builder.Property(e => e.PhoneNumber).HasColumnType(CommonColumnTypes.Nvarchar(CommonMaximumLengthFor.SimpleName));

            builder.HasOne(e => e.Applicant)
                       .WithMany(e => e.ApplicantBankAccounts)
                       .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
