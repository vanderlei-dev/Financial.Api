using Financial.Api.Domain;
using Financial.Api.Endpoints.Import;
using Financial.Api.Infra;
using Financial.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Financial.Test
{
    public class ListCompanyServiceTest
    {
        private readonly Mock<AppDbContext> _mockDbContext;

        public ListCompanyServiceTest()
        {
            _mockDbContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        }

        #region [ HandleAsync - No Filter Tests ]

        [Fact]
        public async Task HandleAsync_WithoutFilter_ReturnsAllCompanies()
        {
            // Arrange
            var companies = new List<Company>
            {
                new() { Id = 1, Cik = 1, EntityName = "Apple Inc", CompanyIncomes = [] },
                new() { Id = 2, Cik = 2, EntityName = "Microsoft Corp", CompanyIncomes = [] },
                new() { Id = 3, Cik = 3, EntityName = "Amazon Inc", CompanyIncomes = [] }
            };

            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            var result = await service.HandleAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task HandleAsync_WithoutFilter_ReturnsEmptyList_WhenNoCompaniesExist()
        {
            // Arrange
            var companies = new List<Company>();

            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            var result = await service.HandleAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region [ HandleAsync - With Letter Filter Tests ]

        [Theory]
        [InlineData('A')]
        [InlineData('m')]
        [InlineData('B')]
        public async Task HandleAsync_WithLetterFilter_ReturnsOnlyCompaniesStartingWithFilterLetter(char filterLetter)
        {
            // Arrange
            var companies = new List<Company>
            {
                new() { Id = 1, Cik = 1, EntityName = "Apple Inc", CompanyIncomes = [] },
                new() { Id = 2, Cik = 2, EntityName = "Microsoft Corp", CompanyIncomes = [] },
                new() { Id = 3, Cik = 3, EntityName = "Amazon Inc", CompanyIncomes = [] }
            };

            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            var result = await service.HandleAsync(filterLetter);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, company => Assert.StartsWith(filterLetter.ToString(), company.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task HandleAsync_WithLetterFilter_ReturnsEmptyList_WhenNoCompaniesMatchFilter()
        {
            // Arrange
            var companies = new List<Company>
            {
                new() { Id = 1, Cik = 1, EntityName = "Apple Inc", CompanyIncomes = [] },
                new() { Id = 2, Cik = 2, EntityName = "Microsoft Corp", CompanyIncomes = [] }
            };

            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            var result = await service.HandleAsync('Z');

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region [ HandleAsync - CompanyFunding Calculation Tests ]

        [Fact]
        public async Task HandleAsync_ReturnsCompanyFundingObjects_WithCalculatedFundingAmounts()
        {
            // Arrange
            var companies = new List<Company>
            {
                new()
                {
                    Id = 1,
                    Cik = 1,
                    EntityName = "Test Company",
                    CompanyIncomes = new List<CompanyIncome>
                    {
                        new() { Id = 1, CompanyId = 1, Year = "2018", Value = 5_000_000_000m },
                        new() { Id = 2, CompanyId = 1, Year = "2021", Value = 6_000_000_000m }
                    }
                }
            };

            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            var result = await service.HandleAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);

            var fundingResult = result.First();
            Assert.Equal(1, fundingResult.Id);
            Assert.Equal("Test Company", fundingResult.Name);
            Assert.IsType<decimal>(fundingResult.StandardFundableAmount);
            Assert.IsType<decimal>(fundingResult.SpecialFundableAmount);
        }

        #endregion

        #region [ HandleAsync - Dependency Usage Tests ]

        [Fact]
        public async Task HandleAsync_InvokesDbContextCompanies_WhenCalled()
        {
            // Arrange
            var companies = new List<Company>();
            var mockQueryable = companies.AsQueryable().BuildMockDbSet();
            _mockDbContext.Setup(x => x.Companies).Returns(mockQueryable.Object);

            var service = new ListCompanyService(_mockDbContext.Object);

            // Act
            await service.HandleAsync(null);

            // Assert
            _mockDbContext.Verify(x => x.Companies, Times.Once);
        }

        #endregion
    }
}
