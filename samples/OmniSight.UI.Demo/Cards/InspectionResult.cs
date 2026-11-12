using System;
using System.Globalization;

namespace OmniSight.UI.Demo.Cards
{
    internal sealed class InspectionResult
    {
        private InspectionResult(DateTime time, bool? cam1, bool? cam2, string height, string width,
            string coil, string twist, string defect = null)
        {
            Time = time;
            Cam1 = cam1;
            Cam2 = cam2;
            Height = height;
            Width = width;
            Coil = coil;
            Twist = twist;
            Defect = defect;
        }

        public DateTime Time { get; }
        public bool? Cam1 { get; }
        public bool? Cam2 { get; }
        public bool Passed => Cam1 != false && Cam2 != false;
        public string Height { get; }
        public string Width { get; }
        public string Coil { get; }
        public string Twist { get; }
        public string Defect { get; }

        public bool Matches(DateTime start, DateTime end, int camera, int verdict)
        {
            if (Time.Date < start || Time.Date > end)
                return false;

            bool? passed = Passed;
            if (camera == 1)
                passed = Cam1;
            else if (camera == 2)
                passed = Cam2;

            return passed.HasValue && (verdict == 0 || passed.Value == (verdict == 1));
        }

        public static string VerdictText(bool? passed)
        {
            return passed.HasValue ? (passed.Value ? "OK" : "NG") : "—";
        }

        public static InspectionResult[] Samples()
        {
            return new[]
            {
                Sample("12 14:32:17", false, true, "12.34 / 12.68", "7.42", "0.81 / 0.84", "0.10 / 0.12",
                    "CAM 1  ·  터미널 높이 우측 12.68 mm  ·  상한 12.60 mm"),
                Sample("12 14:32:15", true, true, "12.36 / 12.39", "7.43", "0.80 / 0.83", "0.10 / 0.11"),
                Sample("12 14:32:13", true, true, "12.33 / 12.37", "7.41", "0.81 / 0.82", "0.09 / 0.12"),
                Sample("12 14:32:11", true, true, "12.35 / 12.38", "7.44", "0.79 / 0.82", "0.10 / 0.13"),
                Sample("12 14:32:09", true, true, "12.32 / 12.38", "7.42", "0.81 / 0.82", "0.11 / 0.12"),
                Sample("12 14:32:07", true, true, "12.34 / 12.36", "7.40", "0.80 / 0.82", "0.10 / 0.11"),
                Sample("12 14:32:05", true, null, "12.35 / 12.41", "7.42", "0.82 / 0.84", "0.10 / 0.12"),
                Sample("12 14:32:03", null, true, "—", "—", "0.80 / 0.82", "0.10 / 0.11"),
                Sample("11 16:28:26", true, false, "12.31 / 12.36", "7.41", "0.80 / 0.83", "0.12 / 0.16",
                    "CAM 2  ·  들뜸 우측 0.16 mm  ·  상한 0.15 mm"),
                Sample("11 16:28:21", true, true, "12.32 / 12.37", "7.42", "0.80 / 0.81", "0.11 / 0.12")
            };
        }

        private static InspectionResult Sample(string time, bool? cam1, bool? cam2, string height,
            string width, string coil, string twist, string defect = null)
        {
            DateTime date = DateTime.ParseExact("2026-11-" + time, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture);
            return new InspectionResult(date, cam1, cam2, height, width, coil, twist, defect);
        }
    }
}
