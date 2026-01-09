using Financial.Api.Domain;
using Financial.Api.Infra;
using Microsoft.EntityFrameworkCore;

namespace Financial.Api.Endpoints.Import
{
    public class ListCompanyService(AppDbContext dbContext) : IListCompanyService
    {
        public async Task<List<CompanyFunding>> HandleAsync(char? startsWithLetterFilter)
        {
            var query = dbContext.Companies.AsNoTracking();

            if (startsWithLetterFilter != null)
            {
                var filterStringToUpper = startsWithLetterFilter.ToString()!.ToUpper();
                query = query.Where(x => x.EntityName.ToUpper().StartsWith(filterStringToUpper!));
            }

            return await query
                .Include(i => i.CompanyIncomes)
                .Select(x => x.CalculateCompanyFunding())
                .ToListAsync();
        }
    }
}
