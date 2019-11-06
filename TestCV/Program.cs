using OpenCVAppLayoutUnderstanding;
using OpenCVAppLayoutUnderstanding.Background;
using OpenCVAppLayoutUnderstanding.Layout;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCV
{
    class Program1
    {
        static void Main(string[] args)
        {
            var v = new Variables
            {
                bg_color_top_n = 3,
                img_padding = 20,
                BGCalcIncludeColors = new List<BGCalcIncludeColor> {
                    new BGCalcIncludeColor {
                        Background = new Scalar(240, 244, 248),
                        IncludeColor = new Scalar(199, 237, 252)
                    }
                },
                segment_ignore_gap_pixels = 8,
                adaptive_gap_pixels = true,
                gap_pixels_minimum = 6,
                gap_pixels_decrement_rate = 2,
                layout_depth = 3
            };
            Mat grayScale = Cv2.ImRead("a12.png", ImreadModes.Grayscale);
            var finalRegions = API.FindLayoutDepthRegions(grayScale, v);
            var layout_depth = 3;
            for (var i = 0; i < layout_depth; ++i)
            {
                var selectedRegions = finalRegions.Where(r => r.Level == i + 1).ToList();
                if (selectedRegions.Count > 0)
                {
                    var title = $"layout hierarchy level - {i + 1}";
                    DrawRegions(grayScale, selectedRegions, title);
                }
            }
            DrawRegions(grayScale, finalRegions);
        }

        static void DrawRegions(Mat grayScale, List<RegionInfo> regions, string title = "regions")
        {
            using (var mat = new Mat())
            {
                Cv2.CvtColor(grayScale, mat, ColorConversionCodes.GRAY2RGB);
                foreach (var b in regions)
                {
                    Cv2.Rectangle(mat, b.Region, new Scalar(255, (b.Level - 1) * 50, (b.Level - 1) * 50), 2);
                }
                Cv2.ImShow(title, mat);
                Cv2.WaitKey(0);
            }
        }
    }
}
