namespace Financial.Api.Domain
{
    public class CompanyIncome
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public required string Year { get; set; }
        public required decimal Value { get; set; }
    }
}
