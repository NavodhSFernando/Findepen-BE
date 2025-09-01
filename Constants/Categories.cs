namespace FinDepen_Backend.Constants
{
    public static class Categories
    {
        public static readonly string[] ValidCategories = new[]
        {
            "Food",
            "Grocery", 
            "Rent",
            "Education",
            "Health",
            "Entertainment",
            "Transportation",	
            "Miscellaneous"
        };

        public static bool IsValidCategory(string category)
        {
            return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetValidCategoriesString()
        {
            return string.Join(", ", ValidCategories);
        }
    }
} 