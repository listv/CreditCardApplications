using System;

namespace CreditCardApplications
{
    public class FrequentFlyerNumberValidator : IFrequentFlyerNumberValidator
    {
        public bool IsValid(string frequentFlyerNumber)
        {
            throw new System.NotImplementedException("For demo purposes");
        }

        public void IsValid(string frequentFlyerNumber, out bool isValid)
        {
            throw new System.NotImplementedException("For demo purposes");
        }

        public IServiceInformation ServiceInformation { get => throw new NotImplementedException("For demo purposes"); }

        public ValidationMode ValidationMode
        {
            get => throw new NotImplementedException("For demo purposes");
            set => throw new NotImplementedException("For demo purposes");
        }
    }
}
