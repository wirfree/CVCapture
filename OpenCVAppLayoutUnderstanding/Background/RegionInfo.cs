using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenCVAppLayoutUnderstanding.Background
{
    public class RegionInfo
    {
        public Rect Region { get; set; }
        public int Color { get; set; }
        public int Level { get; set; } = 1;
    }
}
