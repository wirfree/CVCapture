using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenCVAppLayoutUnderstanding.Background
{
    public class ResidualRegionDetector : IDisposable
    {
        private Mat _graySrcImage;
        private Mat _binarizedImage;
        private Variables _v;
        private List<Rect> _regions;
        private List<Rect> _blocks;
        private List<RegionInfo> _myRegions;
        public List<RegionInfo> Regions => _myRegions;

        public ResidualRegionDetector(Mat graySrcImage, Variables v, List<Rect> regions)
        {
            _graySrcImage = graySrcImage;
            _v = v;
            _regions = regions;
            _binarizedImage = GetPaddedBinary();
            _blocks = GetBlocks();
            _myRegions = _blocks.Select(b =>
            {
                var tb = TranslateBack(b);
                return new RegionInfo { Region = tb, Color = GetPerBlockBaseColor(tb) };
            }).ToList();
        }

        private Mat GetPaddedBinary()
        {
            using (Mat tmp = Mat.Ones(_graySrcImage.Size(), MatType.CV_8UC1) * 255)
            {
                Mat tmp2 = new Mat();
                var pts = _regions.Select(p =>
                {
                    var points = new Point[4];
                    points[0] = p.Location;
                    points[1] = p.Location + new Point(p.Width, 0);
                    points[2] = p.Location + new Point(p.Width, p.Height);
                    points[3] = p.Location + new Point(0, p.Height);
                    return points;
                });
                Cv2.FillPoly(tmp, pts, new Scalar(0, 0, 0));

                var padding = _v.img_padding;
                Cv2.CopyMakeBorder(tmp, tmp2, padding, padding, padding, padding, BorderTypes.Constant, new Scalar(0, 0, 0));
                return tmp2;
            }
        }

        private List<Rect> GetBlocks()
        {
            List<Rect> result = new List<Rect>();
            Point[][] contours = null;
            HierarchyIndex[] hierarchy = null;
            Cv2.FindContours(_binarizedImage, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            foreach (var contour in contours)
            {
                var rect = Cv2.MinAreaRect(contour).BoundingRect();
                var rectArea = rect.Size.Width * rect.Size.Height;
                var contourArea = Cv2.ContourArea(contour);
                if ((rectArea - contourArea) / contourArea <= 0.1)
                {
                    result.Add(rect);
                }
            }
            return result;
        }

        private int GetPerBlockBaseColor(Rect block)
        {
            using(var part = new Mat(_graySrcImage, block))
            using(var hist = GetHist(part))
            {
                return FindPerBlockBaseColor(hist);
            }
        }

        private Mat GetHist(Mat part)
        {
            var images = new Mat[] { part };
            int[] channels = new int[] { 0 };
            int[] histSize = new int[] { 256 };
            float[][] ranges = new float[][] { new float[] { 0, 256 } };
            Mat hist = new Mat();
            Cv2.CalcHist(images, channels, null, hist, 1, histSize, ranges);
            return hist;
        }

        private int FindPerBlockBaseColor(Mat hist)
        {
            List<GrayScaleCount> list = new List<GrayScaleCount>();
            for (var i = 0; i < hist.Rows; ++i)
            {
                var grayScaleCount = (int)hist.Get<float>(i);
                list.Add(new GrayScaleCount { GrayScaleValue = i, Count = grayScaleCount });
            }
            list.Sort((x, y) => y.Count - x.Count);
            
            return list[0].GrayScaleValue;
        }

        class GrayScaleCount
        {
            public int GrayScaleValue { get; set; }
            public int Count { get; set; }
        }

        private Rect TranslateBack(Rect rectAfterPadding)
        {
            return rectAfterPadding - new Point(_v.img_padding, _v.img_padding);
        }

        public void ShowBinary()
        {
            Cv2.ImShow($"Residual", _binarizedImage);
            Cv2.WaitKey(0);
        }

        public void Dispose()
        {
            _binarizedImage.Dispose();
        }
    }
}
