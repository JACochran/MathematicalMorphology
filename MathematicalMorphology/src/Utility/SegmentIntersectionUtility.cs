﻿using Esri.ArcGISRuntime.Geometry;
using MathematicalMorphology.src.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathematicalMorphology.src.Utility
{
    public static class SegmentIntersectionUtility
    {

        public static bool SegmentsIntersect(Segment segment1, Segment segment2)
        {
            return GetLineSegmentIntersection(segment1, segment2) != null;
        }

        public enum SegmentIntersectionType
        {
            Meets,
            Crosses,
            Parallel,
            Colinear,
            NoIntersection
        }

        public static Vector2 Sub(MapPoint vector1, MapPoint vector2)
        {
            return new Vector2() { X = vector1.X - vector2.X, Y = vector1.Y - vector2.Y };
        }

        public static MapPoint Add(MapPoint vector1, MapPoint vector2)
        {
            return new MapPoint(vector1.X + vector2.X, vector1.Y + vector2.Y);
        }

        public static MapPoint ScalarMult(double s, Vector2 v)
        {
            return new MapPoint(s * v.X, s * v.Y);
        }

        public static bool IsCollinear(Segment start, Segment end)
        {
            return IsParallel(start, end) && IsOverlapping(start, end);
        }
        
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

                return GeometryUtility.IsEqual(m1, m2);
            }
        }

        public static double GetSlope(Segment segment)
        {
            return (segment.EndPoint.Y - segment.StartPoint.Y) / (segment.EndPoint.X - segment.StartPoint.X);
        }

        //strictly crossing, not overlaps
        public static bool IsCrossing(Segment start, Segment end)
        {
            if(IsCollinear(start, end))
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
                    if(GeometryUtility.IsEqual(end.StartPoint.Y, start.EndPoint.Y) ||
                       GeometryUtility.IsEqual(end.EndPoint.Y, start.EndPoint.Y))
                    {
                        return true;
                    }                    
                }
                //2 Y's equal
                if (IsHorizontal(end))
                {
                    //3 Y's equal?
                    if (GeometryUtility.IsEqual(start.StartPoint.Y, end.EndPoint.Y) ||
                        GeometryUtility.IsEqual(start.EndPoint.Y, end.EndPoint.Y))
                    {
                        return true;
                    }
                }

                //2 X's Equal
                if(IsVertical(start))
                {
                    //3 X's equal?
                    if (GeometryUtility.IsEqual(end.StartPoint.X, start.EndPoint.X) ||
                        GeometryUtility.IsEqual(end.EndPoint.X, start.EndPoint.X))
                    {
                        return true;
                    }
                }
                
                //2 X's Equal
                if (IsVertical(end))
                {
                    //3 X's equal?
                    if (GeometryUtility.IsEqual(start.StartPoint.X, end.EndPoint.X) ||
                        GeometryUtility.IsEqual(start.EndPoint.X, end.EndPoint.X))
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

            if (GeometryUtility.IsEqual(val, 0.0)) return 0;  // colinear

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
                    return GeometryUtility.IsEqual(GetLineSegmentIntersection(start, end).Y, start.StartPoint.Y);
                }
                else if(IsHorizontal(end))
                {
                    //if end is horizontal then all
                    //the Y values are the same
                    //so if the intersection point is the same
                    //as the Y value of start or end of End segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEqual(GetLineSegmentIntersection(start, end).Y, end.StartPoint.Y);
                }
                else if(IsVertical(start))
                {
                    //if start is vertical then all
                    //the X values are the same
                    //so if the intersection point is the same
                    //as the X value of start or end of start segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEqual(GetLineSegmentIntersection(start, end).X, start.StartPoint.X);
                }
                else if (IsVertical(end))
                {
                    //if end is vertical then all
                    //the X values are the same
                    //so if the intersection point is the same
                    //as the X value of start or end of end segment
                    //then it is a T intersection, otherwise false
                    return GeometryUtility.IsEqual(GetLineSegmentIntersection(start, end).X, end.StartPoint.X);
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

        public static bool IsVertical(Segment segment)
        {
            return GeometryUtility.IsEqual(segment.StartPoint.X, segment.EndPoint.X);
        }

        public static bool IsHorizontal(Segment segment)
        {
            return GeometryUtility.IsEqual(segment.StartPoint.Y, segment.EndPoint.Y);
        }

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

            ////vertical line
            //if (GeometryUtility.IsEqual(start.StartPoint.X, start.EndPoint.X))
            //{
            //    //if end segment also vertical line, could be parallel or colinear
            //    if (GeometryUtility.IsEqual(end.StartPoint.X, end.EndPoint.X))  //if x the same  check overlaps
            //    {
            //        return IsColinearOrParallel(start, end);

            //    }
            //    //if x different check if they intersect, the second line must be perpendicular
            //    //which means the other line is horizontal then they MAY intersect
            //    else if (GeometryUtility.IsEqual(end.StartPoint.Y, end.EndPoint.Y))
            //    {


            //    }
            //    else
            //    {
            //        return SegmentIntersectionType.NoIntersection;
            //    }

            //}
            //else if (GeometryUtility.IsEqual(end.StartPoint.X, end.EndPoint.X))//vertical line
            //{
            //    if (GeometryUtility.IsEqual(end.StartPoint.X, end.EndPoint.X))//if x the same  check overlaps
            //    {
            //        //  if the y's are within eachother's range overlap otherwise the do not
            //        if (Math.Min(start.StartPoint.Y, start.EndPoint.Y) < Math.Max(end.StartPoint.Y, end.EndPoint.Y))
            //        {
            //            return SegmentIntersectionType.Colinear;
            //        }
            //        else
            //        {
            //            return SegmentIntersectionType.Parallel;
            //        }
            //    }
            //    else //if x different then they do not intersect
            //    {
            //        return SegmentIntersectionType.NoIntersection;
            //    }
            //}

            //var A1 = (start.StartPoint.Y - start.EndPoint.Y) / (start.StartPoint.X - start.EndPoint.X);
            //var A2 = (end.StartPoint.Y - end.EndPoint.Y) / (end.StartPoint.X - end.EndPoint.X);

            //var b1 = start.StartPoint.Y - A1 * start.StartPoint.X;
            //var b2 = end.StartPoint.Y - A2 * end.StartPoint.X;

            //if (GeometryUtility.IsEqual(A1, A2))
            //{
            //    //Parallel lines or overlaps
            //}

            //var Xa = (b2 - b1) / (A1 - A2);// Once again, pay attention to not dividing by zero
            //var Ya = A1 * Xa + b1;


            //if ((Xa < Math.Max(Math.Min(start.StartPoint.X, start.EndPoint.X), Math.Min(end.StartPoint.X, end.EndPoint.X))) ||
            //    (Xa > Math.Min(Math.Max(start.StartPoint.X, start.EndPoint.X), Math.Max(start.StartPoint.X, start.EndPoint.X))))
            //    return SegmentIntersectionType.NoIntersection; // intersection is out of bounds
            //else
            //    return SegmentIntersectionType.Crosses; // intersection is within range
        }

        //public static bool IsColinear(Segment start, Segment end)
        //{
        //    //check if lines are parallel 
        //    //http://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        //    int o1 = Orientation(start.StartPoint, start.EndPoint, end.StartPoint);
        //    int o2 = Orientation(start.StartPoint, start.EndPoint, end.EndPoint);
        //    int o3 = Orientation(end.StartPoint, end.EndPoint, start.StartPoint);
        //    int o4 = Orientation(end.StartPoint, end.EndPoint, start.EndPoint);

        //    // General case
        //    if (o1 != o2 && o3 != o4)
        //        return false;

        //    // Special Cases
        //    // p1, q1 and p2 are colinear and p2 lies on segment p1q1
        //    if (o1 == 0 && OnSegment(start.StartPoint, end.StartPoint, start.EndPoint))
        //    {
        //        return true;
        //    }

        //    // p1, q1 and p2 are colinear and q2 lies on segment p1q1
        //    if (o2 == 0 && OnSegment(start.StartPoint, end.EndPoint, start.EndPoint))
        //    {
        //        return true;
        //    }

        //    // p2, q2 and p1 are colinear and p1 lies on segment p2q2
        //    if (o3 == 0 && OnSegment(end.StartPoint, start.StartPoint, end.EndPoint))
        //    {
        //        return true;
        //    }

        //    // p2, q2 and q1 are colinear and q1 lies on segment p2q2
        //    if (o4 == 0 && OnSegment(end.StartPoint, start.EndPoint, end.EndPoint))
        //    {
        //        return true;
        //    }

        //    return false; // Doesn't fall in any of the above cases
        //}

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
         public static bool OnSegment(MapPoint p, MapPoint q, MapPoint r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        private static List<Segment> FindIntersectingSegments(Segment rightmostSegment, Polygon polygon)
        {
            var segments = new List<Segment>();
            var rightmostSegmentPolyline = rightmostSegment.ToPolyine();
            foreach (var segment in polygon.Parts.First())
            {
                //if (segment.StartPoint.IsEqual(rightmostSegment.StartPoint) ||
                //   segment.StartPoint.IsEqual(rightmostSegment.EndPoint) ||
                //   segment.EndPoint.IsEqual(rightmostSegment.StartPoint) ||
                //   segment.EndPoint.IsEqual(rightmostSegment.EndPoint))
                //{
                //    continue;
                //}

                if (SegmentsIntersect(rightmostSegment, segment))
                {
                    var intersectionPoint = GetLineSegmentIntersection(segment, rightmostSegment);
                    if (intersectionPoint == null)
                    {
                        continue;
                    }
                    segments.Add(new LineSegment(intersectionPoint, segment.EndPoint));
                }
            }

            return segments;
        }

        public static bool AnySegmentInstersect(List<Segment> segments)
        {
            for (var index = 0; index < segments.Count; index++)
            {
                var firstSegment = segments[index];

                for (var secondIndex = index + 1; secondIndex < segments.Count - 1; secondIndex++)
                {
                    var secondSegment = segments[secondIndex];

                    if (SegmentsIntersect(firstSegment, secondSegment))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}