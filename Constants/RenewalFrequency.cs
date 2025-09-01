namespace FinDepen_Backend.Constants
{
    public enum RenewalFrequency
    {
        Weekly,
        Monthly,
        Yearly
    }

    public static class RenewalFrequencyExtensions
    {
        public static string GetDisplayName(this RenewalFrequency frequency)
        {
            return frequency switch
            {
                RenewalFrequency.Weekly => "Weekly",
                RenewalFrequency.Monthly => "Monthly",
                RenewalFrequency.Yearly => "Yearly",
                _ => "Monthly"
            };
        }

        public static int GetDaysInPeriod(this RenewalFrequency frequency)
        {
            return frequency switch
            {
                RenewalFrequency.Weekly => 7,
                RenewalFrequency.Monthly => 30,
                RenewalFrequency.Yearly => 365,
                _ => 30
            };
        }
    }
} 