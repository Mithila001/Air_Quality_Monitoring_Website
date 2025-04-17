using System;

namespace SDTP_Project1.Models
{
    /// <summary>
    /// Binds to the "DevModeOptions" section in appsettings.json.
    /// </summary>
    public class DevModeOptions
    {
        /// <summary>If true, run in fast dev‐mode interval.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Seconds between simulation runs when dev‐mode is ON.</summary>
        public int FastIntervalSeconds { get; set; } = 5;

        /// <summary>Interval when dev‐mode is OFF (production). Default 8 hours.</summary>
        public TimeSpan ProductionInterval { get; set; } = TimeSpan.FromHours(8);
    }
}
