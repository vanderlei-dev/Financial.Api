using Financial.Api.Common;
using Financial.Api.Domain;
using Financial.Api.Dto;
using Financial.Api.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Financial.Api.Endpoints.Import
{
    public class ImportCompanyService(ILogger<ImportCompanyService> logger, IHttpClientFactory httpClientFactory, IOptions<List<string>> companiesCiks, AppDbContext dbContext) : IImportCompanyService
    {
        public const string EdgarValidForm = "10-K";
        public const string EdgarUserAgent = "PostmanRuntime/7.51.0";

        public async Task HandleAsync()
        {
            using var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://data.sec.gov/");
            client.DefaultRequestHeaders.Add("User-Agent", EdgarUserAgent);
            client.DefaultRequestHeaders.Add("Accept", "*/*");

            var ciksList = companiesCiks.Value;
            var companies = await ProcessCompanies(client, ciksList);

            // Clear any data before import
            await dbContext.Companies.ExecuteDeleteAsync();

            await dbContext.Companies.AddRangeAsync(companies);
            await dbContext.SaveChangesAsync();
        }

        private async Task<ConcurrentBag<Company>> ProcessCompanies(HttpClient client, List<string> ciksList)
        {
            var companies = new ConcurrentBag<Company>();

            await Parallel.ForEachAsync(ciksList, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, async (cik, cancellationToken) =>
            {
                var cikFormatted = cik.PadLeft(10, '0');

                var result = await client.GetAsync($"api/xbrl/companyfacts/CIK{cikFormatted}.json", cancellationToken);
                if (!result.IsSuccessStatusCode)
                {
                    logger.LogWarning("Failed to fetch data for CIK {Cik}. Status Code: {StatusCode}", cikFormatted, result.StatusCode);
                    return;
                }

                var edgarCompanyInfo = await result.Content.ReadFromJsonAsync<EdgarCompanyInfo>(cancellationToken: cancellationToken);

                var financialCompanyInfo = new Company()
                {
                    Cik = edgarCompanyInfo!.Cik,
                    EntityName = edgarCompanyInfo.EntityName,
                    CompanyIncomes = edgarCompanyInfo.Facts?.UsGaap?.NetIncomeLoss?.Units?.Usd?
                        .Where(x => x.Form == EdgarValidForm
                                    && RegexUtils.EdgarYearFormatRegex().IsMatch(x.Frame ?? string.Empty))
                        .Select(x => new CompanyIncome
                        {
                            Year = x.Frame[^4..],
                            Value = x.Val
                        })
                        .ToList() ?? []
                };

                companies.Add(financialCompanyInfo);
            });

            return companies;
        }
    }
}
