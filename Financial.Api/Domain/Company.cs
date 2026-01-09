using Financial.Api.Common;

namespace Financial.Api.Domain
{
    public class Company
    {
        #region [ Constants and static values ]
        
        public const decimal LargeIncomeThreshold = 10_000_000_000m;
        public const decimal LargeIncomeRate = 0.1233m;
        public const decimal StandardIncomeRate = 0.2151m;
        public const decimal VowelBonus = 1.15m;
        public const decimal IncomeDeclinedPenalty = 0.75m;
        
        public const int MinYear = 2018;
        public const int MaxYear = 2022;

        // From min year to max year inclusive
        private static readonly string[] _totalPeriodYears = [.. Enumerable.Range(MinYear, MaxYear - MinYear + 1).Select(y => y.ToString())];

        // Last two years in the total period
        private static readonly string[] _positivePeriodYears = _totalPeriodYears[^2..];

        #endregion

        public int Id { get; set; }
        public required int Cik { get; set; }
        public required string EntityName { get; set; }
        public ICollection<CompanyIncome> CompanyIncomes { get; set; } = [];

        public CompanyFunding CalculateCompanyFunding()
        {
            decimal standardFundableAmount = 0;
            decimal specialFundableAmount = 0;
            
            if (HaveIncomeInTotalPeriod() && HavePositiveIncomeInLastPeriods())
            {
                var highestIncome = GetHighestIncomeInRange(MinYear, MaxYear);
                standardFundableAmount = CalculateStandardFundableAmount(highestIncome);
                specialFundableAmount = standardFundableAmount;
            }
            
            if (specialFundableAmount > 0)
            {
                ApplyVowelNameBonus(ref specialFundableAmount);
                ApplyIncomeDeclineAdjustment(ref specialFundableAmount);
            }

            return new CompanyFunding
            {
                Id = Id,
                Name = EntityName,
                StandardFundableAmount = standardFundableAmount,
                SpecialFundableAmount = specialFundableAmount
            };
        }

        private decimal GetHighestIncomeInRange(int minYear, int maxYear)
        {
            return CompanyIncomes
                .Where(ci => int.Parse(ci.Year) >= minYear && int.Parse(ci.Year) <= maxYear)
                .Max(ci => ci.Value);
        }

        private static decimal CalculateStandardFundableAmount(decimal highestIncome)
        {
            return highestIncome >= LargeIncomeThreshold
                ? highestIncome * LargeIncomeRate
                : highestIncome * StandardIncomeRate;
        }

        private void ApplyVowelNameBonus(ref decimal specialFundableAmount)
        {
            if (RegexUtils.StartWithVowelRegex().IsMatch(EntityName))
                specialFundableAmount *= VowelBonus;
        }

        private void ApplyIncomeDeclineAdjustment(ref decimal specialFundableAmount)
        {
            var incomeLastYear = GetIncomeForYear(MaxYear);
            var incomeLastButOneYear = GetIncomeForYear(MaxYear - 1);
            
            if (incomeLastYear < incomeLastButOneYear)
                specialFundableAmount *= IncomeDeclinedPenalty;
        }

        private decimal GetIncomeForYear(int year)
        {
            return CompanyIncomes.Single(i => i.Year == year.ToString()).Value;
        }

        private bool HaveIncomeInTotalPeriod()
        {            
            return _totalPeriodYears.All(year => CompanyIncomes.Any(i => i.Year == year));
        }

        private bool HavePositiveIncomeInLastPeriods()
        {            
            return _positivePeriodYears.All(year => CompanyIncomes.Any(i => i.Year == year && i.Value > 0));
        }
    }
}
