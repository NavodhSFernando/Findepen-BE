using System.ComponentModel.DataAnnotations;
using FinDepen_Backend.Constants;

namespace FinDepen_Backend.Validation
{
    public class ValidCategoryAttribute : ValidationAttribute
    {
        public ValidCategoryAttribute()
        {
            ErrorMessage = $"Category must be one of the valid expense categories: {Categories.GetValidCategoriesString()}";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Category is required.");
            }

            string category = value.ToString()!;
            
            if (!Categories.IsValidCategory(category))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
} 