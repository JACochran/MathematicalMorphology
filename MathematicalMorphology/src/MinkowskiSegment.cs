using Esri.ArcGISRuntime.Geometry;
using MathematicalMorphology.src.Utility;
using System;

namespace MathematicalMorphology.src
{
    public class MinkowskiSegment
    {
        public static Comparison<MinkowskiSegment> ClockwiseComparison => ((segment1, segment2) => segment1.Segment.CalculateAngle().CompareTo(segment2.Segment.CalculateAngle()));

        public MinkowskiSegment(Polygon polygon, Segment segment, bool isPolygonA)
        {
            this.Polygon = polygon;
            this.Segment = segment;
            this.IsPolygonA = IsPolygonA;
        }

        public Polygon Polygon { get; private set; }
        public Segment Segment { get; private set; }
        public bool IsPolygonA { get; private set; }
    }
}
