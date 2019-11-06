using OpenCVAppLayoutUnderstanding.Background;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenCVAppLayoutUnderstanding.Layout
{
    public class LayoutDetector: IDisposable
    {
        private Mat _graySrcImage;
        private RegionInfo _region;
        private Variables _v;
        private Mat _binarizedImage;
        private List<Segment> _vertical;
        private List<Segment> _horizontal;
        public eOrientation Orientation { get; private set; }
        private List<RegionInfo> _childRegions;
        public List<RegionInfo> ChildRegions => _childRegions;

        public LayoutDetector(Mat graySrcImage, RegionInfo region, Variables v)
        {
            _graySrcImage = GetPartial(graySrcImage, region);
            _region = region;
            _v = v;
            _binarizedImage = GetBinary();
            _vertical = AnalyseVerticalSumUp();
            _horizontal = AnalyseHorizontalSumUp();
            if (_vertical.Count >= _horizontal.Count)
            {
                Orientation = eOrientation.Vertical;
            }
            else
            {
                Orientation = eOrientation.Horizontal;
            }
            if (Orientation == eOrientation.Vertical)
            {
                _childRegions = _vertical.Select(p => Segment2Region(p)).ToList();
            }
            else
            {
                _childRegions = _horizontal.Select(p => Segment2Region(p)).ToList();
            }
        }

        private Mat GetPartial(Mat graySrcImage, RegionInfo region)
        {
            return new Mat(graySrcImage, region.Region);
        }

        private Mat GetBinary()
        {
            var color = _region.Color;
            var match = _v.BGCalcIncludeColors.FirstOrDefault(p => p.BackgroundGrayScale == color);
            if (match == null)
            {
                var tmp = new Mat();
                Cv2.InRange(_graySrcImage, new Scalar(color), new Scalar(color + 1), tmp);
                return tmp;
            }
            var includeColor = match.IncludeColorGrayScale;
            using (var tmp1 = new Mat())
            using (var tmp2 = new Mat())
            {
                Cv2.InRange(_graySrcImage, new Scalar(color), new Scalar(color + 1), tmp1);
                Cv2.InRange(_graySrcImage, new Scalar(includeColor), new Scalar(includeColor + 1), tmp2);
                return tmp1 + tmp2;
            }
        }

        private List<Segment> AnalyseVerticalSumUp()
        {
            int rows = _binarizedImage.Rows, columns = _binarizedImage.Cols;
            bool[] raw = new bool[rows];
            for (var i = 0; i < rows; ++i)
            {
                bool hasVal = false;
                for(var j = 0; j < columns; ++j)
                {
                    int[] val = new int[1];
                    _binarizedImage.GetArray(i, j, val);
                    // note that with inrange, background is masked white
                    if (val[0] == 0)
                    {
                        hasVal = true;
                        break;
                    }
                }
                raw[i] = hasVal;
            }
            //return WalkSegment(raw);
            var rawSegments = WalkSegment(raw);
            // get rid of small segments which most likely to be lines
            return rawSegments.Where(p => p.End - p.Start >= 3).ToList();
        }

        private List<Segment> AnalyseHorizontalSumUp()
        {
            int rows = _binarizedImage.Rows, columns = _binarizedImage.Cols;
            bool[] raw = new bool[columns];
            for (var i = 0; i < columns; ++i)
            {
                bool hasVal = false;
                for (var j = 0; j < rows; ++j)
                {
                    byte[] val = new byte[1];
                    _binarizedImage.GetArray(j, i, val);
                    // note that with inrange, background is masked white
                    if (val[0] == 0)
                    {
                        hasVal = true;
                        break;
                    }
                }
                raw[i] = hasVal;
            }
            var rawSegments = WalkSegment(raw);
            // get rid of small segments which most likely to be lines
            return rawSegments.Where(p => p.End - p.Start >= 3).ToList();
        }

        private List<Segment> WalkSegment(bool[] raw)
        {
            var segment_ignore_gap_pixels = _v.segment_ignore_gap_pixels;
            var result = new List<Segment>();
            int start = -1;
            int gap_pixel_count = 0;
            for(var i=0; i < raw.Length; ++i)
            {
                var val = raw[i];
                if (start == -1)
                {
                    if (val)
                    {
                        start = i;
                    }
                }
                else
                {
                    if (val)
                    {
                        gap_pixel_count = 0;
                    }
                    else
                    {
                        if (gap_pixel_count >= segment_ignore_gap_pixels)
                        {
                            int end = i - gap_pixel_count;
                            result.Add(new Segment { Start = start, End = end });
                            start = -1;
                            gap_pixel_count = 0;
                        }
                        else
                        {
                            gap_pixel_count++;
                        }
                    }
                }
            }
            if (start != -1)
            {
                result.Add(new Segment { Start = start, End = raw.Length - 1 });
            }
            return result;
        }

        private RegionInfo Segment2Region(Segment segment)
        {
            if (Orientation == eOrientation.Vertical)
            {
                var width = _binarizedImage.Width;
                var height = segment.End - segment.Start + 1;
                return new RegionInfo
                {
                    Color = _region.Color,
                    Region = new Rect(new Point(0, segment.Start) + _region.Region.Location, new Size(width, height)),
                    Level = _region.Level + 1
                };
            }
            else
            {
                var height = _binarizedImage.Height;
                var width = segment.End - segment.Start + 1;
                return new RegionInfo
                {
                    Color = _region.Color,
                    Region = new Rect(new Point(segment.Start, 0) + _region.Region.Location, new Size(width, height)),
                    Level = _region.Level + 1
                };
            }
        }

        public void ShowBinary()
        {
            using (var mat = new Mat())
            {
                Cv2.CvtColor(_binarizedImage, mat, ColorConversionCodes.GRAY2RGB);
                foreach (var r in ChildRegions)
                {
                    Cv2.Rectangle(mat, r.Region - _region.Region.Location, new Scalar(255, 0, 0), 2);
                }
                Cv2.ImShow($"color-{_region.Color}", mat);
                Cv2.WaitKey(0);
            }
        }

        public void Dispose()
        {
            _graySrcImage.Dispose();
            _binarizedImage.Dispose();
        }

        class Segment
        {
            public int Start { get; set; }
            public int End { get; set; }
        }
        public enum eOrientation
        {
            Vertical = 1,
            Horizontal = 2
        }
    }
}
