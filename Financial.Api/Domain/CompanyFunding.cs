namespace Financial.Api.Domain
{
    public class CompanyFunding
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required decimal StandardFundableAmount { get; set; }
        public required decimal SpecialFundableAmount { get; set; }
    }
}
