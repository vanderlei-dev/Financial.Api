using Microsoft.AspNetCore.Mvc;

namespace Financial.Api.Endpoints.Import
{
    public static class ListCompanyEndpoint
    {
        public static void MapListCompanyEndpoint(this RouteGroupBuilder group)
        {
            group.MapGet("", HandleAsync)
                 .WithSummary("List the companies with their funding information optionally filtering by the starting letter");
        }

        private static async Task<IResult> HandleAsync([FromServices] IListCompanyService listCompanyService, [FromQuery] char? startsWithLetterFilter)
        {            
            return TypedResults.Ok(await listCompanyService.HandleAsync(startsWithLetterFilter));            
        }
    }
}
