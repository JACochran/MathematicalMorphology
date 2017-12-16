using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using MathematicalMorphology.src.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MathematicalMorphology.src.Utility
{
    public static class MinkowskiSumUtility
    {

        /// <summary>
        /// Calculates the Minkowski Sum for two convex polygons.
        /// 
        /// If the polygons are non-convex this will produce an incorrect solution
        /// 
        /// </summary>
        /// <param name="polygon1">first polygon</param>
        /// <param name="polygon2">second polygon</param>
        /// <param name="mapview">view to add the translated polygons (for visual effects only)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Performs a translation of a polygon given a distance
        /// </summary>
        /// <param name="polygon1">the polygon to translate</param>
        /// <param name="distance">the distance to move the polygon by (x and y value)</param>
        /// <returns>a new polygon in the translated location</returns>
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

        /// <summary>
        /// This approach is the Convolution:
        /// 
        /// This will calculate the Minkowski Sum by
        /// For each vertex in Polygon A and Polygon B calculate the angle range
        // For each vertex in Polygon A find the segments in B that are within the angle range and add that vertex to the segment’s start and end point
        // For each vertex in Polygon B find the segments in Polygon A that are within the angle range and add that vertex to the segment’s start and end point
        // The resulting polygon then needs to be simplified(more on how later!)
        /// </summary>
        /// <param name="polygon1"></param>
        /// <param name="polygon2"></param>
        /// <param name="mapview"></param>
        /// <returns></returns>
        public static Polygon CalculateMinkowskiSumNonConvexPolygons(this Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var polygon = new PolygonBuilder(polygon1.SpatialReference);

            var modifiedBSegments = GetAugmentationForPolygon(polygon1, polygon2, mapview);
            var modifiedASegments = GetAugmentationForPolygon(polygon2, polygon1, mapview);

            modifiedBSegments.AddRange(modifiedASegments);

            var segments = BreakUpPolygon(modifiedBSegments);

            return SimplifyPolygon(segments, mapview);
        }

        public static Polygon SimplifyPolygon(List<Segment> segments, MapView mapview)
        {
            var polygonSimple = new PolygonBuilder(segments.First().SpatialReference);
            var outermostSegment = GetOuterMostSegment(segments);
            
            outermostSegment.AddSegmentToMap(mapview, Colors.DimGray, "Outermost Segment");
            var closingPoint = outermostSegment.StartPoint;
            bool isClosed = false;
            polygonSimple.AddPoints(new List<MapPoint>() { outermostSegment.StartPoint, outermostSegment.EndPoint });

            var currentSegment = outermostSegment;
            var connectionPoint = currentSegment.EndPoint;
            while(isClosed == false)
            {
                var segmentsConnected = FindConnectedSegments(currentSegment, segments);
                
                var rightmostSegment = FindRightMostSegment(currentSegment, segmentsConnected); 

                polygonSimple.AddPoints(new List<MapPoint>() { rightmostSegment.StartPoint, rightmostSegment.EndPoint });

                currentSegment = rightmostSegment;
                if(currentSegment.EndPoint.MapPointEpsilonEquals(outermostSegment.StartPoint))// || closedList.Contains(currentSegment))
                {
                    isClosed = true;
                }
            }
          
            var validSegments = new List<Segment>();

            foreach(var segment in polygonSimple.Parts.First())
            {
                if(SegmentIsValid(segment))
                {
                    validSegments.Add(segment);
                }
            }

            return new PolygonBuilder(validSegments).ToGeometry();
        }
 
        public static bool HasDuplicates(List<Segment> segments)
        {
            for(var index = 0; index < segments.Count; index++)
            {
                var currentSeg = segments[index];
                for(var index2 = index +1; index2 < segments.Count -1; index2++)
                {
                    var secondSeg = segments[index2];
                    if(secondSeg.SegmentEpsilonEquals(currentSeg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<Segment> RemoveDuplicates(List<Segment> segments)
        {
            for (var index = 0; index < segments.Count; index++)
            {
                var currentSeg = segments[index];
                for (var index2 = index + 1; index2 < segments.Count - 1; index2++)
                {
                    var secondSeg = segments[index2];
                    if (secondSeg.SegmentEpsilonEquals(currentSeg))
                    {
                        segments.RemoveAt(index);
                        return segments;
                    }
                }
            }

            throw new Exception("NO DUPSS");
        }

        public static List<Segment> BreakUpPolygon(List<Segment> segments)
        {
            while (HasDuplicates(segments))
            {
                segments = RemoveDuplicates(segments);
            }

            Console.WriteLine("Break up poly: " + segments.Count());
            if (SegmentIntersectionUtility.AnySegmentInstersect(segments) == false)
            {
                return segments;
            }

            return BreakUpPolygon(SolveIntersections(segments));
        }

        public static List<Segment> SolveIntersections(List<Segment> segments)
        {          
            for (var index = 0; index < segments.Count; index++)
            {
                var firstSegment = segments[index];

                for (var secondIndex = index + 1; secondIndex < segments.Count -1; secondIndex++)
                {
                    var secondSegment = segments[secondIndex];

                    if (SegmentIntersectionUtility.SegmentsIntersect(firstSegment, secondSegment))
                    {
                        segments.RemoveAt(secondIndex);
                        segments.RemoveAt(index);
                        var brokenSegments = new List<Segment>();
                        //3 segments
                        if(SegmentIntersectionUtility.IsCollinear(firstSegment, secondSegment))
                        {
                            //make sure that the start and end points aren't the only things connecting them
                            //2 segments for that
                            //otherwise 3
                            var points = new List<MapPoint>() { firstSegment.StartPoint, firstSegment.EndPoint, secondSegment.StartPoint, secondSegment.EndPoint };
                            points.Sort((mp1, mp2) => mp1.X.CompareTo(mp2.X));
                            brokenSegments.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(points[0], points[1]));
                            brokenSegments.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(points[1], points[2]));
                        }
                        //3 segments
                        else if(SegmentIntersectionUtility.IsTIntersection(firstSegment, secondSegment))
                        {
                            var intersectionPoint = SegmentIntersectionUtility.GetLineSegmentIntersection(firstSegment, secondSegment);
                            //var seg1 = new Esri.ArcGISRuntime.Geometry.LineSegment(Math.Min)
                            var segment1 = new Esri.ArcGISRuntime.Geometry.LineSegment(firstSegment.StartPoint, intersectionPoint);
                            var segment2 = new Esri.ArcGISRuntime.Geometry.LineSegment(intersectionPoint, firstSegment.EndPoint);
                            var segment3 = new Esri.ArcGISRuntime.Geometry.LineSegment(secondSegment.StartPoint, intersectionPoint);
                            var segment4 = new Esri.ArcGISRuntime.Geometry.LineSegment(intersectionPoint, secondSegment.EndPoint);

                            if(!segment1.StartPoint.MapPointEpsilonEquals(segment1.EndPoint))
                            {
                                brokenSegments.Add(segment1);
                            }
                            if (!segment2.StartPoint.MapPointEpsilonEquals(segment2.EndPoint))
                            {
                                brokenSegments.Add(segment2);
                            }
                            if (!segment3.StartPoint.MapPointEpsilonEquals(segment3.EndPoint))
                            {
                                brokenSegments.Add(segment3);
                            }
                            if (!segment4.StartPoint.MapPointEpsilonEquals(segment4.EndPoint))
                            {
                                brokenSegments.Add(segment4);
                            }

                        }
                        else
                        {
                            var intersectionPoint = SegmentIntersectionUtility.GetLineSegmentIntersection(firstSegment, secondSegment);
                            var segment1 = new Esri.ArcGISRuntime.Geometry.LineSegment(firstSegment.StartPoint, intersectionPoint);
                            var segment2 = new Esri.ArcGISRuntime.Geometry.LineSegment(intersectionPoint, firstSegment.EndPoint);
                            var segment3 = new Esri.ArcGISRuntime.Geometry.LineSegment(secondSegment.StartPoint, intersectionPoint);
                            var segment4 = new Esri.ArcGISRuntime.Geometry.LineSegment(intersectionPoint, secondSegment.EndPoint);

                            brokenSegments.AddRange(new List<Segment>() { segment1, segment2, segment3, segment4 });
                        }
                        
                        segments.AddRange(brokenSegments);                        
                        return segments;
                    }
                }
            }

            Console.WriteLine("NO INTERSECTIONS SHOULD STOP!");
            return segments;
            //throw new ArgumentException("No segments intersect");
        }

        private static bool SegmentIsValid(Segment seg1)
        {
            return seg1 != null && seg1.StartPoint.MapPointEpsilonEquals(seg1.EndPoint) == false;
        }

        public static Segment FindRightMostSegment(Segment segment, List<Segment> connectedSegments)
        {
            var rightmostSegment = connectedSegments.First();
            var rightmostAngle = CalculateAngle(segment, rightmostSegment);
            foreach(var connectedSegment in connectedSegments)
            {
                var angle = CalculateAngle(segment, connectedSegment);
                if (angle < rightmostAngle)
                {
                    rightmostSegment = connectedSegment;
                    rightmostAngle = angle;
                }
            }
            return rightmostSegment;
        }

        /// <summary>
        /// http://www.euclideanspace.com/maths/algebra/vectors/angleBetween/index.htm
        /// Calculates the angle between two connected 2D Vectors.
        ///     start seg
        ///    .------>
        /// end|  inside angle
        /// seg|
        ///    V
        /// 
        /// 
        /// 
        /// outside
        /// angle   end seg
        ///         .------>
        ///    start| 
        ///    seg  |
        ///         V
        ///         
        /// The start segments start point MUST be connected to
        /// end segment's End point
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static double CalculateAngle(Segment start, Segment end)
        {
            Vector2 AB = new Vector2()
            {
                X = end.StartPoint.X - start.StartPoint.X,
                Y = end.StartPoint.Y - start.StartPoint.Y
            };

            Vector2 BC = new Vector2()
            {
                X = end.EndPoint.X - end.StartPoint.X,
                Y = end.EndPoint.Y - end.StartPoint.Y
            };

            return Vector2.GetAngle(AB, BC) * 180.0/Math.PI;
        }

        private static List<Segment> FindConnectedSegments(Segment outermostSegment, List<Segment> segments)
        {
            var connectedList = segments.Where(segment => segment.StartPoint.MapPointEpsilonEquals(outermostSegment.EndPoint)).ToList();
            foreach(var segment in segments)
            {
                //reverse the segment if connected in wrong way
                if(segment.EndPoint.MapPointEpsilonEquals(outermostSegment.EndPoint))
                {
                    connectedList.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(segment.EndPoint, segment.StartPoint));
                }
            }

            return connectedList;
        }

        public static MapPoint GetMinXPoint(List<MapPoint> points)
        {
            var minXPoint = points.First();
            foreach (var point in points)
            {
                if (point.X < minXPoint.X)
                {
                    minXPoint = point;
                }
            }
            return minXPoint;
        }


        public static Segment GetOuterMostSegment(List<Segment> segments)
        {
            var points = new List<MapPoint>();
            foreach(var segment in segments)
            {
                points.Add(segment.StartPoint);
                points.Add(segment.EndPoint);
            }

            var minXPoint = GetMinXPoint(points);

            var pointsConnectedTo = segments.Where(segment => segment.StartPoint.MapPointEpsilonEquals(minXPoint) ||
                                                              segment.EndPoint  .MapPointEpsilonEquals(minXPoint))
                                            .Select(segment => segment.StartPoint.MapPointEpsilonEquals(minXPoint) ? segment.EndPoint : segment.StartPoint);

            var outerMostEnd = pointsConnectedTo.First();
            foreach(var point in pointsConnectedTo)
            {
                if(point.X < outerMostEnd.X &&
                   point.Y < outerMostEnd.Y)
                {
                    outerMostEnd = point;
                }
                else if(GeometryUtility.IsEpsilonEquals(point.X, outerMostEnd.X) && 
                        point.Y < outerMostEnd.Y)
                {
                    outerMostEnd = point;
                }
            }

            return new Esri.ArcGISRuntime.Geometry.LineSegment(minXPoint, outerMostEnd);
        }

        /**
         * Polygon A vertices -> added to Polygon B segments
         */
        public static List<Segment> GetAugmentationForPolygon(Polygon polygonA, Polygon polygonB, MapView mapView)
        {
            var modifiedSegments = new List<Segment>();
            //points are sorted in counter-clockwise order
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
                    var movedSegment = new Esri.ArcGISRuntime.Geometry.LineSegment(new MapPoint(segment.StartPoint.X + currentPoint.X, segment.StartPoint.Y + currentPoint.Y),
                                                                                   new MapPoint(segment.EndPoint.X   + currentPoint.X, segment.EndPoint.Y + currentPoint.Y));
                    modifiedSegments.Add(movedSegment);
                    
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
                if(lowerBound > upperBound)
                {
                    if(angle >= lowerBound || angle <= upperBound)
                    {
                        segments.Add(segment);
                    }
                }
                if(angle >= lowerBound && angle <= upperBound)
                {
                    segments.Add(segment);
                }
            }
            return segments;
        }     
    }    
}
