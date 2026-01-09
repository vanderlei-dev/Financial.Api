using Financial.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Api.Infra
{
    public class IncomeConfiguration : IEntityTypeConfiguration<CompanyIncome>
    {
        public void Configure(EntityTypeBuilder<CompanyIncome> builder)
        {
            builder.ToTable("CompanyIncomes");
            builder.HasKey(x => x.Id);       
        }
    }
}
