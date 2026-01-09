using Financial.Api.Domain;

namespace Financial.Api.Endpoints.Import
{
    public interface IListCompanyService
    {
        Task<List<CompanyFunding>> HandleAsync(char? startsWithLetterFilter);
    }
}