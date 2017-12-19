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
        /// This will calculate the Minkowski Sum by:
        /// 
        /// For each vertex in Polygon A and Polygon B calculate the angle range
        /// For each vertex in Polygon A find the segments in B that are within the angle range and add that vertex to the segment’s start and end point
        /// For each vertex in Polygon B find the segments in Polygon A that are within the angle range and add that vertex to the segment’s start and end point
        /// Take the resulting segments from the modification and calculate the Arrangement:
        ///    The arrangement is essentially breaking up all the segments at their intersections
        ///  Take the arrangement and find the outermost segment and traverse counter-clockwise to the connected segments
        ///  tracing along the boundary of the arrangement.  
        ///  the result is the Minkowksi Sum  
        ///
        /// 
        /// </summary>
        /// <param name="polygon1">first polygon</param>
        /// <param name="polygon2">second polygon</param>
        /// <returns>a polygon that is the minkowski sum of the two polygons passed in</returns>
        public static Polygon CalculateMinkowskiSumNonConvexPolygons(this Polygon polygon1, Polygon polygon2)
        {
            var polygon = new PolygonBuilder(polygon1.SpatialReference);
            //For each vertex in Polygon A find the segments in B that are within the angle range and add that vertex to the segment’s start and end point
            var modifiedBSegments = GetAugmentationForPolygon(polygon1, polygon2);
            //For each vertex in Polygon B find the segments in Polygon A that are within the angle range and add that vertex to the segment’s start and end point
            var modifiedASegments = GetAugmentationForPolygon(polygon2, polygon1);
            //combine the list
            modifiedBSegments.AddRange(modifiedASegments);
            //Take the resulting segments from the modification and calculate the Arrangement:
            var segments = BreakUpPolygon(modifiedBSegments, modifiedBSegments.Count());

            //trace the boundary of the arrangement
            return SimplifyPolygon(segments);
        }

        /// <summary>
        /// This will traverse the boundary of the arrangment.
        /// 
        /// First it will find a segment along the outside of the arrangement
        /// Then it will traverse to through its connected segments choosing 
        /// the right most segment to be part of the Minkowski sum.
        /// 
        /// It will continue until the polygon is fully closed.
        /// </summary>
        /// <param name="segments">the list of segments that are part of the arrangement</param>
        /// <returns>the simplified polygon, only the boundary of the segments</returns>
        public static Polygon SimplifyPolygon(List<Segment> segments)
        {
            var polygonSimple = new PolygonBuilder(segments.First().SpatialReference);
            var outermostSegment = GetOuterMostSegment(segments);
            
            var closingPoint = outermostSegment.StartPoint;
            bool isClosed = false;
            polygonSimple.AddPoints(new List<MapPoint>() { outermostSegment.StartPoint, outermostSegment.EndPoint });

            var currentSegment = outermostSegment;
            var connectionPoint = currentSegment.EndPoint;
            while(isClosed == false)
            {
                var segmentsConnected = FindConnectedSegments(currentSegment, segments);
                
                var rightmostSegment = FindRightMostSegment(currentSegment, segmentsConnected);

                polygonSimple.AddPoint(rightmostSegment.EndPoint);
                //polygonSimple.AddPoints(new List<MapPoint>() { rightmostSegment.StartPoint, rightmostSegment.EndPoint });

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
 
        /// <summary>
        /// returns true if any of the segments are copies of one another
        ///  returns false if all segments are unique
        /// 
        /// </summary>
        /// <param name="segments">list of segments to check</param>
        /// <returns>true if there are segments that are copies of one another, false otherwise</returns>
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

        /// <summary>
        /// Removes all the duplicated segments from the given list
        /// Will throw if there are no duplicates in the list!!
        /// </summary>
        /// <param name="segments">the list of segments that contain duplicates</param>
        /// <returns>the modified list of segments without duplicates</returns>
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

            throw new Exception("NO Duplicates exist in the list, when RemoveDuplicates were called.");
        }

        /// <summary>
        /// Breaks up the list of segments until there are no more intersections (other than at their
        /// connection points)
        /// </summary>
        /// <param name="segments">list of segments to break apart by their intersections</param>
        /// <returns>list of segments that are broken by their intersections</returns>
        public static List<Segment> BreakUpPolygon(List<Segment> segments, int largestSize)
        {
            //remove any duplicate segments
            //otherwise there will be infinte intersections
            while (HasDuplicates(segments))
            {
                segments = RemoveDuplicates(segments);
            }
            
            
            //if there are no more intersections return the segments passed in
            if (SegmentIntersectionUtility.AnySegmentInstersect(segments) == false || segments.Count() <= largestSize)
            {
                return segments;
            }

            largestSize = segments.Count();
            Console.Out.WriteLine("Segments Count: " + largestSize);


            //otherwise break up the polygon by the first intersection found
            return BreakUpPolygon(SegmentIntersectionUtility.SolveIntersections(segments), largestSize);
        }

        /// <summary>
        /// Tests if a segment was created with the same start and end point
        /// </summary>
        /// <param name="seg1">segment to test</param>
        /// <returns>true if its length is > 0, false otherwise</returns>
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
        /// 
        /// After converted to vectors:
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
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
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

        /// <summary>
        /// Finds the list of segments that is connected to the connected segment parameter passed in.
        /// 
        /// It will search through the list of potential connected segments from the list passed in.
        /// </summary>
        /// <param name="connectedSegment">Segment that we are looking to connect to</param>
        /// <param name="segments">the list of potential connections</param>
        /// <returns>a list of segments whose startpoint is connected to the connectedSegment's end point</returns>
        private static List<Segment> FindConnectedSegments(Segment connectedSegment, List<Segment> segments)
        {
            var connectedList = new List<Segment>();
            foreach(var segment in segments)
            {
                //reverse the segment if connected in wrong way (wrong way is not in counter-clockwise order)
                if(segment.EndPoint.MapPointEpsilonEquals(connectedSegment.EndPoint))
                {
                    connectedList.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(segment.EndPoint, segment.StartPoint));
                }
                else if (segment.StartPoint.MapPointEpsilonEquals(connectedSegment.EndPoint))
                {
                    connectedList.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(segment.StartPoint, segment.EndPoint));
                }
            }

            return connectedList;
        }

        /// <summary>
        /// Gets the MapPoint whose X is the lowest value
        /// </summary>
        /// <param name="points">the list of points to search through</param>
        /// <returns>the MapPoint with the lowest value</returns>
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

        /// <summary>
        /// Gets the outermost segment (furthest east) if there
        /// are two segments whose X's starts are the same, will pick the 
        /// one with the least Y value end point
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
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

            MapPoint outerMostEndQuad1 = null;
            MapPoint outerMostEndQuad4 = null;
            foreach(var point in pointsConnectedTo)
            {
                var angle = new Esri.ArcGISRuntime.Geometry.LineSegment(minXPoint, point).CalculateAngle();


                // if the angle is in quadrant 1 pick the largest angle
                if (angle >= 0.0 && angle <= 90.0)
                {
                    if (outerMostEndQuad1 == null)
                    {
                        outerMostEndQuad1 = point;
                    }
                    else if(new Esri.ArcGISRuntime.Geometry.LineSegment(minXPoint, outerMostEndQuad1).CalculateAngle() < angle)
                    {
                        outerMostEndQuad1 = point;
                    }
                }
                //if the angle is in quadrant 4 pick the smallest angle
                else if (angle >= 270.0 && angle <= 360.0)
                {
                    if(outerMostEndQuad4 == null)
                    {
                        outerMostEndQuad4 = point;
                    }
                    else if( new Esri.ArcGISRuntime.Geometry.LineSegment(minXPoint, outerMostEndQuad4).CalculateAngle() > angle)
                    {
                        outerMostEndQuad4 = point;
                    }
                }
                else
                {
                    //SHouldnt ever happen
                }
            }

            return outerMostEndQuad4 != null ? new Esri.ArcGISRuntime.Geometry.LineSegment(minXPoint, outerMostEndQuad4)
                                             : new Esri.ArcGISRuntime.Geometry.LineSegment(outerMostEndQuad1, minXPoint);
        }

        /**
         * Polygon A vertices -> added to Polygon B segments
         */
        public static List<Segment> GetAugmentationForPolygon(Polygon polygonA, Polygon polygonB)
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

        /// <summary>
        /// Returns the segments that are within the angle range 
        /// </summary>
        /// <param name="lowerBound">lower angle bound in degrees (0, 360]</param>
        /// <param name="upperBound">upper angle bound in degrees (0, 360]</param>
        /// <param name="polygon">The polygon to find segments that are within that angle range</param>
        /// <returns> Returns the segments that are within the angle range </returns>
        public static List<Segment> GetSegmentsWithinRange(double lowerBound, double upperBound, Polygon polygon)
        {
            var segments = new List<Segment>();
            foreach(var segment in polygon.Parts.First())
            {
                var angle = segment.CalculateAngle();
                //cases where the angle is behind
                //this is for the reflex points in a polygon
                if(lowerBound > upperBound)
                {
                    if(angle >= lowerBound || angle <= upperBound)
                    {
                        segments.Add(segment);
                    }
                }
                //otherwise treat as normal angle range
                if(angle >= lowerBound && angle <= upperBound)
                {
                    segments.Add(segment);
                }
            }
            return segments;
        }     
    }    
}
