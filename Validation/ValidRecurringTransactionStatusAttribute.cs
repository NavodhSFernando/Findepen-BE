using System.ComponentModel.DataAnnotations;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.Validation
{
    public class ValidRecurringTransactionStatusAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Recurring transaction status is required");
            }

            var statusString = value.ToString();
            
            if (Enum.TryParse<RecurringTransactionStatus>(statusString, true, out _))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid recurring transaction status. Must be Active, Paused, or Cancelled");
        }
    }
}
