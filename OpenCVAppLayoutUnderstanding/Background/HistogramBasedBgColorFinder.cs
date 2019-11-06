using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCVAppLayoutUnderstanding.Background
{
    public class HistogramBasedBgColorFinder
    {
        private Mat _graySrcImage;
        private Mat _hist;
        private Variables _v;
        public HistogramBasedBgColorFinder(Mat graySrcImage, Variables v)
        {
            _graySrcImage = graySrcImage;
            _v = v;
            _hist = GetHist();
        }

        private Mat GetHist()
        {
            var images = new Mat[] { _graySrcImage };
            int[] channels = new int[] { 0 };
            int[] histSize = new int[] { 256 };
            float[][] ranges = new float[][] { new float[] { 0, 256 } };
            Mat hist = new Mat();
            Cv2.CalcHist(images, channels, null, hist, 1, histSize, ranges);
            return hist;
        }

        public int[] FindBackgroundGrayScales()
        {
            var bg_color_top_n = _v.bg_color_top_n;
            List<GrayScaleCount> list = new List<GrayScaleCount>();
            for (var i = 0; i < _hist.Rows; ++i)
            {
                var grayScaleCount = (int)_hist.Get<float>(i);
                // Console.WriteLine($"position: {i}; count: {grayScaleCount}");
                list.Add(new GrayScaleCount { GrayScaleValue = i, Count = grayScaleCount });
            }
            list.Sort((x, y) => y.Count - x.Count);
            var resultLength = Math.Min(list.Count, bg_color_top_n);
            int[] result = new int[resultLength];
            for(var i=0; i< resultLength; ++i)
            {
                result[i] = list[i].GrayScaleValue;
            }
            return result;
        }

        class GrayScaleCount
        {
            public int GrayScaleValue { get; set; }
            public int Count { get; set; }
        }
    }
}
