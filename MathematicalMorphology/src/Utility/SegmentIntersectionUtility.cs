using Esri.ArcGISRuntime.Geometry;
using MathematicalMorphology.src.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathematicalMorphology.src.Utility
{
    public static class SegmentIntersectionUtility
    {
        /// <summary>
        /// Returns true if the segments interesect,
        /// false otherwise.
        /// 
        /// Segments are NOT considered intersecting if the are connected
        /// at their start or end points!!!!!!
        /// </summary>
        /// <param name="segment1">fist segment</param>
        /// <param name="segment2">second segment</param>
        /// <returns>true if the segments are intersecting (this includes overlapping)</returns>
        public static bool SegmentsIntersect(Segment segment1, Segment segment2)
        {
            return GetLineSegmentIntersection(segment1, segment2) != null;
        }       

        /// <summary>
        /// Subtracts the two vectors coords
        /// </summary>
        /// <param name="vector1">first vector</param>
        /// <param name="vector2">second vector</param>
        /// <returns>difference between the two vectors</returns>
        public static Vector2 Sub(MapPoint vector1, MapPoint vector2)
        {
            return new Vector2() { X = vector1.X - vector2.X, Y = vector1.Y - vector2.Y };
        }

        /// <summary>
        /// Adds the two vectors
        /// </summary>
        /// <param name="vector1">first vector</param>
        /// <param name="vector2">second vector</param>
        /// <returns>the point where it is added</returns>
        public static MapPoint Add(MapPoint vector1, MapPoint vector2)
        {
            return new MapPoint(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        /// <summary>
        /// Multiplies the vector by the scalar value
        /// </summary>
        /// <param name="scalarValue">scalar value to mutliply the vector with</param>
        /// <param name="vector">the vector to multiply to</param>
        /// <returns>the point with the scalar multiplication applied</returns>
        public static MapPoint ScalarMult(double scalarValue, Vector2 vector)
        {
            return new MapPoint(scalarValue * vector.X, scalarValue * vector.Y);
        }

        /// <summary>
        /// Returns true if the two segments are Colinear
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns>true if they are colinear false otherwise</returns>
        public static bool IsColinear(Segment start, Segment end)
        {
            return IsParallel(start, end) && IsOverlapping(start, end);
        }

        /// <summary>
        /// Returns true if the two segments are overlapping
        ///  
        /// Note: pictures are just to help, do not need to be horizontal lines, can detect at any slope
        /// 
        /// Overlaps = true
        ///     |------|  start
        /// |------|      end
        ///
        /// Overlaps = true
        ///|-------------| start
        ///    |----|      end
        ///    
        /// Overlaps = false
        ///|---|           start
        ///        |---|   end
        ///    
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns></returns>
        public static bool IsOverlapping(Segment start, Segment end)
        {
            if(IsParallel(start, end))
            {
                if(IsVertical(start) || IsVertical(end))
                {
                    return Math.Max(end.StartPoint.Y,     end.EndPoint.Y) >= Math.Min(start.StartPoint.Y, start.EndPoint.Y) &&
                           Math.Max(start.StartPoint.Y, start.EndPoint.Y) >= Math.Min(end.StartPoint.Y,     end.EndPoint.Y);
                }
                else
                {
                    return Math.Max(end.StartPoint.X,     end.EndPoint.X) >= Math.Min(start.StartPoint.X, start.EndPoint.X) &&
                           Math.Max(start.StartPoint.X, start.EndPoint.X) >= Math.Min(end.StartPoint.X, end.EndPoint.X);
                }              
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Returns true if the segments are parallel
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns></returns>
        public static bool IsParallel(Segment start, Segment end)
        {
            //check horizontal and vertical lines first
            //to avoid dividing by 0 and NaN values
            if(IsHorizontal(start) && IsHorizontal(end))
            {
                return true;
            }
            else if(IsVertical(start) && IsVertical(end))
            {
                return true;
            }
            else
            {
                //calculate the slopes and see if they are about equal
                //slope is calculated rise over run  (y2 -y1) / (x2 - x1)
                var m1 = GetSlope(start);
                var m2 = GetSlope(end);

                return GeometryUtility.IsEpsilonEquals(m1, m2);
            }
        }

        /// <summary>
        /// calculates the slope of the segment
        /// </summary>
        /// <param name="segment">the segment</param>
        /// <returns>the slope of the segment</returns>
        public static double GetSlope(Segment segment)
        {
            return (segment.EndPoint.Y - segment.StartPoint.Y) / (segment.EndPoint.X - segment.StartPoint.X);
        }

        /// <summary>
        /// Strictly crossing intersections, Overlapping would return false.
        /// 
        /// It also does not consider segments whose start and end points are the intersection points as crossing ("L" shape intersections)
        /// 
        /// Crossing = false
        /// .--------  start
        /// |
        /// |
        /// |
        /// end
        /// 
        /// Crossing = true
        ///       end
        ///       |
        /// |---- |------| start
        ///       |
        ///       
        ///  Crossing = true
        ///       end       
        /// |---- |------| start
        ///       |
        ///      
        /// crossing = false
        ///     |------|  start
        /// |------|      end
        ///
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns>true if the segments are crossing, false otherwise</returns>
        public static bool IsCrossing(Segment start, Segment end)
        {
            if(IsColinear(start, end))
            {
                return false;
            }

            //never can cross if parallel
            if(IsParallel(start, end))
            {
                return false;
            }           

            //check vertical and horizontal intersections to avoid NaN and dividing by 0 values

            if(IsVertical(start) && IsHorizontal(end) ||
               IsVertical(end)   && IsHorizontal(start))
            {
                //either 3 x Values are shared or 3 y values are shared
                //3 X's are shared if the T is upright or upside down
                //3 Y's are shared if the T is lying on its side
                var sameXValues = new List<MapPoint>();
                var sameYValues = new List<MapPoint>();
                //2 Y's equal
                if (IsHorizontal(start))
                {
                    //3 Y's equal?
                    if(GeometryUtility.IsEpsilonEquals(end.StartPoint.Y, start.EndPoint.Y) ||
                       GeometryUtility.IsEpsilonEquals(end.EndPoint.Y, start.EndPoint.Y))
                    {
                        return true;
                    }                    
                }
                //2 Y's equal
                if (IsHorizontal(end))
                {
                    //3 Y's equal?
                    if (GeometryUtility.IsEpsilonEquals(start.StartPoint.Y, end.EndPoint.Y) ||
                        GeometryUtility.IsEpsilonEquals(start.EndPoint.Y, end.EndPoint.Y))
                    {
                        return true;
                    }
                }

                //2 X's Equal
                if(IsVertical(start))
                {
                    //3 X's equal?
                    if (GeometryUtility.IsEpsilonEquals(end.StartPoint.X, start.EndPoint.X) ||
                        GeometryUtility.IsEpsilonEquals(end.EndPoint.X, start.EndPoint.X))
                    {
                        return true;
                    }
                }
                
                //2 X's Equal
                if (IsVertical(end))
                {
                    //3 X's equal?
                    if (GeometryUtility.IsEpsilonEquals(start.StartPoint.X, end.EndPoint.X) ||
                        GeometryUtility.IsEpsilonEquals(start.EndPoint.X, end.EndPoint.X))
                    {
                        return true;
                    }
                }

                return false;
            }

            //check if it is elbow intersection which we do not count as crossing
            if(IsLIntersection(start, end))
            {
                return false;
            }


            //note it can't be both vertical lines or both horizontal lines since we
            //check if lines are parallel 
            //http://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
            int o1 = Orientation(start.StartPoint, start.EndPoint, end.StartPoint);
            int o2 = Orientation(start.StartPoint, start.EndPoint, end.EndPoint);
            int o3 = Orientation(end.StartPoint,   end.EndPoint, start.StartPoint);
            int o4 = Orientation(end.StartPoint,   end.EndPoint, start.EndPoint);

            // General case
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the intersection is at the start or end points of the segment
        /// (which creates an "L" shape)
        /// 
        /// L intersection = true
        /// .--------  start
        /// |
        /// |
        /// |
        /// end
        /// 
        /// L Intersection = false
        ///       end       
        /// |---- |------| start
        ///       |
        ///       |
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns>true if the intersection is an L shape, false otherwise</returns>
        private static bool IsLIntersection(Segment start, Segment end)
        {
            return (start.StartPoint.MapPointEpsilonEquals(end.StartPoint) ||
                    start.StartPoint.MapPointEpsilonEquals(end.EndPoint)   ||
                    start.EndPoint  .MapPointEpsilonEquals(end.StartPoint) ||
                    start.EndPoint  .MapPointEpsilonEquals(end.EndPoint));
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are colinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        public static int Orientation(MapPoint p, MapPoint q, MapPoint r)
        {
            // See http://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            var val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (GeometryUtility.IsEpsilonEquals(val, 0.0)) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        /*
         * So if it is a T intersection, in our case we do not want
         * to count if they are intersecting at the start and end points
         * of the segment, since we are okay with that case in the arrangement
         * So T intersections are strictly intersections that are on the segment,
         * excluding the start and end points
         */
        public static bool IsTIntersection(Segment start, Segment end)
        {
            if(IsCrossing(start, end))
            {
                if(IsHorizontal(start))
                {
                    //if start is horizontal then all
                    //the Y values are the same
                    //so if the intersection point is the same
                    //as the Y value of start or end of Start segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEpsilonEquals(GetLineSegmentIntersection(start, end).Y, start.StartPoint.Y);
                }
                else if(IsHorizontal(end))
                {
                    //if end is horizontal then all
                    //the Y values are the same
                    //so if the intersection point is the same
                    //as the Y value of start or end of End segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEpsilonEquals(GetLineSegmentIntersection(start, end).Y, end.StartPoint.Y);
                }
                else if(IsVertical(start))
                {
                    //if start is vertical then all
                    //the X values are the same
                    //so if the intersection point is the same
                    //as the X value of start or end of start segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEpsilonEquals(GetLineSegmentIntersection(start, end).X, start.StartPoint.X);
                }
                else if (IsVertical(end))
                {
                    //if end is vertical then all
                    //the X values are the same
                    //so if the intersection point is the same
                    //as the X value of start or end of end segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEpsilonEquals(GetLineSegmentIntersection(start, end).X, end.StartPoint.X);
                }
                
                //safe from dividing by 0 and getting NaN
                var intersection = GetLineSegmentIntersection(start, end);

                //check if intersection is a point shared by start and end segment
                //if they have this as a common point then it isn't a T but more of
                //an elbow L
                if( (start.EndPoint.MapPointEpsilonEquals(intersection) || 
                     start.StartPoint.MapPointEpsilonEquals(intersection)) &&
                    (end.StartPoint.MapPointEpsilonEquals(intersection) ||
                     end.EndPoint.MapPointEpsilonEquals(intersection)))
                {
                    return false;
                }

                //if intersection is the start or end point of either segment,
                //intersection is a T
                return intersection.MapPointEpsilonEquals(start.StartPoint) ||
                       intersection.MapPointEpsilonEquals(start.EndPoint)   ||
                       intersection.MapPointEpsilonEquals(end.StartPoint)   ||
                       intersection.MapPointEpsilonEquals(end.EndPoint);
            }

            return false;
        }

        /// <summary>
        /// True if the segment is vertical, false otherwise
        /// </summary>
        /// <param name="segment">segment</param>
        /// <returns>True if the segment is vertical, false otherwise</returns>
        public static bool IsVertical(Segment segment)
        {
            return GeometryUtility.IsEpsilonEquals(segment.StartPoint.X, segment.EndPoint.X);
        }

        /// <summary>
        /// True if the segment is horizontal, false otherwise
        /// </summary>
        /// <param name="segment">segment</param>
        /// <returns>True if the segment is horizontal, false otherwise</returns>
        public static bool IsHorizontal(Segment segment)
        {
            return GeometryUtility.IsEpsilonEquals(segment.StartPoint.Y, segment.EndPoint.Y);
        }

        /// <summary>
        /// Gets the intersection point of the two segments.
        /// Does not consider a valid intersection if it is NOT crossing (check IsCrossing Documentation for further description)
        /// If the segments are not crossing, will return null
        /// otherwise will return the intersection point
        /// </summary>
        /// <param name="start">start segment</param>
        /// <param name="end">end segment</param>
        /// <returns>the intersection point if they are crossing, null otherwise</returns>
        public static MapPoint GetLineSegmentIntersection(Segment start, Segment end)
        {
            if(IsCrossing(start, end) == false)
            {
                return null;
            }

            Vector2 vectorA = new Vector2()
            {
                X = start.EndPoint.X - start.StartPoint.X,
                Y = start.EndPoint.Y - start.StartPoint.Y
            };

            Vector2 vectorB = new Vector2()
            {
                X = end.EndPoint.X - end.StartPoint.X,
                Y = end.EndPoint.Y - end.StartPoint.Y
            };

            var p = start.StartPoint;
            var r = vectorA;

            var q = end.StartPoint;
            var s = vectorB;

            var crossProduct = Vector2.Cross(vectorA, vectorB);

            var qmp = Sub(q, p);
            var numerator = Vector2.Cross(qmp, s);
            var t = numerator / crossProduct;
            var intersection = Add(p, ScalarMult(t, r));

            return intersection;         
        }

        /// <summary>
        /// Returns true if any segments intersect with one another (with our definition of intersection, look at IsCrossing documentation
        /// for further clarification), false otherwise
        /// </summary>
        /// <param name="segments">list of segments to check for intersections</param>
        /// <returns>true if any are intersecting, false otherwise</returns>
        public static bool AnySegmentInstersect(List<Segment> segments)
        {
            var originalSize = segments.Count();
            return SolveIntersections(segments).Count() > originalSize;
        }

        /// <summary>
        /// Will break up the list of segments at the first found intersection.
        /// 
        /// Does not return a fully non-intersecting list of segments, must be called recursively 
        /// to achieve no intersections in the entire list.
        /// </summary>
        /// <param name="segments">list of segments which contain at least one intersection</param>
        /// <returns>the list of segments with one less intersection point</returns>
        public static List<Segment> SolveIntersections(List<Segment> segments)
        {
            for (var index = 0; index < segments.Count; index++)
            {
                var firstSegment = segments[index];

                for (var secondIndex = index + 1; secondIndex < segments.Count - 1; secondIndex++)
                {
                    var secondSegment = segments[secondIndex];

                    if (SegmentsIntersect(firstSegment, secondSegment))
                    {
                        segments.RemoveAt(secondIndex);
                        segments.RemoveAt(index);
                        var brokenSegments = new List<Segment>();
                        //3 segments
                        if (IsColinear(firstSegment, secondSegment))
                        {
                            //make sure that the start and end points aren't the only things connecting them
                            //2 segments for that
                            //otherwise 3
                            var points = new List<MapPoint>() { firstSegment.StartPoint, firstSegment.EndPoint, secondSegment.StartPoint, secondSegment.EndPoint };
                            points.Sort((mp1, mp2) => mp1.X.CompareTo(mp2.X));
                            brokenSegments.Add(new LineSegment(points[0], points[1]));
                            brokenSegments.Add(new LineSegment(points[1], points[2]));
                        }
                        //3 segments
                        else if (IsTIntersection(firstSegment, secondSegment))
                        {
                            var intersectionPoint = GetLineSegmentIntersection(firstSegment, secondSegment);
                            //var seg1 = new Esri.ArcGISRuntime.Geometry.LineSegment(Math.Min)
                            var segment1 = new LineSegment(firstSegment.StartPoint, intersectionPoint);
                            var segment2 = new LineSegment(intersectionPoint, firstSegment.EndPoint);
                            var segment3 = new LineSegment(secondSegment.StartPoint, intersectionPoint);
                            var segment4 = new LineSegment(intersectionPoint, secondSegment.EndPoint);

                            if (!segment1.StartPoint.MapPointEpsilonEquals(segment1.EndPoint))
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
                            var intersectionPoint = GetLineSegmentIntersection(firstSegment, secondSegment);
                            var segment1 = new LineSegment(firstSegment.StartPoint, intersectionPoint);
                            var segment2 = new LineSegment(intersectionPoint, firstSegment.EndPoint);
                            var segment3 = new LineSegment(secondSegment.StartPoint, intersectionPoint);
                            var segment4 = new LineSegment(intersectionPoint, secondSegment.EndPoint);

                            brokenSegments.AddRange(new List<Segment>() { segment1, segment2, segment3, segment4 });
                        }

                        segments.AddRange(brokenSegments);
                        return segments;
                    }
                }
            }
            return segments;
        }

    }
}
