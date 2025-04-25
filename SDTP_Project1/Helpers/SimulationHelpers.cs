using System;
using System.Linq;

namespace SDTP_Project1.Helpers
{
    public static class SimulationHelpers
    {
        // Generic sampler
        public static double SampleFromWeightedRanges((double Min, double Max, double Weight)[] ranges)
        {
            var totalWeight = ranges.Sum(r => r.Weight);
            var roll = Random.Shared.NextDouble() * totalWeight;
            double cum = 0;

            foreach (var r in ranges)
            {
                cum += r.Weight;
                if (roll <= cum)
                    return r.Min + Random.Shared.NextDouble() * (r.Max - r.Min);
            }

            // Fallback (should never hit)
            var last = ranges[^1];
            return last.Min + Random.Shared.NextDouble() * (last.Max - last.Min);
        }

        // Round to 2 decimals
        public static double Round2(double v) => Math.Round(v, 2);

        // Pollutant-specific samplers:

        private static readonly (double Min, double Max, double Weight)[] PM25_Ranges = {
            (0.0,   12.0,   0.35),
            (12.1,  35.4,   0.30),
            (35.5,  55.4,   0.15),
            (55.5, 150.4,   0.10),
            (150.5,250.4,   0.07),
            (250.5,500.4,   0.03)
        };
        public static double SamplePM25()
            => SampleFromWeightedRanges(PM25_Ranges);

        private static readonly (double Min, double Max, double Weight)[] PM10_Ranges = {
            (0.0,   54.0,   0.30),
            (55.0, 154.0,   0.30),
            (155.0,254.0,  0.20),
            (255.0,354.0,  0.10),
            (355.0,424.0,  0.07),
            (425.0,604.0,  0.03)
        };
        public static double SamplePM10()
            => SampleFromWeightedRanges(PM10_Ranges);

        private static readonly (double Min, double Max, double Weight)[] O3_Ranges = {
            (0.000, 0.054, 0.40),
            (0.055, 0.070, 0.35),
            (0.071, 0.085, 0.15),
            (0.086, 0.105, 0.08),
            (0.106, 0.200, 0.02)
        };
        public static double SampleO3()
            => SampleFromWeightedRanges(O3_Ranges);

        private static readonly (double Min, double Max, double Weight)[] NO2_Ranges = {
            (0.000, 0.053, 0.40),
            (0.054, 0.100, 0.30),
            (0.101, 0.360, 0.20),
            (0.361, 0.649, 0.08),
            (0.650, 1.249, 0.02)
        };
        public static double SampleNO2()
            => SampleFromWeightedRanges(NO2_Ranges);

        private static readonly (double Min, double Max, double Weight)[] SO2_Ranges = {
            (0.000, 0.035, 0.40),
            (0.036, 0.075, 0.30),
            (0.076, 0.185, 0.20),
            (0.186, 0.304, 0.08),
            (0.305, 0.604, 0.02)
        };
        public static double SampleSO2()
            => SampleFromWeightedRanges(SO2_Ranges);

        private static readonly (double Min, double Max, double Weight)[] CO_Ranges = {
            (0.0,   4.4,    0.50),
            (4.5,   9.4,    0.30),
            (9.5,  12.4,    0.15),
            (12.5, 15.4,    0.05)
        };
        public static double SampleCO()
            => SampleFromWeightedRanges(CO_Ranges);
    }
}
