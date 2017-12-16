using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathematicalMorphology.src.Utility
{
    public static class GeometryUtility
    {
        public static String SegmentToSting(this Segment value)
        {
            return $"Start Point: {value.StartPoint.MapPointToString()}, End Point: {value.EndPoint.MapPointToString()}";
        }

        public static String MapPointToString(this MapPoint value)
        {
            return $" ({value.X}, {value.Y})";
        }

        public static bool IsEpsilonEquals(double value, double value2, double epsilon = Epsilon)
        {
            return Math.Abs(value - value2) <= epsilon;
        }

        public const double Epsilon = 1e-7;

        public static bool SegmentEpsilonEquals(this Segment segment, Segment segment2)
        {
            return segment.StartPoint.MapPointEpsilonEquals(segment2.StartPoint) &&
                   segment.EndPoint  .MapPointEpsilonEquals(segment2.EndPoint);
        }

        public static bool MapPointEpsilonEquals(this MapPoint mapPoint, MapPoint mapPoint2)
        {
            return IsEpsilonEquals(mapPoint.X, mapPoint2.X) && IsEpsilonEquals(mapPoint.Y, mapPoint2.Y);
        }

        public const double RotationAngle = 0.5;

        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }


        public static bool HasEqualSegmentAngles(Polygon polygonA, Polygon polygonB)
        {
            foreach (var segmentA in polygonA.Parts.First())
            {
                foreach (var segmentB in polygonB.Parts.First())
                {
                    if (IsEpsilonEquals(segmentA.CalculateAngle(), segmentB.CalculateAngle()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static double DegreeToRadian(double radian)
        {
            return radian * (Math.PI / 180.0);
        }

        public static Polygon RotatePolygon(this Polygon polygon, double angle = RotationAngle)
        {
            angle = DegreeToRadian(angle);
            var rotatedPolygon = new PolygonBuilder(polygon.SpatialReference);
            foreach(var point in polygon.Parts.First().GetPoints())
            {
                rotatedPolygon.AddPoint(new MapPoint(point.X*Math.Cos(angle) - point.Y*Math.Sin(angle), 
                                                     point.X*Math.Sin(angle) + point.Y*Math.Cos(angle)));
            }
            return rotatedPolygon.ToGeometry();
        }

        public static double CalculateAngle(this Segment segment)
        { 
            var opposite = segment.EndPoint.Y - segment.StartPoint.Y;
            var adjacent = segment.EndPoint.X - segment.StartPoint.X;
            var radians = Math.Atan2(opposite, adjacent);
            var degrees = RadianToDegree(radians);
            return ConvertRange(degrees);
        }

        public static Polyline ToPolyine(this Segment segment)
        {
            return new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint });
        }

        public static double ConvertRange(double angle)
        {
            return angle < 0 ? angle + 360.0 : angle;
        }

        public static MapPoint GetTopRightPoint(this Polygon polygon)
        {
            var topRightPoint = polygon.Parts.First().First().StartPoint;
            foreach (var part in polygon.Parts)
            {
                foreach (var point in part.GetPoints())
                {
                    if (point.Y > topRightPoint.Y)
                    {
                        topRightPoint = point;
                    }
                    else if (point.Y == topRightPoint.Y)
                    {
                        if (point.X > topRightPoint.X)
                        {
                            topRightPoint = point;
                        }
                    }

                }
            }

            return topRightPoint;
        }

        public static MapPoint GetBottomLeftPoint(this Polygon polygon)
        {
            var bottomLeftPoint = polygon.Parts.First().StartPoint;

            foreach (var part in polygon.Parts)
            {
                foreach (var point in part.GetPoints())
                {
                    if (point.X <= bottomLeftPoint.X)
                    {
                        if(GeometryUtility.IsEpsilonEquals(point.X, bottomLeftPoint.X))
                        {
                            bottomLeftPoint = point.Y < bottomLeftPoint.Y ? point : bottomLeftPoint;
                        }
                        else
                        {
                            bottomLeftPoint = point;
                        }
                    }
                }
            }

            return bottomLeftPoint;
        }

        public static Polygon OrderVerticiesCounterClockwise(this Polygon polygon)
        {
            //https://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            if (polygon.IsCounterClockwise())
            {
                return polygon;
            }
            //reverse segments
            var reversed = new PolygonBuilder(polygon.SpatialReference);
            var points = polygon.Parts.First().GetPoints().ToList();
            for (var index = points.Count() - 1; index >= 0; index--)
            {
                var point = points[index];
                reversed.AddPoint(point);
            }

            return reversed.ToGeometry();
        }


        public static bool IsCounterClockwise(this Polygon polygon)
        {
            var sum = 0.0;
            var points = polygon.Parts.First().GetPoints().ToList();
            var previousPoint = points.First();
            for (var index = 1; index < points.Count(); index++)
            {
                var nextPoint = points[index];
                sum += (nextPoint.X - previousPoint.X) * (nextPoint.Y + previousPoint.Y);
                previousPoint = nextPoint;
            }
            return sum <= 0.0;
        }


        public static ScalarDistance GetSalarDistance(this MapPoint fromPoint, MapPoint toPoint)
        {
            return new ScalarDistance((fromPoint.X < toPoint.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(fromPoint.X, 0.0), new MapPoint(toPoint.X, 0.0)),
                                      (fromPoint.Y < toPoint.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, fromPoint.Y), new MapPoint(0.0, toPoint.Y)));
        }
    }
}
