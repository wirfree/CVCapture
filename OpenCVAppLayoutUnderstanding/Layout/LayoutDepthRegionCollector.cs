using OpenCVAppLayoutUnderstanding.Background;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenCVAppLayoutUnderstanding.Layout
{
    public class LayoutDepthRegionCollector
    {
        private Mat _graySrcImage;
        private List<RegionInfo> _topRegions;
        private Variables _v;
        private List<RegionInfo> _regions;
        public List<RegionInfo> Regions => _regions;
        public LayoutDepthRegionCollector(Mat graySrcImage, List<RegionInfo> topRegions, Variables v)
        {
            _graySrcImage = graySrcImage;
            _topRegions = topRegions;
            _v = v;
            _regions = GetRegions(topRegions, v.layout_depth);
        }

        public List<RegionInfo> GetRegions(List<RegionInfo> regions, int depth)
        {
            if (depth == 1)
            {
                return regions;
            }
            var result = new List<RegionInfo>(regions);
            foreach(var region in regions)
            {
                Variables v = _v;
                if (_v.adaptive_gap_pixels)
                {
                    var gapPixReduction = (_v.layout_depth - depth + 1) * _v.gap_pixels_decrement_rate;
                    var gapPix = _v.segment_ignore_gap_pixels - gapPixReduction;
                    if (gapPix < _v.gap_pixels_minimum)
                    {
                        gapPix = _v.gap_pixels_minimum;
                    }
                    v = _v.OverrideSegmentIgnoreGapPixels(gapPix);
                }
                using (LayoutDetector layoutDetector = new LayoutDetector(_graySrcImage, region, v))
                {
                    var children = layoutDetector.ChildRegions;
                    //layoutDetector.ShowBinary();
                    if (children.Count == 1)
                    {
                        result.Add(children[0]);
                    }
                    else if (children.Count > 1)
                    {
                        result.AddRange(GetRegions(children, depth - 1));
                    }
                }
            }
            return result;
        }
    }
}
