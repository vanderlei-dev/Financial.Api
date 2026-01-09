using Financial.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Financial.Api.Infra
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");
            builder.HasKey(x => x.Id);

            builder
                .HasMany(x => x.CompanyIncomes)
                .WithOne()
                .HasForeignKey(x => x.CompanyId);            
        }
    }
}
