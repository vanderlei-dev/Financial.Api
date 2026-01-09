using Financial.Api.Domain;
using Xunit;

namespace Financial.Test
{
    public class CalculateCompanyFundingTest
    {
        #region [ Helper Methods ]

        private Company CreateCompanyWithIncomes(int id, string entityName, params (string year, decimal value)[] incomes)
        {
            var company = new Company
            {
                Id = id,
                Cik = 1000 + id,
                EntityName = entityName,
                CompanyIncomes = incomes.Select(x => new CompanyIncome
                {
                    Id = id * 10 + int.Parse(x.year),
                    CompanyId = id,
                    Year = x.year,
                    Value = x.value
                }).ToList()
            };
            return company;
        }

        #endregion

        #region [ Valid Funding Calculation Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithValidIncomeData_ReturnsCompanyFundingWithNonZeroAmounts()
        {
            // Arrange
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2018", 10_000_000_000m),
                ("2019", 12_000_000_000m),
                ("2020", 14_000_000_000m),
                ("2021", 15_000_000_000m),
                ("2022", 16_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Apple Inc", result.Name);
            Assert.True(result.StandardFundableAmount > 0, "StandardFundableAmount should be greater than 0");
            Assert.True(result.SpecialFundableAmount > 0, "SpecialFundableAmount should be greater than 0");
        }

        [Fact]
        public void CalculateCompanyFunding_WithValidIncomeData_StandardFundableAmountEqualsBaseCalculation()
        {
            // Arrange
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 9_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();
            var expectedStandard = 9_000_000_000m * Company.StandardIncomeRate; // highest income * standard rate

            // Assert
            Assert.Equal(expectedStandard, result.StandardFundableAmount, precision: 2);
        }

        #endregion

        #region [ Income Threshold Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithHighIncome_AppliesLargeIncomeRate()
        {
            // Arrange - Income >= threshold
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", Company.LargeIncomeThreshold),
                ("2019", Company.LargeIncomeThreshold + 1_000_000_000m),
                ("2020", Company.LargeIncomeThreshold + 2_000_000_000m),
                ("2021", Company.LargeIncomeThreshold + 3_000_000_000m),
                ("2022", Company.LargeIncomeThreshold + 4_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();
            var expectedAmount = (Company.LargeIncomeThreshold + 4_000_000_000m) * Company.LargeIncomeRate;

            // Assert
            Assert.Equal(expectedAmount, result.StandardFundableAmount, precision: 2);
        }

        [Fact]
        public void CalculateCompanyFunding_WithStandardIncome_AppliesStandardRate()
        {
            // Arrange - Income < threshold
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 1_000_000_000m),
                ("2019", 2_000_000_000m),
                ("2020", 3_000_000_000m),
                ("2021", 4_000_000_000m),
                ("2022", 5_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();
            var expectedAmount = 5_000_000_000m * Company.StandardIncomeRate;

            // Assert
            Assert.Equal(expectedAmount, result.StandardFundableAmount, precision: 2);
        }

        [Fact]
        public void CalculateCompanyFunding_WithIncomeAtThreshold_AppliesLargeIncomeRate()
        {
            // Arrange - Income exactly at threshold
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", Company.LargeIncomeThreshold),
                ("2019", Company.LargeIncomeThreshold),
                ("2020", Company.LargeIncomeThreshold),
                ("2021", Company.LargeIncomeThreshold),
                ("2022", Company.LargeIncomeThreshold));

            // Act
            var result = company.CalculateCompanyFunding();
            var expectedAmount = Company.LargeIncomeThreshold * Company.LargeIncomeRate;

            // Assert
            Assert.Equal(expectedAmount, result.StandardFundableAmount, precision: 2);
        }

        #endregion

        #region [ Vowel Name Bonus Tests ]

        [Theory]
        [InlineData("Apple Inc")]
        [InlineData("orange Corp")]
        [InlineData("Uber Systems")]
        [InlineData("Intel Ltd")]
        [InlineData("ebay Industries")]
        public void CalculateCompanyFunding_WithVowelStartingName_AppliesVowelBonus(string companyName)
        {
            // Arrange
            var company = CreateCompanyWithIncomes(1, companyName,
                ("2018", 5_000_000_000m),
                ("2019", 5_500_000_000m),
                ("2020", 6_000_000_000m),
                ("2021", 6_500_000_000m),
                ("2022", 7_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 7_000_000_000m * Company.StandardIncomeRate;
            var expectedSpecial = baseAmount * Company.VowelBonus;
            Assert.Equal(expectedSpecial, result.SpecialFundableAmount, precision: 2);
            Assert.True(result.SpecialFundableAmount > result.StandardFundableAmount);
        }

        [Theory]
        [InlineData("Zebra Corp")]
        [InlineData("microsoft Inc")]
        [InlineData("Google LLC")]
        [InlineData("Facebook Inc")]
        [InlineData("netflix Corp")]
        public void CalculateCompanyFunding_WithConsonantStartingName_DoesNotApplyVowelBonus(string companyName)
        {
            // Arrange
            var company = CreateCompanyWithIncomes(1, companyName,
                ("2018", 5_000_000_000m),
                ("2019", 5_500_000_000m),
                ("2020", 6_000_000_000m),
                ("2021", 6_500_000_000m),
                ("2022", 7_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(result.StandardFundableAmount, result.SpecialFundableAmount, precision: 2);
        }

        #endregion

        #region [ Income Decline Adjustment Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithIncomeDecline_AppliesDeclinePenalty()
        {
            // Arrange - Income declines from 2021 to 2022
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 7_500_000_000m)); // Decline from 8B to 7.5B

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 8_000_000_000m * Company.StandardIncomeRate;
            var expectedWithPenalty = baseAmount * Company.IncomeDeclinedPenalty;
            Assert.Equal(expectedWithPenalty, result.SpecialFundableAmount, precision: 2);
            Assert.True(result.SpecialFundableAmount < result.StandardFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithIncomeGrowth_DoesNotApplyDeclinePenalty()
        {
            // Arrange - Income increases from 2021 to 2022
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 8_500_000_000m)); // Growth from 8B to 8.5B

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 8_500_000_000m * Company.StandardIncomeRate;
            Assert.Equal(baseAmount, result.SpecialFundableAmount, precision: 2);
            Assert.Equal(result.StandardFundableAmount, result.SpecialFundableAmount, precision: 2);
        }

        [Fact]
        public void CalculateCompanyFunding_WithIncomeFlat_DoesNotApplyDeclinePenalty()
        {
            // Arrange - Income stays flat from 2021 to 2022
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 8_000_000_000m)); // Flat income

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 8_000_000_000m * Company.StandardIncomeRate;
            Assert.Equal(baseAmount, result.SpecialFundableAmount, precision: 2);
        }

        #endregion

        #region [ Combined Adjustments Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithVowelNameAndIncomeGrowth_AppliesOnlyVowelBonus()
        {
            // Arrange - Apple (vowel) with income growth
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 9_000_000_000m)); // Growth

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 9_000_000_000m * Company.StandardIncomeRate;
            var expectedSpecial = baseAmount * Company.VowelBonus;
            Assert.Equal(expectedSpecial, result.SpecialFundableAmount, precision: 2);
            Assert.True(result.SpecialFundableAmount > result.StandardFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithVowelNameAndIncomeDecline_AppliesBothAdjustments()
        {
            // Arrange - Apple (vowel) with income decline
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 7_000_000_000m)); // Decline

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 8_000_000_000m * Company.StandardIncomeRate;
            var withVowelBonus = baseAmount * Company.VowelBonus;
            var withBothAdjustments = withVowelBonus * Company.IncomeDeclinedPenalty;
            Assert.Equal(withBothAdjustments, result.SpecialFundableAmount, precision: 2);
        }

