using Moq;
using Xunit;

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorTest
    {
        [Fact]
        public void Evaluate_should_accept_high_income_applications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { GrossAnnualIncome = 100_000 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void Evaluate_should_refer_young_applications()
        {
            Mock<IFrequentFlyerNumberValidator> mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(v => v.IsValid(It.IsAny<string>()))
                .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 19 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void Evaluate_should_decline_low_income_applications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(x => x.IsValid(It.Is<string>(n => n.StartsWith("x"))))
                         .Returns(true);
            //mockValidator.Setup(x => x.IsValid(It.IsIn("x", "y", "z"))).Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "x"
            };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void Evaluate_should_refer_invalid_frequentFlyer_applications()
        {
            // Arrange
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

            mockValidator.Setup(v => v.IsValid(It.IsAny<string>()))
                         .Returns(true);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();

            // Act
            CreditCardApplicationDecision decision = sut.Evaluate(application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
    }
}
