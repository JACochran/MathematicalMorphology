using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MathematicalMorphology.src.Utility
{
    public static class MinkowskiSumUtility
    {

        public static Polygon CalculateMinkowskiSumPolygons(this Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var bottomLeftPoint = polygon1.GetBottomLeftPoint();

            PartCollection sumParts = new PartCollection(polygon1.SpatialReference);

            foreach (var part in polygon2.Parts)
            {
                foreach (var point in part.GetPoints())
                { 
                    var distance = bottomLeftPoint.GetSalarDistance(point);                    

                    var translatedPolygon = polygon1.TranslatePolygon(distance);


                    sumParts.AddParts(translatedPolygon.Parts);

                    translatedPolygon.AddPolygonToMap(mapview, Colors.Purple);
                }
            }

            var sumPolygon = GeometryEngine.ConvexHull(new Polygon(sumParts));

            return sumPolygon as Polygon;
        }

        public static Polygon TranslatePolygon(this Polygon polygon1, ScalarDistance distance)
        {
            var polygonBuilder = new PolygonBuilder(polygon1.SpatialReference);
            foreach (var part in polygon1.Parts)
            {
                foreach (var point in part.GetPoints())
                {
                    polygonBuilder.AddPoint(distance.XDistance + point.X, distance.YDistance + point.Y);
                }
            }

            return polygonBuilder.ToGeometry();
        }

        public static Polygon CalculateMinkowskiSumNonConvexPolygonsSecond(this Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var bottomLeft = polygon1.GetBottomLeftPoint();
            var bottomLeft2 = polygon2.GetBottomLeftPoint();
            MapPoint centerPoint = new MapPoint(0.0, 0.0, polygon1.SpatialReference);
            var distance = bottomLeft.GetSalarDistance(centerPoint);
            var distance2 = bottomLeft2.GetSalarDistance(centerPoint);
            var a = TranslatePolygon(polygon1, distance);
            var b = TranslatePolygon(polygon2, distance2);

            //clockwise sort
            var sortedClockwisePolygon = a.Parts.First().ToList().Select(segment => new MinkowskiSegment(a, segment, true)).ToList();
            sortedClockwisePolygon.AddRange(b.Parts.First().Select(segment => new MinkowskiSegment(b, segment, false)).ToList());
            sortedClockwisePolygon.Sort(MinkowskiSegment.ClockwiseComparison);

            MapPoint previousPoint = null;

            var polygon = new PolygonBuilder(polygon1.SpatialReference);

            var mapPointMap = new HashSet<MapPoint>(new MapPointEqualityComparison());

            for (var index = 0; index < sortedClockwisePolygon.Count; index++)
            {
                var segment = sortedClockwisePolygon[index];

                var previousSegment = sortedClockwisePolygon[index - 1 < 0 ? sortedClockwisePolygon.Count - 1 : index - 1];
                var nextSegment = sortedClockwisePolygon[index + 1 >= sortedClockwisePolygon.Count ? 0 : index + 1];


                MapPoint point = previousSegment.GetSharedPoint(nextSegment);
                //which point to use?

                segment.Segment.AddSegmentToMap(mapview, Colors.Chocolate, $"Original {index}");
                var movedSegment = new List<MapPoint>(){
                                                             new MapPoint(point.X + segment.Segment.StartPoint.X, point.Y + segment.Segment.StartPoint.Y),
                                                             new MapPoint(point.X + segment.Segment.EndPoint.X, point.Y + segment.Segment.EndPoint.Y)
                                                       };

                if (previousPoint != null)
                {
                    if (previousPoint.IsEqual(movedSegment.First()))
                    {
                        movedSegment.RemoveAt(0);
                    }
                    else if (previousPoint.IsEqual(movedSegment.Last()))
                    {
                        movedSegment.RemoveAt(1);
                    }
                }

                previousPoint = movedSegment.Last();
                movedSegment.ForEach(mapPoint => mapPointMap.Add(mapPoint));

                //AddSegmentToMap(new Esri.ArcGISRuntime.Geometry.LineSegment(movedSegment.First(), movedSegment.Last()), mapview, Colors.Coral, $"Moved {index}");
                polygon.AddPoints(movedSegment);
            }


            return GeometryEngine.Simplify(polygon.ToGeometry()) as Polygon;
        }


        public static Polygon CalculateMinkowskiSumNonConvexPolygons(this Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var bottomLeft = polygon1.GetBottomLeftPoint();
            var bottomLeft2 = polygon2.GetBottomLeftPoint();
            MapPoint centerPoint = new MapPoint(0.0, 0.0, polygon1.SpatialReference);
            var distance = bottomLeft.GetSalarDistance(centerPoint);
            var distance2 = bottomLeft2.GetSalarDistance(centerPoint);
            var a = TranslatePolygon(polygon1, distance);
            var b = TranslatePolygon(polygon2, distance2);


            var polygon = new PolygonBuilder(polygon1.SpatialReference);

            var mapPointMap = new HashSet<MapPoint>(new MapPointEqualityComparison());
            var modifiedBSegments = GetAugmentationForPolygon(a,b);
            var modifiedASegments = GetAugmentationForPolygon(b, a);

            var mapPointBSet = new HashSet<MapPoint>(new MapPointEqualityComparison());
            
            foreach(var segment in modifiedBSegments)
            {
                mapPointBSet.Add(segment.StartPoint);
                mapPointBSet.Add(segment.EndPoint);
            }

            var mapPointASet = new HashSet<MapPoint>(new MapPointEqualityComparison());

            foreach (var segment in modifiedASegments)
            {
                mapPointASet.Add(segment.StartPoint);
                mapPointASet.Add(segment.EndPoint);
            }

            polygon.AddPoints(mapPointBSet);
            polygon.AddPoints(mapPointASet);

            return polygon.ToGeometry();// GeometryEngine.Simplify(polygon.ToGeometry()) as Polygon;
        }

        public static List<MapPoint> GetModifiedListOfMapPoints(Dictionary<Segment, List<MapPoint>> augmentationList)
        {
            var modifiedMapPoints = new List<MapPoint>();
            foreach (var segment in augmentationList)
            {
                var modifiedSegmentStartPoint = segment.Key.StartPoint;
                var modifiedSegmentEndPoint = segment.Key.EndPoint;
                foreach (var mappoint in segment.Value)
                {
                    modifiedSegmentStartPoint = new MapPoint(modifiedSegmentStartPoint.X + mappoint.X, modifiedSegmentStartPoint.Y + mappoint.Y);
                    modifiedSegmentEndPoint = new MapPoint(modifiedSegmentEndPoint.X + mappoint.Y, modifiedSegmentEndPoint.Y + mappoint.Y);
                }
                modifiedMapPoints.AddRange(new List<MapPoint> { modifiedSegmentStartPoint, modifiedSegmentEndPoint });
            }

            return modifiedMapPoints;
        }

        public static List<Segment> GetAugmentationForPolygon(Polygon polygonA, Polygon polygonB)
        {
            var modifiedSegments = new List<Segment>();
            //points are sorted in clockwise order
            for (var index = 0; index < polygonA.Parts.First().GetPoints().Count() - 1; index++)
            {
                var currentPoint = polygonA.Parts.First().GetPoint(index);
                //last point is the same as the first point therefore get the second to last point (since that is the end)
                var previousPoint = index - 1 < 0 ? polygonA.Parts.First().GetPoint(polygonA.Parts.First().GetPoints().Count() - 2) : polygonA.Parts.First().GetPoint(index - 1);
                var nextPoint = index + 1 > polygonA.Parts.First().GetPoints().Count() ? polygonA.Parts.First().GetPoints().First() : polygonA.Parts.First().GetPoint(index + 1);

                var previousSegment = new Esri.ArcGISRuntime.Geometry.LineSegment(previousPoint, currentPoint);
                var nextSegment = new Esri.ArcGISRuntime.Geometry.LineSegment(currentPoint, nextPoint);

                var segmentsToAlter = GetSegmentsWithinRange(previousSegment.CalculateAngle(), nextSegment.CalculateAngle(), polygonB);
                foreach(var segment in segmentsToAlter)
                {
                    modifiedSegments.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(new MapPoint(segment.StartPoint.X + currentPoint.X, segment.StartPoint.Y + currentPoint.Y),
                                                                                     new MapPoint(segment.EndPoint.X   + currentPoint.X, segment.EndPoint.Y   + currentPoint.Y)));
                }
               
            }

            return modifiedSegments;
        }

        public static List<Segment> GetSegmentsWithinRange(double lowerBound, double upperBound, Polygon polygon)
        {
            var segments = new List<Segment>();
            foreach(var segment in polygon.Parts.First())
            {
                var angle = segment.CalculateAngle();
                if(angle >= lowerBound || angle <= upperBound)
                {
                    segments.Add(segment);
                }
            }
            return segments;
        }

        public static MapPoint GetSharedPoint(this MinkowskiSegment segment1, MinkowskiSegment segment2)
        {
            if (segment1.IsPolygonA == segment2.IsPolygonA)
            {
                if (segment1.Segment.StartPoint.IsEqual(segment2.Segment.StartPoint))
                {
                    return segment1.Segment.StartPoint;
                }
                else if (segment1.Segment.StartPoint.IsEqual(segment2.Segment.EndPoint))
                {
                    return segment1.Segment.StartPoint;
                }
                else if (segment1.Segment.EndPoint.IsEqual(segment2.Segment.StartPoint))
                {
                    return segment1.Segment.EndPoint;
                }
                else if (segment1.Segment.EndPoint.IsEqual(segment2.Segment.EndPoint))
                {
                    return segment1.Segment.EndPoint;
                }
                else
                {
                    throw new ArgumentException("Points dont line up");
                }

            }
            else
            {
                throw new ArgumentException("Invalid segments");
            }

        }

        //public static Dictionary<Segment,MapPoint> GetSharedPoints(this MinkowskiSegment segment1, MinkowskiSegment segment2)
        //{
            
        //}
    }


    
}
