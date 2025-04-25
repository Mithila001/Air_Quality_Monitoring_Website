// Helpers/AqiCalculator.cs
using System;
using System.Linq;

namespace SDTP_Project1.Helpers
{
    public static class AqiCalculator
    {
        // Format: (C_low, C_high, I_low, I_high)
        private static readonly (double Cl, double Ch, int Il, int Ih)[] PM25 = {
            (0.0,  12.0,   0,   50),
            (12.1, 35.4,  51,  100),
            (35.5, 55.4, 101,  150),
            (55.5,150.4, 151,  200),
            (150.5,250.4,201,  300),
            (250.5,350.4,301,  400),
            (350.5,500.4,401,  500),
        };
        private static readonly (double Cl, double Ch, int Il, int Ih)[] PM10 = {
            (0,   54,   0,   50),
            (55, 154,  51,  100),
            (155,254, 101,  150),
            (255,354, 151,  200),
            (355,424, 201,  300),
            (425,504, 301,  400),
            (505,604, 401,  500),
        };
        private static readonly (double Cl, double Ch, int Il, int Ih)[] O3 = {
            (0.000, 0.054,  0,  50),
            (0.055, 0.070, 51, 100),
            (0.071, 0.085,101,150),
            (0.086, 0.105,151,200),
            (0.106, 0.200,201,300),
            // if you want 301–500 you can add (0.201,0.604,301,500)
        };
        private static readonly (double Cl, double Ch, int Il, int Ih)[] NO2 = {
            (0.000, 0.053,   0,  50),
            (0.054, 0.100,  51, 100),
            (0.101, 0.360, 101, 150),
            (0.361, 0.649, 151, 200),
            (0.650, 1.249, 201, 300),
            (1.250, 1.649, 301, 400),
            (1.650, 2.049, 401, 500),
        };
        private static readonly (double Cl, double Ch, int Il, int Ih)[] SO2 = {
            (0.000, 0.035,   0,  50),
            (0.036, 0.075,  51, 100),
            (0.076, 0.185, 101, 150),
            (0.186, 0.304, 151, 200),
            (0.305, 0.604, 201, 300),
            (0.605, 0.804, 301, 400),
            (0.805, 1.004, 401, 500),
        };
        private static readonly (double Cl, double Ch, int Il, int Ih)[] CO = {
            (0.0,   4.4,    0,  50),
            (4.5,   9.4,   51, 100),
            (9.5,  12.4,  101, 150),
            (12.5,15.4,  151, 200),
            (15.5,30.4,  201, 300),
            (30.5,40.4,  301, 400),
            (40.5,50.4,  401, 500),
        };

        private static int SubIndex(double C, (double Cl, double Ch, int Il, int Ih)[] bp)
        {
            var seg = bp.FirstOrDefault(s => C >= s.Cl && C <= s.Ch);
            if (seg == default)
                return C < bp[0].Cl ? bp[0].Il : bp.Last().Ih;

            return (int)Math.Round(((seg.Ih - seg.Il) / (seg.Ch - seg.Cl)) * (C - seg.Cl) + seg.Il);
        }

        public static int ComputeAqi(
            double pm25, double pm10,
            double o3, double no2,
            double so2, double co)
        {
            var i1 = SubIndex(pm25, PM25);
            var i2 = SubIndex(pm10, PM10);
            var i3 = SubIndex(o3, O3);
            var i4 = SubIndex(no2, NO2);
            var i5 = SubIndex(so2, SO2);
            var i6 = SubIndex(co, CO);
            return new[] { i1, i2, i3, i4, i5, i6 }.Max();
        }
    }
}
