using System.ComponentModel.DataAnnotations;

namespace NashTech_TCG_API.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class GreaterThanZeroAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            
            if (value == null)
            {
                return ValidationResult.Success;
            }

            try
            {
                var convertedValue = Convert.ToDecimal(value);
                if (convertedValue > 0)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be greater than 0.");
                }
            }
            catch (Exception)
            {
                // Nếu convert lỗi (ví dụ value không phải số)
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is not a valid number.");
            }
        }
    }
}