        [Fact]
        public void CalculateCompanyFunding_WithConsonantNameAndIncomeDecline_AppliesOnlyDeclinePenalty()
        {
            // Arrange - Zebra (consonant) with income decline
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 7_000_000_000m)); // Decline

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 8_000_000_000m * Company.StandardIncomeRate;
            var expectedWithPenalty = baseAmount * Company.IncomeDeclinedPenalty;
            Assert.Equal(expectedWithPenalty, result.SpecialFundableAmount, precision: 2);
        }

        #endregion

        #region [ Missing Data Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithMissingYearInTotalPeriod_ReturnsZeroAmounts()
        {
            // Arrange - Missing 2018
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2019", 5_000_000_000m),
                ("2020", 6_000_000_000m),
                ("2021", 7_000_000_000m),
                ("2022", 8_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(0m, result.StandardFundableAmount);
            Assert.Equal(0m, result.SpecialFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithMissingYearInLastPeriods_ReturnsZeroAmounts()
        {
            // Arrange - Has all years but missing positive income in 2021
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 0m),
                ("2022", 8_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(0m, result.StandardFundableAmount);
            Assert.Equal(0m, result.SpecialFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithNegativeIncomeInLastYear_ReturnsZeroAmounts()
        {
            // Arrange - Last year has negative income
            var company = CreateCompanyWithIncomes(1, "Apple Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", -1_000_000_000m)); // Negative income

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(0m, result.StandardFundableAmount);
            Assert.Equal(0m, result.SpecialFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithNoIncomeData_ReturnsZeroAmounts()
        {
            // Arrange
            var company = new Company
            {
                Id = 1,
                Cik = 1001,
                EntityName = "Empty Company",
                CompanyIncomes = []
            };

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(0m, result.StandardFundableAmount);
            Assert.Equal(0m, result.SpecialFundableAmount);
        }

        #endregion

        #region [ Result Mapping Tests ]

        [Fact]
        public void CalculateCompanyFunding_MapsCompanyPropertiesToResult()
        {
            // Arrange
            var company = CreateCompanyWithIncomes(42, "Test Company Inc",
                ("2018", 5_000_000_000m),
                ("2019", 6_000_000_000m),
                ("2020", 7_000_000_000m),
                ("2021", 8_000_000_000m),
                ("2022", 9_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.Equal(42, result.Id);
            Assert.Equal("Test Company Inc", result.Name);
        }

        #endregion

        #region [ Large Income with Adjustments Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithLargeIncomeAndVowelName_AppliesBothCalculations()
        {
            // Arrange - Large income (>10B) with vowel name
            var company = CreateCompanyWithIncomes(1, "Uber Inc",
                ("2018", 11_000_000_000m),
                ("2019", 12_000_000_000m),
                ("2020", 13_000_000_000m),
                ("2021", 14_000_000_000m),
                ("2022", 15_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 15_000_000_000m * Company.LargeIncomeRate;
            var expectedStandard = baseAmount;
            var expectedSpecial = baseAmount * Company.VowelBonus;
            
            Assert.Equal(expectedStandard, result.StandardFundableAmount, precision: 2);
            Assert.Equal(expectedSpecial, result.SpecialFundableAmount, precision: 2);
        }

        [Fact]
        public void CalculateCompanyFunding_WithLargeIncomeAndDecline_AppliesBothCalculations()
        {
            // Arrange - Large income (>10B) with decline
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 11_000_000_000m),
                ("2019", 12_000_000_000m),
                ("2020", 13_000_000_000m),
                ("2021", 14_000_000_000m),
                ("2022", 13_000_000_000m)); // Decline

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var baseAmount = 14_000_000_000m * Company.LargeIncomeRate;
            var expectedWithPenalty = baseAmount * Company.IncomeDeclinedPenalty;
            
            Assert.Equal(baseAmount, result.StandardFundableAmount, precision: 2);
            Assert.Equal(expectedWithPenalty, result.SpecialFundableAmount, precision: 2);
        }

        #endregion

        #region [ Edge Case Tests ]

        [Fact]
        public void CalculateCompanyFunding_WithAllYearsAtMinimumValidIncome_CalculatesFunding()
        {
            // Arrange - Minimum positive income for all years
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 0.01m),
                ("2019", 0.01m),
                ("2020", 0.01m),
                ("2021", 0.01m),
                ("2022", 0.01m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            Assert.NotEqual(0m, result.StandardFundableAmount);
            Assert.NotEqual(0m, result.SpecialFundableAmount);
        }

        [Fact]
        public void CalculateCompanyFunding_WithVeryLargeIncome_HandlesCalculationCorrectly()
        {
            // Arrange - Very large income values
            var company = CreateCompanyWithIncomes(1, "Zebra Inc",
                ("2018", 100_000_000_000m),
                ("2019", 110_000_000_000m),
                ("2020", 120_000_000_000m),
                ("2021", 130_000_000_000m),
                ("2022", 140_000_000_000m));

            // Act
            var result = company.CalculateCompanyFunding();

            // Assert
            var expectedStandard = 140_000_000_000m * Company.LargeIncomeRate;
            Assert.Equal(expectedStandard, result.StandardFundableAmount, precision: 2);
            Assert.True(result.SpecialFundableAmount > 0);
        }

        #endregion
    }
}
