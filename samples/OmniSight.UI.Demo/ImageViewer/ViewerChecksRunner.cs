using System;
using System.IO;

namespace OmniSight.UI.Demo.ViewerChecks
{
    /// <summary>ImageViewer 검증 진입점. 하나라도 실패하면 예외를 던져 데모가 종료 코드 1을 반환하게 한다.</summary>
    internal static class ViewerChecksRunner
    {
        public static void RunAll()
        {
            string artifacts = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "artifacts");
            Directory.CreateDirectory(artifacts);

            ViewChecks.Transform();
            ViewChecks.PixelAccuracy();
            ViewChecks.Formats();
            ViewChecks.Input();
            FileChecks.Overlays(artifacts);
            FileChecks.Files(artifacts);
            StreamChecks.LastFrame();
            StreamChecks.QueuePolicy();
            StreamChecks.ThreadStress(1000);
            StreamChecks.Memory(20000);
            Finish();
        }

        /// <summary>--viewer bench | soak [프레임 수] | stress [반복 수]</summary>
        public static void RunSpecial(string[] args)
        {
            string command = args.Length > 1 ? args[1].ToLowerInvariant() : "bench";
            switch (command)
            {
                case "bench":
                    StreamChecks.Benchmark();
                    return;
                case "soak":
                    StreamChecks.Memory(args.Length > 2 ? int.Parse(args[2]) : 1000000);
                    break;
                case "stress":
                    StreamChecks.ThreadStress(args.Length > 2 ? int.Parse(args[2]) : 1000);
                    break;
                default:
                    throw new ArgumentException("--viewer bench | soak [프레임 수] | stress [반복 수]");
            }
            Finish();
        }

        private static void Finish()
        {
            Harness.Summary();
            if (Harness.Failed != 0)
            {
                throw new InvalidOperationException("ImageViewer 검증 실패 " + Harness.Failed + "건");
            }
        }
    }
}
