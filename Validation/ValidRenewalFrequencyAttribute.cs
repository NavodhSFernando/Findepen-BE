using System.ComponentModel.DataAnnotations;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.Validation
{
    public class ValidRenewalFrequencyAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Renewal frequency is required");
            }

            var frequencyString = value.ToString();
            
            if (Enum.TryParse<RenewalFrequency>(frequencyString, true, out _))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid renewal frequency. Must be Weekly, Monthly, or Yearly");
        }
    }
} 