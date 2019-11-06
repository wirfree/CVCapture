using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenCVAppLayoutUnderstanding.Background
{
    public class RegionDetector: IDisposable
    {
        private Mat _graySrcImage;
        private int _color;
        private Mat _binarizedImage;
        private Variables _v;
        private Size _imgSize;
        /// <summary>
        /// blocks from padded image
        /// </summary>
        private List<Rect> _blocks;
        public List<RegionInfo> Regions => _blocks.Select(b => new RegionInfo { Region = TranslateBack(b), Color = _color }).ToList();

        public RegionDetector(Mat graySrcImage, int color, Variables v)
        {
            _graySrcImage = graySrcImage;
            _imgSize = _graySrcImage.Size();
            _color = color;
            _v = v;
            _binarizedImage = GetPaddedBinary();
            _blocks = GetBlocks();
        }

        private Mat GetInRangeBinary()
        {
            BGCalcIncludeColor match = _v.BGCalcIncludeColors.FirstOrDefault(p => p.BackgroundGrayScale == _color);
            if (match == null)
            {
                Mat tmp = new Mat();
                Cv2.InRange(_graySrcImage, new Scalar(_color), new Scalar(_color + 1), tmp);
                return tmp;
            }
            var includeColor = match.IncludeColorGrayScale;
            using (var tmp1 = new Mat())
            using (var tmp2 = new Mat())
            {
                Cv2.InRange(_graySrcImage, new Scalar(_color), new Scalar(_color + 1), tmp1);
                Cv2.InRange(_graySrcImage, new Scalar(includeColor), new Scalar(includeColor + 1), tmp2);
                return tmp1 + tmp2;
            }
        }

        private Mat GetPaddedBinary()
        {
            using (Mat tmp = GetInRangeBinary())
            using (Mat tmp2 = new Mat())
            {
                Mat tmp3 = new Mat();
                // median blue to get ride of the noise
                Cv2.MedianBlur(tmp, tmp2, 21);
                var padding = _v.img_padding;
                Cv2.CopyMakeBorder(tmp2, tmp3, padding, padding, padding, padding, BorderTypes.Constant, new Scalar(0, 0, 0));
                return tmp3;
            }
        }

        private List<Rect> GetBlocks()
        {
            List<Rect> result = new List<Rect>();
            Point[][] contours = null;
            HierarchyIndex[] hierarchy = null;
            Cv2.FindContours(_binarizedImage, out contours, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            foreach(var contour in contours)
            {
                var rect = Cv2.MinAreaRect(contour).BoundingRect();
                var rectArea = rect.Size.Width * rect.Size.Height;
                var contourArea = Cv2.ContourArea(contour);
                if ((rectArea - contourArea) / contourArea <= 0.1 &&
                    ((double)rect.Width / _imgSize.Width > 0.5 || (double)rect.Height / _imgSize.Height > 0.5))
                {
                    result.Add(rect);
                }
            }
            return result;
        }

        private Rect TranslateBack(Rect rectAfterPadding)
        {
            return rectAfterPadding - new Point(_v.img_padding, _v.img_padding);
        } 

        public void ShowBinary()
        {
            using (var mat = new Mat())
            {
                Cv2.CvtColor(_binarizedImage, mat, ColorConversionCodes.GRAY2RGB);
                foreach(var b in _blocks)
                {
                    Cv2.Rectangle(mat, b, new Scalar(255, 0, 0), 2);
                }
                Cv2.ImShow($"color-{_color}", mat);
                Cv2.WaitKey(0);
            }
        }

        public void Dispose()
        {
            _binarizedImage.Dispose();
        }
    }
}
