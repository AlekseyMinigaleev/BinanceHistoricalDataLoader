using API.Controllers.HistoricalData.Actions;
using FluentValidation.TestHelper;

namespace Tests.UnitTests.ControllerTests.HistoricalDataTests.LoadTests
{
    public class ValidatorTests
    {
        private readonly Load.Validator _validator = new Load.Validator();

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void Pairs_InvalidCount_ShouldHaveValidationError(int count)
        {
            var query = new Load.LoadQuery
            {
                Pairs = Enumerable.Repeat("BTCUSDT", count).ToList(),
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.Pairs);
        }

        [Fact]
        public void Pairs_ValidCount_ShouldNotHaveValidationError()
        {
            var query = new Load.LoadQuery
            {
                Pairs = ["BTCUSDT", "ETHUSDT"],
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow
            };

            var result = _validator.TestValidate(query);
            result.ShouldNotHaveValidationErrorFor(x => x.Pairs);
        }

        [Theory]
        [InlineData(null, "2023-01-01")]
        [InlineData("2023-01-01", null)]
        [InlineData("2023-01-02", "2023-01-01")]
        public void Dates_InvalidValues_ShouldHaveValidationError(
            string startDateStr, string endDateStr)
        {
            var query = new Load.LoadQuery
            {
                Pairs = ["BTCUSDT", "ETHUSDT"],

                StartDate = startDateStr is not null
                    ? DateTime.Parse(startDateStr)
                    : default,

                EndDate = endDateStr is not null
                    ? DateTime.Parse(endDateStr)
                    : default
            };

            var result = _validator.TestValidate(query);

            if (startDateStr is null)
                result.ShouldHaveValidationErrorFor(x => x.StartDate);

            if (endDateStr is null)
                result.ShouldHaveValidationErrorFor(x => x.EndDate);

            if (startDateStr is not null && endDateStr is not null)
                result.ShouldHaveValidationErrorFor(x => x);
        }

        [Fact]
        public void Dates_ValidRange_ShouldNotHaveValidationError()
        {
            var query = new Load.LoadQuery
            {
                Pairs = ["BTCUSDT", "ETHUSDT"],
                StartDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(-1)
            };

            var result = _validator.TestValidate(query);

            result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
            result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
        }

        [Fact]
        public void Dates_FutureDates_ShouldHaveValidationError()
        {
            var query = new Load.LoadQuery
            {
                Pairs = ["BTCUSDT", "ETHUSDT"],
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(2)
            };

            var result = _validator.TestValidate(query);

            result.ShouldHaveValidationErrorFor(x => x.StartDate);
        }
    }
}