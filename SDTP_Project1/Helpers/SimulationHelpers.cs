using System;
using System.Linq;

namespace SDTP_Project1.Helpers
{
    /// <summary>
    /// Provides correlated sampling of pollutant values so that the overall AQI category
    /// follows the global weights you specify, rather than six independent draws.
    /// </summary>
    public static class SimulationHelpers
    {
        // ─── 1) Tweak these weights as needed ───
        public static readonly double GoodWeight = 0.3;
        public static readonly double ModerateWeight = 0.3;
        public static readonly double UnhealthySWeight = 0.1; // Still unchanged
        public static readonly double UnhealthyWeight = 0.1;
        public static readonly double VeryUnhealthyWeight = 0.1;
        public static readonly double HazardousWeight = 0.1;

        /// <summary>
        /// Must match the order of the weight variables and the pollutant band arrays below.
        /// </summary>
        public enum AqiCategory
        {
            Good = 0,
            Moderate = 1,
            UnhealthyS = 2,
            Unhealthy = 3,
            VeryUnhealthy = 4,
            Hazardous = 5
        }

        // ─── Global category sampler ───
        private static readonly (AqiCategory Cat, double Weight)[] GlobalWeights = {
            (AqiCategory.Good,          GoodWeight),
            (AqiCategory.Moderate,      ModerateWeight),
            (AqiCategory.UnhealthyS,    UnhealthySWeight),
            (AqiCategory.Unhealthy,     UnhealthyWeight),
            (AqiCategory.VeryUnhealthy, VeryUnhealthyWeight),
            (AqiCategory.Hazardous,     HazardousWeight)
        };

        /// <summary>
        /// Samples one AQI category according to the six weights above.
        /// </summary>
        public static AqiCategory SampleGlobalCategory()
        {
            double total = GlobalWeights.Sum(x => x.Weight);
            double roll = Random.Shared.NextDouble() * total;
            double cum = 0;
            foreach (var (cat, w) in GlobalWeights)
            {
                cum += w;
                if (roll <= cum) return cat;
            }
            // fallback
            return GlobalWeights[^1].Cat;
        }

        // ─── 2) Define pollutant bands (Min, Max, Weight var for reference) ───
       

        private static readonly (double Min, double Max, double Weight)[] PM25_Ranges = {
            (  0.0,   12.0,  GoodWeight),
            ( 12.1,   35.4,  ModerateWeight),
            ( 35.5,   55.4,  UnhealthySWeight),
            ( 55.5,  150.4,  UnhealthyWeight),
            (150.5,  250.4,  VeryUnhealthyWeight),
            (250.5,  500.4,  HazardousWeight)
        };

        private static readonly (double Min, double Max, double Weight)[] PM10_Ranges = {
            (   0.0,   54.0,  GoodWeight),
            (  55.0,  154.0,  ModerateWeight),
            ( 155.0,  254.0,  UnhealthySWeight),
            ( 255.0,  354.0,  UnhealthyWeight),
            ( 355.0,  424.0,  VeryUnhealthyWeight),
            ( 425.0,  604.0,  HazardousWeight)
        };

        private static readonly (double Min, double Max, double Weight)[] O3_Ranges = {
            (0.000, 0.054,  GoodWeight),
            (0.055, 0.070,  ModerateWeight),
            (0.071, 0.085,  UnhealthySWeight),
            (0.086, 0.105,  UnhealthyWeight),
            (0.106, 0.200,  VeryUnhealthyWeight),
            (0.201, 0.500,  HazardousWeight)
        };

        private static readonly (double Min, double Max, double Weight)[] NO2_Ranges = {
            (0.000, 0.053,  GoodWeight),
            (0.054, 0.100,  ModerateWeight),
            (0.101, 0.360,  UnhealthySWeight),
            (0.361, 0.649,  UnhealthyWeight),
            (0.650, 1.249,  VeryUnhealthyWeight),
            (1.250, 2.000,  HazardousWeight)
        };

        private static readonly (double Min, double Max, double Weight)[] SO2_Ranges = {
            (0.000, 0.035,  GoodWeight),
            (0.036, 0.075,  ModerateWeight),
            (0.076, 0.185,  UnhealthySWeight),
            (0.186, 0.304,  UnhealthyWeight),
            (0.305, 0.604,  VeryUnhealthyWeight),
            (0.605, 1.000,  HazardousWeight)
        };

        private static readonly (double Min, double Max, double Weight)[] CO_Ranges = {
            ( 0.0,   4.4,    GoodWeight),
            ( 4.5,   9.4,    ModerateWeight),
            ( 9.5,  12.4,    UnhealthySWeight),
            (12.5,  15.4,    UnhealthyWeight),
            (15.5,  30.0,    VeryUnhealthyWeight),
            (30.1, 100.0,    HazardousWeight)
        };

        // ─── 3) Helpers to extract Min/Max and sample uniformly within that band ───

        private static (double Min, double Max) GetBand((double Min, double Max, double Weight)[] ranges, AqiCategory cat)
        {
            var idx = (int)cat;
            if (idx < 0 || idx >= ranges.Length)
                throw new ArgumentOutOfRangeException(nameof(cat));
            return (ranges[idx].Min, ranges[idx].Max);
        }

        public static double SamplePollutantByCategory(
            (double Min, double Max, double Weight)[] ranges,
            AqiCategory cat)
        {
            var (min, max) = GetBand(ranges, cat);
            return min + Random.Shared.NextDouble() * (max - min);
        }

        /// <summary>
        /// Picks one global AQI category, then draws all six pollutant values within that category.
        /// </summary>
        public static (double pm25, double pm10, double o3, double no2, double so2, double co)
            SampleCorrelatedReadings()
        {
            var cat = SampleGlobalCategory();
            return (
                SamplePollutantByCategory(PM25_Ranges, cat),
                SamplePollutantByCategory(PM10_Ranges, cat),
                SamplePollutantByCategory(O3_Ranges, cat),
                SamplePollutantByCategory(NO2_Ranges, cat),
                SamplePollutantByCategory(SO2_Ranges, cat),
                SamplePollutantByCategory(CO_Ranges, cat)
            );
        }

        // keep Round2 here to avoid referencing Math.Round elsewhere
        public static double Round2(double v) => Math.Round(v, 2);
    }
}
