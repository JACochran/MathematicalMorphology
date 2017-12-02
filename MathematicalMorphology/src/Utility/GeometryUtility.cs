using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathematicalMorphology.src.Utility
{
    public static class GeometryUtility
    {

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }


        public static double CalculateAngle(this Segment segment)
        {
            //Translate start point to origin
            MapPoint point = new MapPoint(0.0, 0.0, segment.SpatialReference);
            var distance = new KeyValuePair<double, double>((segment.StartPoint.X < point.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(segment.StartPoint.X, 0.0), new MapPoint(point.X, 0.0)), //x distance
                                                            (segment.StartPoint.Y < point.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, segment.StartPoint.Y), new MapPoint(0.0, point.Y))); //y distance

            var startPoint = new MapPoint(segment.StartPoint.X + distance.Key, segment.StartPoint.Y + distance.Value);
            var endPoint = new MapPoint(segment.EndPoint.X + distance.Key, segment.EndPoint.Y + distance.Value);
            var opposite = startPoint.Y - endPoint.Y;
            var adjacent = startPoint.X - endPoint.X;
            var radians = Math.Atan2(opposite, adjacent);
            var degrees = RadianToDegree(radians);
            return ConvertRange(degrees);
        }

        private static double ConvertRange(double angle)
        {
            return angle - 360.0 * Math.Floor(angle / 360.0);
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
                    if (point.X < bottomLeftPoint.X &&
                       point.Y < bottomLeftPoint.Y)
                    {
                        bottomLeftPoint = point;
                    }
                }
            }

            return bottomLeftPoint;
        }


        public static ScalarDistance GetSalarDistance(this MapPoint fromPoint, MapPoint toPoint)
        {
            return new ScalarDistance((fromPoint.X < toPoint.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(fromPoint.X, 0.0), new MapPoint(toPoint.X, 0.0)),
                                      (fromPoint.Y < toPoint.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, fromPoint.Y), new MapPoint(0.0, toPoint.Y)));
        }
    }
}
