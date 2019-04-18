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
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns(GetLicenseKeyExpiryString);

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
            //mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns(GetLicenseKeyExpiryStringOk);
            mockValidator.DefaultValue = DefaultValue.Mock;

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
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            mockValidator.Setup(v => v.IsValid(It.IsAny<string>()))
                .Returns(true);
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns(GetLicenseKeyExpiryString);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication();

            // Act
            CreditCardApplicationDecision decision = sut.Evaluate(application);

            // Assert
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void EvaluateUsingOut_should_decline_low_income_applications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            bool isValid = true;
            mockValidator.Setup(v => v.IsValid(It.IsAny<string>(), out isValid));

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                FrequentFlyerNumber = "a"
            };

            CreditCardApplicationDecision decision = sut.EvaluateUsingOut(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void Evaluate_should_refer_when_license_key_expired()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(v => v.IsValid(It.IsAny<string>())).Returns(true);

            //var mockLicenseData=new Mock<ILicenseData>();
            //mockLicenseData.Setup(ld => ld.LicenseKey).Returns("EXPIRED");

            //var mockServiceInfo=new Mock<IServiceInformation>();
            //mockServiceInfo.Setup(si => si.License).Returns(mockLicenseData.Object);

            //mockValidator.Setup(v => v.ServiceInformation).Returns(mockServiceInfo.Object);

            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns(GetLicenseKeyExpiryString);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication { Age = 42 };

            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void Evaluate_should_use_detailed_lookup_for_older_applications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");
            //mockValidator.SetupProperty(v => v.ValidationMode);

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication() { Age = 30 };
            CreditCardApplicationDecision decision = sut.Evaluate(application);
            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void Evaluate_should_validate_frequentFlyerNumber_for_low_income_applications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication() { FrequentFlyerNumber = "q" };

            sut.Evaluate(application);

            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()));
        }

        //[Fact]
        //public void Evaluate_should_validate_frequentFlyerNumber_for_low_income_applications_CustomMessage()
        //{
        //    var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
        //    mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");

        //    var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        //    var application = new CreditCardApplication();

        //    sut.Evaluate(application);

        //    mockValidator.Verify(x => x.IsValid(It.IsNotNull<string>()), "Frequent flyer number passed should not be null");
        //}

        [Fact]
        public void Evaluate_not_validate_FrequentFlyerNumber_for_high_income_applications()
        {
            var mockValidator=new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut=new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication() {GrossAnnualIncome = 100_000};
            sut.Evaluate(application);

            mockValidator.Verify(v=>v.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Evaluate_should_check_lisenceKey_for_lowIncome_applications()
        {
            var mockValidator=new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut=new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication() {GrossAnnualIncome = 99_000};
            sut.Evaluate(application);
            mockValidator.VerifyGet(x=>x.ServiceInformation.License.LicenseKey);
        }

        [Fact]
        public void Evaluate_should_set_detailedLookup_for_older_applications()
        {
            var mockValidator=new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(v => v.ServiceInformation.License.LicenseKey).Returns("OK");

            var sut=new CreditCardApplicationEvaluator(mockValidator.Object);

            var application = new CreditCardApplication() {Age = 30};
            sut.Evaluate(application);

            mockValidator.VerifySet(v=>v.ValidationMode=ValidationMode.Detailed);
        }

        private string GetLicenseKeyExpiryString() => "EXPIRED";
        private string GetLicenseKeyExpiryStringOk() => "OK";
    }
}
