using OpenCVAppLayoutUnderstanding.Background;
using OpenCVAppLayoutUnderstanding.Layout;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace OpenCVAppLayoutUnderstanding
{
    public static class API
    {
        public static List<RegionInfo> FindLayoutDepthRegions(Mat grayScale, Variables v)
        {
            HistogramBasedBgColorFinder bgFinder = new HistogramBasedBgColorFinder(grayScale, v);
            var colors = bgFinder.FindBackgroundGrayScales();
            var regions = colors.SelectMany(c =>
            {
                using (var rd = new RegionDetector(grayScale, c, v))
                {
                    return rd.Regions;
                }
            }).ToList();
            LayoutDepthRegionCollector collector = new LayoutDepthRegionCollector(grayScale, regions, v);
            var finalRegions = collector.Regions;
            return finalRegions;
        }
    }
}
