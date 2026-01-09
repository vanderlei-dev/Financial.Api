using Microsoft.AspNetCore.Mvc;

namespace Financial.Api.Endpoints.Import
{
    public static class ImportCompanyEndpoint
    {
        public static void MapImportCompanyEndpoint(this RouteGroupBuilder group)
        {
            group.MapPost("import", HandleAsync)
                 .WithSummary("Import or replace predefined companies from SEC’s EDGAR API");
        }

        private static async Task<IResult> HandleAsync([FromServices] IImportCompanyService importService)
        {            
            await importService.HandleAsync();
            return TypedResults.Ok();
        }
    }
}
