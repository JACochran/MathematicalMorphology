using Esri.ArcGISRuntime.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MathematicalMorphology.src.Utility.Tests
{
    [TestClass()]
    public class MinkowskiSumUtilityTests
    {
        [TestMethod()]
        public void FindRightMostSegmentTest1()
        {
            /*
            * Right Most Segment From  (x1: 0, y1: 0) (x2 : 0.411421301481155, y2: -2.78199165763448)
            *ANGLE -277.449481664791 (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
            *ANGLE 71.60582542184 (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.86035761982136, y2: -3.21300444966235)
            *RightMost ANGLE -277.449481664791 (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             */
            var connectionPoint = new MapPoint(0.411421301481155, -2.78199165763448);
            var expectedRightMostSegment = new LineSegment(connectionPoint,
                                                   new MapPoint(2.86035761982136, -3.21300444966235));
            var leftMostSegment = new LineSegment(connectionPoint,
                                                  new MapPoint(2.74280867654103, -2.74280867654103));

            var startingSegment = new LineSegment(new MapPoint(0, 0), connectionPoint);


            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startingSegment, new List<Segment>() { leftMostSegment, expectedRightMostSegment });

            Assert.IsTrue(expectedRightMostSegment.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest2()
        {
            /*
             * Right Most Segment From  (x1: 2.86035761982136, y1: -3.21300444966235) (x2 : 5.19174499488124, y2: -3.17382146856891)
             * ANGLE 75.1857173605588 (x1: 5.19174499488124, y1: -3.17382146856891) (x2 : 5.89703865456322, y2: -0.313463848747547)
             * ANGLE 125.141520878138 (x1: 5.19174499488124, y1: -3.17382146856891) (x2 : 4.02145227899537, y2: -1.56920751644972)
             */
            var connectionPoint = new MapPoint(5.19174499488124, -3.17382146856891);
            var expectedRightMostSegment = new LineSegment(connectionPoint,
                                                           new MapPoint(5.89703865456322, -0.313463848747547));
            var leftMostSegment = new LineSegment(connectionPoint,
                                                  new MapPoint(4.02145227899537, -1.56920751644972));

            var startingSegment = new LineSegment(new MapPoint(2.86035761982136, -3.21300444966235), connectionPoint);


            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startingSegment, new List<Segment>() { leftMostSegment, expectedRightMostSegment });

            Assert.IsTrue(expectedRightMostSegment.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest3()
        {
            /*
             * Right Most Segment From  (x1: 5.19174499488124, y1: -3.17382146856891) (x2 : 5.89703865456322, y2: -0.313463848747547)
             * ANGLE 27.4519615304637 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 5.30929393816157, y2: 2.11588097904594)
             * ANGLE 139.268474290272 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 4.13380450535827, y2: -1.56731924373773)
             * RightMost ANGLE 27.4519615304637 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 5.30929393816157, y2: 2.11588097904594)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectionPoint = new MapPoint(5.89703865456322, -0.313463848747547);
            var expectedRightMostSegment = new LineSegment(connectionPoint,
                                                           new MapPoint(5.30929393816157, 2.11588097904594));
            var leftMostSegment = new LineSegment(connectionPoint,
                                                  new MapPoint(4.13380450535827, -1.56731924373773));

            var startingSegment = new LineSegment(new MapPoint(5.19174499488124, -3.17382146856891), connectionPoint);


            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startingSegment, new List<Segment>() { leftMostSegment, expectedRightMostSegment });

            Assert.IsTrue(expectedRightMostSegment.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest4()
        {
            /*
             * Right Most Segment From  (x1: 5.19174499488124, y1: -3.17382146856891) (x2 : 5.89703865456322, y2: -0.313463848747547)
             * ANGLE 27.4519615304637 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 5.30929393816157, y2: 2.11588097904594)
             * ANGLE 139.268474290272 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 4.13380450535827, y2: -1.56731924373773)
             * RightMost ANGLE 27.4519615304637 (x1: 5.89703865456322, y1: -0.313463848747547) (x2 : 5.30929393816157, y2: 2.11588097904594)
             * 
             *                                                                                                                                                                                                                                                                                                                                                                                          
             */
            var connectionPoint = new MapPoint(5.89703865456322, -0.313463848747547);
            var expectedRightMostSegment = new LineSegment(connectionPoint,
                                                           new MapPoint(5.30929393816157, 2.11588097904594));
            var leftMostSegment = new LineSegment(connectionPoint,
                                                  new MapPoint(4.13380450535827, -1.56731924373773));

            var startingSegment = new LineSegment(new MapPoint(5.19174499488124, -3.17382146856891), connectionPoint);


            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startingSegment, new List<Segment>() { leftMostSegment, expectedRightMostSegment });

            Assert.IsTrue(expectedRightMostSegment.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest5()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectionPoint = new MapPoint(2.74280867654103, -2.74280867654103);
            var expectedRightMostSegment = new LineSegment(connectionPoint,
                                                           new MapPoint(2.86035761982136, -3.21300444966235));
            var wrongSegment1 = new LineSegment(connectionPoint,
                                                  new MapPoint(2.96658502324369, -2.7821933135607));

            var wrongSegment2 = new LineSegment(connectionPoint,
                                                  new MapPoint(3.06036833405948, -2.40184988636334));
            var wrongSegment3 = new LineSegment(connectionPoint,
                                                  new MapPoint(3.83291738346822, -1.57237617015605));

            var startingSegment = new LineSegment(new MapPoint(0.411421301481155, -2.78199165763448), connectionPoint);


            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startingSegment, new List<Segment>() { wrongSegment1, wrongSegment2, wrongSegment3, expectedRightMostSegment });

            Assert.IsTrue(expectedRightMostSegment.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest6()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectedPoint = new MapPoint(0, 3);
            var startSegment = new LineSegment(new MapPoint(0, 0), connectedPoint);
            var segment2 = new LineSegment(connectedPoint, new MapPoint(2, 4));

            var quad4 = MinkowskiSumUtility.CalculateAngle(startSegment, segment2);

            var seg3 = new LineSegment(connectedPoint, new MapPoint(-2, 4));
            var quad1 = MinkowskiSumUtility.CalculateAngle(startSegment, seg3);

            var seg4 = new LineSegment(connectedPoint, new MapPoint(-2, -4));
            var quad2 = MinkowskiSumUtility.CalculateAngle(startSegment, seg4);

            var seg5 = new LineSegment(connectedPoint, new MapPoint(2, -4));
            var quad3 = MinkowskiSumUtility.CalculateAngle(startSegment, seg5);

            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startSegment, new List<Segment>() { segment2, seg3, seg4, seg5  });

            Assert.IsTrue(seg5.IsEqual(actualRightMostSegment));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest7()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectedPoint = new MapPoint(3,0);
            var startSegment = new LineSegment(new MapPoint(0, 0), connectedPoint);
            var segment2 = new LineSegment(connectedPoint, new MapPoint(4, 1));

            var quad4 = MinkowskiSumUtility.CalculateAngle(startSegment, segment2);

            var seg3 = new LineSegment(connectedPoint, new MapPoint(4, -1));
            var quad1 = MinkowskiSumUtility.CalculateAngle(startSegment, seg3);

            var seg4 = new LineSegment(connectedPoint, new MapPoint(2, 1));
            var quad2 = MinkowskiSumUtility.CalculateAngle(startSegment, seg4);

            var seg5 = new LineSegment(connectedPoint, new MapPoint(2, -1));
            var quad3 = MinkowskiSumUtility.CalculateAngle(startSegment, seg5);

            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startSegment, new List<Segment>() { segment2, seg3, seg4, seg5 });

            Assert.IsTrue(actualRightMostSegment.IsEqual(seg5));
        }


        [TestMethod()]
        public void FindRightMostSegmentTest8()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectedPoint = new MapPoint(0, -4);
            var startSegment = new LineSegment(new MapPoint(0, 0), connectedPoint);
            var segment2 = new LineSegment(connectedPoint, new MapPoint(1, -3));

            var quad4 = MinkowskiSumUtility.CalculateAngle(startSegment, segment2);

            var seg3 = new LineSegment(connectedPoint, new MapPoint(1, -5));
            var quad1 = MinkowskiSumUtility.CalculateAngle(startSegment, seg3);

            var seg4 = new LineSegment(connectedPoint, new MapPoint(-1, -5));
            var quad2 = MinkowskiSumUtility.CalculateAngle(startSegment, seg4);

            var seg5 = new LineSegment(connectedPoint, new MapPoint(-1, -3));
            var quad3 = MinkowskiSumUtility.CalculateAngle(startSegment, seg5);

            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startSegment, new List<Segment>() { segment2, seg3, seg4, seg5 });

            Assert.IsTrue(actualRightMostSegment.IsEqual(seg5));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest9()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectedPoint = new MapPoint(-1, 0);
            var startSegment = new LineSegment(new MapPoint(0, 0), connectedPoint);
            var segment2 = new LineSegment(connectedPoint, new MapPoint(-2, 1));

            var quad4 = MinkowskiSumUtility.CalculateAngle(startSegment, segment2);

            var seg3 = new LineSegment(connectedPoint, new MapPoint(-2, -1));
            var quad1 = MinkowskiSumUtility.CalculateAngle(startSegment, seg3);

            var seg4 = new LineSegment(connectedPoint, new MapPoint(0, -1));
            var quad2 = MinkowskiSumUtility.CalculateAngle(startSegment, seg4);

            var seg5 = new LineSegment(connectedPoint, new MapPoint(0, 1));
            var quad3 = MinkowskiSumUtility.CalculateAngle(startSegment, seg5);

            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startSegment, new List<Segment>() { segment2, seg3, seg4, seg5 });

            Assert.IsTrue(actualRightMostSegment.IsEqual(seg5));
        }

        [TestMethod()]
        public void FindRightMostSegmentTest10()
        {
            /*
             * Right Most Segment From  (x1: 0.411421301481155, y1: -2.78199165763448) (x2 : 2.74280867654103, y2: -2.74280867654103)
             * ANGLE 283.07337984229 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.86035761982136, y2: -3.21300444966235)RM
             * ANGLE 349.055307086631 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 2.96658502324369, y2: -2.7821933135607)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.06036833405948, y2: -2.40184988636334)
             * ANGLE 46.0721708312237 (x1: 2.74280867654103, y1: -2.74280867654103) (x2 : 3.83291738346822, y2: -1.57237617015605)                                                                                                                                                                                                                                                                                                                                                                                         
             */
            var connectedPoint = new MapPoint(-2, 4);
            var startSegment = new LineSegment(new MapPoint(-3, 3), connectedPoint);
            var segment2 = new LineSegment(connectedPoint, new MapPoint(-3, 4));

            var quad4 = MinkowskiSumUtility.CalculateAngle(startSegment, segment2);

            var seg3 = new LineSegment(connectedPoint, new MapPoint(-2, 5));
            var quad1 = MinkowskiSumUtility.CalculateAngle(startSegment, seg3);

            var seg4 = new LineSegment(connectedPoint, new MapPoint(-1, 4));
            var quad2 = MinkowskiSumUtility.CalculateAngle(startSegment, seg4);

            var seg5 = new LineSegment(connectedPoint, new MapPoint(-2, 3));
            var quad3 = MinkowskiSumUtility.CalculateAngle(startSegment, seg5);

            var actualRightMostSegment = MinkowskiSumUtility.FindRightMostSegment(startSegment, new List<Segment>() { segment2, seg3, seg4, seg5 });

            Assert.IsTrue(actualRightMostSegment.IsEqual(seg5));
        }


        [TestMethod()]
        public void TestCounterClockwisePointOrder()
        {
            var polygon = new Polygon(new List<MapPoint>() { new MapPoint(1,0), new MapPoint(-1,0), new MapPoint(-2,1), new MapPoint(-1,2), new MapPoint(0,3), new MapPoint(1,2), new MapPoint(1,0) });

            var expectedOrder = new List<MapPoint>() { new MapPoint(1, 0), new MapPoint(1, 2), new MapPoint(0, 3), new MapPoint(-1, 2), new MapPoint(-2, 1), new MapPoint(-1, 0), new MapPoint(1, 0) };
            var mvm = new MainWindowViewModel();
            var orderedPolygon = polygon.OrderVerticiesCounterClockwise();
            var points = orderedPolygon.Parts.First().GetPoints().ToList();
            for(var index = 0; index < polygon.Parts.First().Count(); index++)
            {
                var actualPoint = points[index];
                var expectedPoint = expectedOrder[index];
                Assert.IsTrue(expectedPoint.IsEqual(actualPoint));
            }
        }

        [TestMethod()]
        public void TestCounterClockwisePointOrder2AlreadyCounterClockwise()
        {
            var expectedOrder = new List<MapPoint>() { new MapPoint(1, 0), new MapPoint(1, 2), new MapPoint(0, 3), new MapPoint(-1, 2), new MapPoint(-2, 1), new MapPoint(-1, 0), new MapPoint(1, 0) };
            var polygon = new Polygon(expectedOrder);           
            var mvm = new MainWindowViewModel();
            var orderedPolygon = polygon.OrderVerticiesCounterClockwise();
            var points = orderedPolygon.Parts.First().GetPoints().ToList();
            for (var index = 0; index < polygon.Parts.First().Count(); index++)
            {
                var actualPoint = points[index];
                var expectedPoint = expectedOrder[index];
                Assert.IsTrue(expectedPoint.IsEqual(actualPoint));
            }
        }

        [TestMethod()]
        public void RotatePolygonTest()
        {
            var originalPolygon = new Polygon(new List<MapPoint>() { new MapPoint(-6, 7, SpatialReferences.Wgs84), new MapPoint(-3, 6, SpatialReferences.Wgs84), new MapPoint(-5, 2, SpatialReferences.Wgs84), new MapPoint(-8, 3, SpatialReferences.Wgs84), new MapPoint(-6, 7, SpatialReferences.Wgs84) }, SpatialReferences.Wgs84);
            var expectedPolygonPoints = new List<MapPoint>() { new MapPoint(-7, -6, SpatialReferences.Wgs84), new MapPoint(-6, -3, SpatialReferences.Wgs84), new MapPoint(-2, -5, SpatialReferences.Wgs84), new MapPoint(-3, -8, SpatialReferences.Wgs84), new MapPoint(-7, -6, SpatialReferences.Wgs84) };

            var rotatedPolygon = originalPolygon.RotatePolygon(90.0);
            var rotatedPolygonPoints = rotatedPolygon.Parts.First().GetPoints().ToList();
            for(var index = 0; index < rotatedPolygonPoints.Count(); index++)
            {
                var actualPoint = rotatedPolygonPoints[index];
                var expectedPoint = expectedPolygonPoints[index];
                Assert.IsTrue(IsEqualXYCoordinates(actualPoint, expectedPoint));
            }
        }

        [TestMethod()]
        public void HasEqualSegmentAngles()
        {
            var polygonA = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(2, 1), new MapPoint(1, 2), new MapPoint(0, 0) }, SpatialReferences.Wgs84);
            var polygonB = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(3, 2), new MapPoint(2, 4), new MapPoint(0, 0) }, SpatialReferences.Wgs84);

            Assert.IsTrue(GeometryUtility.HasEqualSegmentAngles(polygonA, polygonB));            
        }

        [TestMethod()]
        public void HasEqualSegmentAngles2()
        {
            var polygonA = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(2, 0), new MapPoint(1, 2), new MapPoint(0, 0) }, SpatialReferences.Wgs84);
            var polygonB = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(2, 2), new MapPoint(1, 3), new MapPoint(0, 0) }, SpatialReferences.Wgs84);

            Assert.IsFalse(GeometryUtility.HasEqualSegmentAngles(polygonA, polygonB));
        }



        [TestMethod()]
        public void GetSegmentsWithinRangeTest()
        {
            var polygonA = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(4, 0), new MapPoint(4, 4), new MapPoint(2, 2), new MapPoint(0, 4), new MapPoint(0, 0) }, SpatialReferences.Wgs84);
            var polygonB = new Polygon(new List<MapPoint>() { new MapPoint(0, 0), new MapPoint(2, 1), new MapPoint(1, 2), new MapPoint(0, 0) }, SpatialReferences.Wgs84);

            foreach(var segment in polygonA.Parts.First())
            {
                Console.WriteLine($"Polygon A: Segment: {segment.SegmentToSting()} Angle: " + segment.CalculateAngle());
            }

            foreach (var segment in polygonB.Parts.First())
            {
                Console.WriteLine($"Polygon B: Segment: {segment.SegmentToSting()} Angle: " + segment.CalculateAngle());
            }

            var segments =  MinkowskiSumUtility.GetSegmentsWithinRange(0.0, 90.0, polygonB);
            Assert.IsTrue(segments.Count == 1, $"Has {segments.Count} segments when should be 1");
            Assert.IsTrue(segments[0].SegmentEpsilonEquals(new LineSegment(new MapPoint(0, 0), new MapPoint(2, 1))));

            var segments2 = MinkowskiSumUtility.GetSegmentsWithinRange(90.0, 225, polygonB);
            Assert.IsTrue(segments2.Count == 1, $"Has {segments2.Count} segments when should be 1");
            Assert.IsTrue(segments2[0].SegmentEpsilonEquals(new LineSegment(new MapPoint(2, 1), new MapPoint(1, 2))));

            var segments3 = MinkowskiSumUtility.GetSegmentsWithinRange(225.0, 135.0, polygonB);
            Assert.IsTrue(segments3.Count == 3, $"Has {segments3.Count} segments when should be 3");
            Assert.IsTrue(segments3[0].SegmentEpsilonEquals(new LineSegment(new MapPoint(0, 0), new MapPoint(2, 1))));
            Assert.IsTrue(segments3[1].SegmentEpsilonEquals(new LineSegment(new MapPoint(2, 1), new MapPoint(1, 2))));
            Assert.IsTrue(segments3[2].SegmentEpsilonEquals(new LineSegment(new MapPoint(1, 2), new MapPoint(0, 0))));

            var segments4 = MinkowskiSumUtility.GetSegmentsWithinRange(135.0, 270.0, polygonB);
            Assert.IsTrue(segments4.Count == 2, $"Has {segments4.Count} segments when should be 2");
            Assert.IsTrue(segments4[0].SegmentEpsilonEquals(new LineSegment(new MapPoint(2, 1), new MapPoint(1, 2))));
            Assert.IsTrue(segments4[1].SegmentEpsilonEquals(new LineSegment(new MapPoint(1, 2), new MapPoint(0, 0))));


            var segments5 = MinkowskiSumUtility.GetSegmentsWithinRange(270.0, 0.0, polygonB);
            Assert.IsTrue(segments5.Count == 0, $"Has {segments5.Count} segments when should be 0");


        }


        [TestMethod()]
        public void GetSegmentsWithinRangeTestMapExample()
        {
            var mapPoint1 = new MapPoint(-4.29103106769687, 4.55853308561756);
            var mapPoint2 = new MapPoint(-3.31581773313863, 3.55545651292909);
            var mapPoint3 = new MapPoint(-4.27245557561004, 2.63596965463132);
            var mapPoint4 = new MapPoint(-2.60066128779593, 3.05391822658485);
            var mapPoint5 = new MapPoint(-2.3406043985804,  4.54924533957415);

            var mapPoint6 = new MapPoint(-7.82037456419334, 2.68240838484838);
            var mapPoint7 = new MapPoint(-5.50772579938381, 2.96104076615074);
            var mapPoint8 = new MapPoint(-5.86066014903346, 4.20559873596791);
            var polygonA = new Polygon(new List<MapPoint>() { mapPoint1, mapPoint2, mapPoint3, mapPoint4, mapPoint5, mapPoint1});
            var polygonB = new Polygon(new List<MapPoint>() { mapPoint6, mapPoint7, mapPoint8, mapPoint6 });


            var segments = MinkowskiSumUtility.GetSegmentsWithinRange(6.86999, 105.83239, polygonA);
            Assert.IsTrue(segments.Count == 2, $"Has {segments.Count} segments when should be 2");
            Assert.IsTrue(segments[0].SegmentEpsilonEquals(new LineSegment(mapPoint3, mapPoint4)));
            Assert.IsTrue(segments[1].SegmentEpsilonEquals(new LineSegment(mapPoint4, mapPoint5)));
            
            var segments2 = MinkowskiSumUtility.GetSegmentsWithinRange(105.83239, 217.85618, polygonA);
            Assert.IsTrue(segments2.Count == 1, $"Has {segments2.Count} segments when should be 1");
            Assert.IsTrue(segments2[0].SegmentEpsilonEquals(new LineSegment(mapPoint5, mapPoint1)));

            var segments3 = MinkowskiSumUtility.GetSegmentsWithinRange(217.85618, 6.86999, polygonA);
            Assert.IsTrue(segments3.Count == 2, $"Has {segments3.Count} segments when should be 2");
            Assert.IsTrue(segments3[0].SegmentEpsilonEquals(new LineSegment(mapPoint1, mapPoint2)));
            Assert.IsTrue(segments3[1].SegmentEpsilonEquals(new LineSegment(mapPoint2, mapPoint3)));



            var segments4 = MinkowskiSumUtility.GetSegmentsWithinRange(179.72717, 314.19307, polygonB);
            Assert.IsTrue(segments4.Count == 1, $"Has {segments4.Count} segments when should be 1");
            Assert.IsTrue(segments4[0].SegmentEpsilonEquals(new LineSegment(mapPoint8, mapPoint6)));

            var segments5 = MinkowskiSumUtility.GetSegmentsWithinRange(314.19307, 223.86558, polygonB);
            Assert.IsTrue(segments5.Count == 3, $"Has {segments5.Count} segments when should be 3");
            Assert.IsTrue(segments5[0].SegmentEpsilonEquals(new LineSegment(mapPoint6, mapPoint7)));
            Assert.IsTrue(segments5[1].SegmentEpsilonEquals(new LineSegment(mapPoint7, mapPoint8)));
            Assert.IsTrue(segments5[2].SegmentEpsilonEquals(new LineSegment(mapPoint8, mapPoint6)));

            var segments6 = MinkowskiSumUtility.GetSegmentsWithinRange(223.86558, 14.03624, polygonB);
            Assert.IsTrue(segments6.Count == 1, $"Has {segments6.Count} segments when should be 2");
            Assert.IsTrue(segments6[0].SegmentEpsilonEquals(new LineSegment(mapPoint6, mapPoint7)));


            var segments7 = MinkowskiSumUtility.GetSegmentsWithinRange(14.03624, 80.13419, polygonB);
            Assert.IsTrue(segments7.Count == 0, $"Has {segments.Count} segments when should be 7");

            var segments8 = MinkowskiSumUtility.GetSegmentsWithinRange(80.13419, 179.72717, polygonB);
            Assert.IsTrue(segments8.Count == 1, $"Has {segments8.Count} segments when should be 1");
            Assert.IsTrue(segments8[0].SegmentEpsilonEquals(new LineSegment(mapPoint7, mapPoint8)));
        }

        public static Segment ModifySegment(MapPoint vertex, Segment segment)
        {
            return new LineSegment(new MapPoint(segment.StartPoint.X + vertex.X, segment.StartPoint.Y + vertex.Y), new MapPoint(segment.EndPoint.X + vertex.X, segment.EndPoint.Y + vertex.Y));
        }

        [TestMethod()]
        public void GetAugmentationForPolygonTestMapExample()
        {
            var mapPoint1 = new MapPoint(-4.29103106769687, 4.55853308561756);
            var mapPoint2 = new MapPoint(-3.31581773313863, 3.55545651292909);
            var mapPoint3 = new MapPoint(-4.27245557561004, 2.63596965463132);
            var mapPoint4 = new MapPoint(-2.60066128779593, 3.05391822658485);
            var mapPoint5 = new MapPoint(-2.3406043985804, 4.54924533957415);

            var mapPoint6 = new MapPoint(-7.82037456419334, 2.68240838484838);
            var mapPoint7 = new MapPoint(-5.50772579938381, 2.96104076615074);
            var mapPoint8 = new MapPoint(-5.86066014903346, 4.20559873596791);
            var polygonA = new Polygon(new List<MapPoint>() { mapPoint1, mapPoint2, mapPoint3, mapPoint4, mapPoint5, mapPoint1 });
            var polygonB = new Polygon(new List<MapPoint>() { mapPoint6, mapPoint7, mapPoint8, mapPoint6 });


            var segments = MinkowskiSumUtility.GetSegmentsWithinRange(6.86999, 105.83239, polygonA);
            var segments2 = MinkowskiSumUtility.GetSegmentsWithinRange(105.83239, 217.85618, polygonA);
            var segments3 = MinkowskiSumUtility.GetSegmentsWithinRange(217.85618, 6.86999, polygonA);
            var expectedModifiedSegments = new List<Segment>() { ModifySegment(mapPoint6, segments3[0]), ModifySegment(mapPoint6, segments3[1]),
                                                                 ModifySegment(mapPoint7, segments[0]),  ModifySegment(mapPoint7, segments[1]),
                                                                 ModifySegment(mapPoint8, segments2[0])                                                                 
                                                               };

            var actualSegments = MinkowskiSumUtility.GetAugmentationForPolygon(polygonB, polygonA, null);

            //1
            Assert.IsTrue(actualSegments[0].SegmentEpsilonEquals(expectedModifiedSegments[0]));
            Assert.IsTrue(actualSegments[1].SegmentEpsilonEquals(expectedModifiedSegments[1]));
            //2
            Assert.IsTrue(actualSegments[2].SegmentEpsilonEquals(expectedModifiedSegments[2]));
            //3
            Assert.IsTrue(actualSegments[3].SegmentEpsilonEquals(expectedModifiedSegments[3]));
            Assert.IsTrue(actualSegments[4].SegmentEpsilonEquals(expectedModifiedSegments[4]));
            

            var segments4 = MinkowskiSumUtility.GetSegmentsWithinRange(179.72717, 314.19307, polygonB);
            var segments5 = MinkowskiSumUtility.GetSegmentsWithinRange(314.19307, 223.86558, polygonB);
            var segments6 = MinkowskiSumUtility.GetSegmentsWithinRange(223.86558, 14.03624, polygonB);
            var segments8 = MinkowskiSumUtility.GetSegmentsWithinRange(80.13419, 179.72717, polygonB);
            
            var expectedModifiedSegments2 = new List<Segment>() { ModifySegment(mapPoint1, segments4[0]),
                                                                  ModifySegment(mapPoint2, segments5[0]), ModifySegment(mapPoint2, segments5[1]), ModifySegment(mapPoint2, segments5[2]),
                                                                  ModifySegment(mapPoint3, segments6[0]),
                                                                  ModifySegment(mapPoint5, segments8[0])
                                                               };

            var actualSegments2 = MinkowskiSumUtility.GetAugmentationForPolygon(polygonA, polygonB, null);

            Assert.IsTrue(actualSegments2[0].SegmentEpsilonEquals(expectedModifiedSegments2[0]));
            Assert.IsTrue(actualSegments2[1].SegmentEpsilonEquals(expectedModifiedSegments2[1]));
            Assert.IsTrue(actualSegments2[2].SegmentEpsilonEquals(expectedModifiedSegments2[2]));
            Assert.IsTrue(actualSegments2[3].SegmentEpsilonEquals(expectedModifiedSegments2[3]));
            Assert.IsTrue(actualSegments2[4].SegmentEpsilonEquals(expectedModifiedSegments2[4]));
            Assert.IsTrue(actualSegments2[5].SegmentEpsilonEquals(expectedModifiedSegments2[5]));
        }

        [TestMethod()]
        public void TestLineSegmentIntersection()
        {
            //TODO reverse segment order and assert both seg1, seg2 intersection and seg2, seg1, since they should be the same


            //should intersect (1.5) (1.5) -Crosses
            var startSegment = new LineSegment(new MapPoint(0, 0), new MapPoint(3, 3));
            var endSegment = new LineSegment(new MapPoint(1, 2), new MapPoint(2, 1));
            var intersectionPoint = SegmentIntersectionUtility.GetLineSegmentIntersection(startSegment, endSegment);
            var reversedIntersectionPoint = SegmentIntersectionUtility.GetLineSegmentIntersection(endSegment, startSegment);
            var expectedIntersectionPoint = new MapPoint(1.5, 1.5);
            Assert.IsTrue(intersectionPoint.MapPointEpsilonEquals(expectedIntersectionPoint));
            Assert.IsTrue(intersectionPoint.MapPointEpsilonEquals(reversedIntersectionPoint));

            //does not intersect (since segment intersection, they do not reach)
            var startSeg2 = new LineSegment(new MapPoint(0, 0), new MapPoint(0, 2));
            var endSeg2 = new LineSegment(new MapPoint(1, 1), new MapPoint(3, 1));
            //expect null?
            var intersectionPoint2 = SegmentIntersectionUtility.GetLineSegmentIntersection(startSeg2, endSeg2);
            var reversedIntersectionPoint2 = SegmentIntersectionUtility.GetLineSegmentIntersection(endSeg2, startSeg2);
            Assert.IsTrue(intersectionPoint2 == (null));
            Assert.IsTrue(reversedIntersectionPoint2 == (null));

            //does intersect (10.57, 4.71)
            var startSeg3 = new LineSegment(new MapPoint(10, 5), new MapPoint(12, 4));
            var endSeg3 = new LineSegment(new MapPoint(11, 5), new MapPoint(8, 3));
           
            var intersectionPoint3 = SegmentIntersectionUtility.GetLineSegmentIntersection(startSeg3, endSeg3);
            var reversedIntersectionPoint3 = SegmentIntersectionUtility.GetLineSegmentIntersection(endSeg3, startSeg3);

            var expectedIntersectionPoint3 = new MapPoint(10.571428571428571, 4.7142857142857144);
            Assert.IsTrue(intersectionPoint3.MapPointEpsilonEquals(expectedIntersectionPoint3));
            Assert.IsTrue(reversedIntersectionPoint3.MapPointEpsilonEquals(expectedIntersectionPoint3));

            //vertical lines non-overlap return null
            var segment4 = new LineSegment(new MapPoint(0, 4), new MapPoint(0, 3));
            var segment5 = new LineSegment(new MapPoint(0, 0), new MapPoint(0, 2));

            var intersectionPoint4 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment4, segment5);
            var reversedIntersectionPoint4 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment5, segment4);

            Assert.IsTrue(intersectionPoint4 == null);
            Assert.IsTrue(reversedIntersectionPoint4 == null);

            //vertical lines overlap -final segments (0,0) (0,2) and (0,2) (0,3)  and (0,3) (0,4)
            var segment6 = new LineSegment(new MapPoint(0, 2), new MapPoint(0, 3));
            var segment7 = new LineSegment(new MapPoint(0, 0), new MapPoint(0, 4));

            var intersectionPoint5 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment6, segment7);
            var reversedIntersectionPoint5 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment7, segment6);

            //horizontal lines non-overlap return null
            var segment8 = new LineSegment(new MapPoint(2, 0), new MapPoint(3, 0));
            var segment9 = new LineSegment(new MapPoint(0, 0), new MapPoint(1, 0));

            var intersectionPoint6 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment8, segment9);
            var reversedIntersectionPoint6 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment9, segment8);

            //horizontal lines overlap -final segments (0,0) (1,0) and (1,0) (2,0) and (2,0) (3,0)
            var segment10 = new LineSegment(new MapPoint(1, 0), new MapPoint(3, 0));
            var segment11 = new LineSegment(new MapPoint(0, 0), new MapPoint(2, 0));

            var intersectionPoint7 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment10, segment11);
            var reversedIntersectionPoint7 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment11, segment10);

           
            //non intersecting with one line horizontal return null
            var segment16 = new LineSegment(new MapPoint(0, 0),  new MapPoint(2, 0));
            var segment17 = new LineSegment(new MapPoint(-2, 0), new MapPoint(0, 1));

            var intersectionPoint10 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment16, segment17);
            var reversedIntersectionPoint10 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment17, segment16);

            //non intersecting with one line vertical return null
            var segment18 = new LineSegment(new MapPoint(0, 0), new MapPoint(0, 2));
            var segment19 = new LineSegment(new MapPoint(1, 0), new MapPoint(2, 2));

            var intersectionPoint11 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment18, segment19);
            var reversedIntersectionPoint11 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment19, segment18);

            //same exact line- non horizontal or vertical
            var intersectionPoint12 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment19, segment19); //should throw

            //same exact line- horizontal 
            var intersectionPoint13 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment19, segment19); //should throw

            //same exact line- vertical 
            var intersectionPoint14 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment16, segment16); //should throw

        }

        [TestMethod()]
        public void TestTIntersection()
        {
            //T intersection "meets" - not on horizontal or vertical (intersection 1,1)
            var segment12 = new LineSegment(new MapPoint(0, 2), new MapPoint(2, 0));
            var segment13 = new LineSegment(new MapPoint(1, 1), new MapPoint(3, 3));

            var isTIntersection = SegmentIntersectionUtility.IsTIntersection(segment12, segment13);
            var isTIntersectionReversed = SegmentIntersectionUtility.IsTIntersection(segment13, segment12);

            Assert.IsTrue(isTIntersection);
            Assert.IsTrue(isTIntersectionReversed);

            //var intersectionPoint8 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment12, segment13);
            //var reversedIntersectionPoint8 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment13, segment12);
                      

            //T intersection "meets" on horizontal or vertical (intersection (1,0) )
            var segment14 = new LineSegment(new MapPoint(0, 0), new MapPoint(2, 0));
            var segment15 = new LineSegment(new MapPoint(1, 0), new MapPoint(1, -2));

            Assert.IsTrue(SegmentIntersectionUtility.IsTIntersection(segment14, segment15));
            Assert.IsTrue(SegmentIntersectionUtility.IsTIntersection(segment15, segment14));

            //var intersectionPoint9 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment14, segment15);
            //var reversedIntersectionPoint9 = SegmentIntersectionUtility.GetLineSegmentIntersection(segment15, segment14);

            //L intersection where the segments intersect at their start or end points
            //should return false
            var segment1 = new LineSegment(new MapPoint(8, 9), new MapPoint(10, 12));
            var segment2 = new LineSegment(segment1.StartPoint, new MapPoint(-2, 0));

            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment1, segment2));
            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment2, segment1));

            //L intersection with horizontal line -return false
            var segment3 = new LineSegment(new MapPoint(8, 0), new MapPoint(10, 0));
            var segment4 = new LineSegment(segment1.StartPoint, new MapPoint(-2, 3));

            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment3, segment4));
            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment4, segment3));

            //L intersection with vertical line- return false
            var segment5 = new LineSegment(new MapPoint(0, 8), new MapPoint(0, 10));
            var segment6 = new LineSegment(segment1.StartPoint, new MapPoint(-2, 3));

            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment5, segment6));
            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment6, segment5));

            //L intersection with vertical line and horizontal line- return false
            var segment7 = new LineSegment(new MapPoint(0, 0),  new MapPoint(2, 0));
            var segment8 = new LineSegment(segment1.StartPoint, new MapPoint(0, -2));

            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment5, segment6));
            Assert.IsFalse(SegmentIntersectionUtility.IsTIntersection(segment6, segment5));


        }

        [TestMethod()]
        public void TestIsCollinear()
        {
            var diagonalPoint1 = new MapPoint(0, 0);
            var diagonalPoint2 = new MapPoint(1, 1);
            var diagonalPoint3 = new MapPoint(2, 2);
            var diagonalPoint4 = new MapPoint(3, 3);
            
            //     |------|
            // |------|
            var overlapSegment = new LineSegment(diagonalPoint1, diagonalPoint3);
            var overlapSegment2 = new LineSegment(diagonalPoint2, diagonalPoint4);

            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegment, overlapSegment2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegment2, overlapSegment));

            //|-------------|
            //    |----|
            var withinSegment = new LineSegment(diagonalPoint1, diagonalPoint4);
            var withinSegment2 = new LineSegment(diagonalPoint2, diagonalPoint3);

            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegment, withinSegment2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegment2, withinSegment));
            
            //|---|
            //       |---|
            var separateSegment = new LineSegment(diagonalPoint1, diagonalPoint2);
            var separateSegment2 = new LineSegment(diagonalPoint3, diagonalPoint4);

            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegment, separateSegment2));
            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegment2, separateSegment));

            var horizontalPoint1 = new MapPoint(0, 0);
            var horizontalPoint2 = new MapPoint(1, 0);
            var horizontalPoint3 = new MapPoint(2, 0);
            var horizontalPoint4 = new MapPoint(3, 0);

            //     |------|
            // |------|
            var overlapSegmentHorizontal = new LineSegment(horizontalPoint1, horizontalPoint3);
            var overlapSegmentHorizontal2 = new LineSegment(horizontalPoint2, horizontalPoint4);

            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegmentHorizontal, overlapSegmentHorizontal2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegmentHorizontal2, overlapSegmentHorizontal));

            //|-------------|
            //    |----|
            var withinSegmentHorizontal = new LineSegment(horizontalPoint1, horizontalPoint4);
            var withinSegmentHorizontal2 = new LineSegment(horizontalPoint2, horizontalPoint3);
            
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegmentHorizontal, withinSegmentHorizontal2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegmentHorizontal2, withinSegmentHorizontal));

            //|---|
            //       |---|
            var separateSegmentHorizontal = new LineSegment(horizontalPoint1, horizontalPoint2);
            var separateSegmentHorizontal2 = new LineSegment(horizontalPoint3, horizontalPoint4);
            
            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegmentHorizontal, separateSegmentHorizontal2));
            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegmentHorizontal2, separateSegmentHorizontal));
            
            var verticalPoint1 = new MapPoint(0,0);
            var verticalPoint2 = new MapPoint(0,1);
            var verticalPoint3 = new MapPoint(0,2);
            var verticalPoint4 = new MapPoint(0,3);

            //     |------|
            // |------|
            var overlapSegmentVertical = new LineSegment(verticalPoint1, verticalPoint3);
            var overlapSegmentVertical2 = new LineSegment(verticalPoint2, verticalPoint4);
            
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegmentVertical, overlapSegmentVertical2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(overlapSegmentVertical2, overlapSegmentVertical));

            //|-------------|
            //    |----|
            var withinSegmentVertical = new LineSegment(verticalPoint1, verticalPoint4);
            var withinSegmentVertical2 = new LineSegment(verticalPoint2, verticalPoint3);
            
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegmentVertical, withinSegmentVertical2));
            Assert.IsTrue(SegmentIntersectionUtility.IsCollinear(withinSegmentVertical2, withinSegmentVertical));

            //|---|
            //       |---|
            var separateSegmentVertical = new LineSegment(verticalPoint1, verticalPoint2);
            var separateSegmentVertical2 = new LineSegment(verticalPoint3, verticalPoint4);

            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegmentVertical, separateSegmentVertical2));
            Assert.IsFalse(SegmentIntersectionUtility.IsCollinear(separateSegmentVertical2, separateSegmentVertical));

        }



        public bool IsEqualXYCoordinates(MapPoint point1, MapPoint point2)
        {
            return GeometryUtility.IsEqual(point1.X,point2.X, GeometryUtility.Epsilon) && GeometryUtility.IsEqual(point1.Y, point2.Y, GeometryUtility.Epsilon);
        }
    }
}