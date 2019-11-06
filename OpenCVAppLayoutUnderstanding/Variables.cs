using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCVAppLayoutUnderstanding
{
    public class Variables
    {
        public int bg_color_top_n { get; set; }
        public int img_padding { get; set; }
        public List<BGCalcIncludeColor> BGCalcIncludeColors { get; set; }
        public int segment_ignore_gap_pixels { get; set; }
        public bool adaptive_gap_pixels { get; set; }
        public int gap_pixels_decrement_rate { get; set; } = 2;
        public int gap_pixels_minimum { get; set; }
        public int layout_depth { get; set; } = 2;

        private Variables Clone()
        {
            var v = new Variables
            {
                bg_color_top_n = this.bg_color_top_n,
                img_padding = this.img_padding,
                BGCalcIncludeColors = this.BGCalcIncludeColors,
                segment_ignore_gap_pixels = this.segment_ignore_gap_pixels,
                layout_depth = this.layout_depth,
                adaptive_gap_pixels = this.adaptive_gap_pixels,
                gap_pixels_decrement_rate = this.gap_pixels_decrement_rate,
                gap_pixels_minimum = this.gap_pixels_minimum
            };
            return v;
        }

        public Variables OverrideSegmentIgnoreGapPixels(int val)
        {
            var v = Clone();
            v.segment_ignore_gap_pixels = val;
            return v;
        }
    }

    public class BGCalcIncludeColor
    {
        public Scalar Background { get; set; }
        public Scalar IncludeColor { get; set; }

        public int BackgroundGrayScale => ToGrayScale(Background);
        public int IncludeColorGrayScale => ToGrayScale(IncludeColor);

        private static int ToGrayScale(Scalar color) => (int)(0.299 * color.Val0 + 0.587 * color.Val1 + 0.114 * color.Val2);
    }
}
