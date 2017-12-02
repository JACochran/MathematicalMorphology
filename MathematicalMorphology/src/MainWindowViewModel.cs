
using System;
using GalaSoft.MvvmLight.Command;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Esri.ArcGISRuntime.Symbology;
using System.Windows.Media;

namespace MathematicalMorphology.src
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool canSelectManyFeatures;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            CalculateMinkowskiSum = new RelayCommand<MapView>(ExecuteCalculateMinkowskiSum, CanExecuteMinkowskiSum);
        }

        private bool CanExecuteMinkowskiSum(MapView mapView)
        {
            return FirstSelectedGraphic != null && SecondSelectedGraphic != null;
        }

        public RelayCommand<MapView> CalculateMinkowskiSum { get; set; }

        private void ExecuteCalculateMinkowskiSum(MapView mapView)
        {
            var geometry1 = FirstSelectedGraphic.Geometry;
            var geometry2 = SecondSelectedGraphic.Geometry;
            Polygon polygon;
            if (geometry1 is Polygon && geometry2 is Polygon)
            {
                polygon = CalculateMinkowskiSumNonConvexPolygons((Polygon)geometry1, (Polygon)geometry2, mapView);
                AddPolygonToMap(polygon, mapView, Colors.Tan);
                polygon = CalculateMinkowskiSumPolygons((Polygon)geometry1, (Polygon)geometry2, mapView);
            }
            else if (geometry1 is Polyline && geometry2 is Polygon)
            {
                polygon = CalculateMinkowskiSumPolygonPolyline((Polygon)geometry2, (Polyline)geometry1);
            }
            else if (geometry1 is Polygon && geometry2 is Polyline)
            {
                polygon = CalculateMinkowskiSumPolygonPolyline((Polygon)geometry1, (Polyline)geometry2);
            }
            else
            {
                polygon = CalculateMinkowskiSumPolylines((Polyline)geometry1, (Polyline)geometry2);
            }
            
            if(polygon == null)
            {
                return;
            }

            AddPolygonToMap(polygon, mapView, Colors.Snow);
            mapView.SetViewAsync(polygon);
        }

        public static void AddPolygonToMap(Polygon polygon, MapView mapView, Color color)
        {
            var PolygonOverlay = mapView.GraphicsOverlays["PolygonGraphicsOverlay"];

            var graphic = new Graphic(polygon);
            var symbol = new SimpleFillSymbol(){
                                                   Color = color
                                               };
            graphic.Symbol = symbol;
            
            PolygonOverlay.Graphics.Add(graphic);
        }

        public static MapPoint GetTopRightPoint(Polygon polygon)
        {
            var topRightPoint = polygon.Parts.First().First().StartPoint;
            foreach(var part in polygon.Parts)
            {
                foreach(var point in part.GetPoints())
                {
                    if(point.Y > topRightPoint.Y)
                    {
                        topRightPoint = point;
                    }
                    else if(point.Y == topRightPoint.Y)
                    {
                        if(point.X > topRightPoint.X)
                        {
                            topRightPoint = point;
                        }
                    }
                    
                }
            }

            return topRightPoint;
        }

        public MapPoint GetBottomLeftPoint(Polygon polygon)
        {
            var bottomLeftPoint = polygon.Parts.First().StartPoint;

            foreach(var part in polygon.Parts)
            {
                foreach(var point in part.GetPoints())
                {
                    if(point.X < bottomLeftPoint.X &&
                       point.Y < bottomLeftPoint.Y)
                    {
                        bottomLeftPoint = point;
                    }
                }
            }

            return bottomLeftPoint;
        }

        public Polygon CalculateMinkowskiDifferencePolygons(Polygon polygon1, Polygon polygon2, MapView mapView)
        {
            var mirrorPolygon = FlipPolygon(polygon1);
            return CalculateMinkowskiSumPolygons(mirrorPolygon, polygon2, mapView);
        }

        private Polygon FlipPolygon(Polygon polygon1)
        {
            var polygonFlipped = new PolygonBuilder(polygon1.SpatialReference);

            foreach(var part in polygon1.Parts)
            {
                foreach(var point in part.GetPoints())
                {
                    polygonFlipped.AddPoint(new MapPoint(-1*point.X, -1*point.Y));
                }
            }

            return polygonFlipped.ToGeometry();
        }

        public class MinkowskiSegment
        {
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

        public Polygon CalculateMinkowskiSumNonConvexPolygons(Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var bottomLeft = GetBottomLeftPoint(polygon1);
            var bottomLeft2 = GetBottomLeftPoint(polygon2);
            MapPoint centerPoint = new MapPoint(0.0, 0.0, polygon1.SpatialReference);
            var distance = new KeyValuePair<double, double>((bottomLeft.X < centerPoint.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(bottomLeft.X, 0.0), new MapPoint(centerPoint.X, 0.0)),
                                                            (bottomLeft.Y < centerPoint.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, bottomLeft.Y), new MapPoint(0.0, centerPoint.Y)));
            var distance2 = new KeyValuePair<double, double>((bottomLeft2.X < centerPoint.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(bottomLeft2.X, 0.0), new MapPoint(centerPoint.X, 0.0)),
                                                             (bottomLeft2.Y < centerPoint.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, bottomLeft2.Y), new MapPoint(0.0, centerPoint.Y)));
            var a = TranslatePolygon(polygon1, distance);
            var b = TranslatePolygon(polygon2, distance2);

            //clockwise sort
            var sortedClockwisePolygon = a.Parts.First().ToList().Select(segment => new MinkowskiSegment(a, segment, true)).ToList();
            sortedClockwisePolygon.AddRange(b.Parts.First().Select(segment => new MinkowskiSegment(b, segment, false)).ToList());
            sortedClockwisePolygon.Sort(ClockwiseComparison);

            MapPoint previousPoint = null;

            var polygon = new PolygonBuilder(polygon1.SpatialReference);
      
            var mapPointMap = new HashSet<MapPoint>(new MapPointEqualityComparison());

            for(var index = 0; index < sortedClockwisePolygon.Count; index++)
            {
                var segment = sortedClockwisePolygon[index];
            
                var previousSegment = sortedClockwisePolygon[index - 1 < 0 ? sortedClockwisePolygon.Count - 1 : index - 1];
                var nextSegment = sortedClockwisePolygon[index + 1 >= sortedClockwisePolygon.Count ? 0 : index + 1];
                MapPoint point = nextSegment.Segment.EndPoint;
                //which point to use?

                AddSegmentToMap(segment.Segment, mapview, Colors.Chocolate, $"Original {index}");
                var movedSegment = new List<MapPoint>(){
                                                             new MapPoint(point.X + segment.Segment.StartPoint.X, point.Y + segment.Segment.StartPoint.Y),
                                                             new MapPoint(point.X + segment.Segment.EndPoint.X, point.Y + segment.Segment.EndPoint.Y)
                                                       };

                if(previousPoint != null)
                {
                    if(previousPoint.IsEqual(movedSegment.First()))
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

        private void AddSegmentToMap(Segment segment, MapView mapView, Color color, String label)
        {
            var polylineOverlay = mapView.GraphicsOverlays["PolylineGraphicsOverlay"];
            var graphic = new Graphic(new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint }));

            // create a text symbol: define color, font, size, and text for the label
            var textSym = new TextSymbol();
            textSym.Color = Colors.Red;
            textSym.Font = new SymbolFont("Arial", 16);
            textSym.Text = label + $"\n{CalculateAngle(segment)}";
            textSym.Angle = -60;
            var textGraphic = new Graphic(new MapPoint((segment.StartPoint.X + segment.EndPoint.X )/2.0, (segment.StartPoint.Y + segment.EndPoint.Y)/2.0), textSym);

            var symbol = new SimpleLineSymbol{
                                                 Color = color
                                             };
            graphic.Symbol = symbol;

            var pointOverlay = mapView.GraphicsOverlays["PointGraphicsOverlay"];
            pointOverlay.Graphics.Add(textGraphic);
            polylineOverlay.Graphics.Add(graphic);
        }

        private static Comparison<MinkowskiSegment> ClockwiseComparison => ((segment1, segment2) => CalculateAngle(segment1.Segment).CompareTo(CalculateAngle(segment2.Segment)));

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        
        private static double CalculateAngle(Segment segment)
        {
            //Translate start point to origin
            MapPoint point = new MapPoint(0.0, 0.0, segment.SpatialReference);
            var distance = new KeyValuePair<double, double>((segment.StartPoint.X < point.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(segment.StartPoint.X, 0.0), new MapPoint(point.X, 0.0)), //x distance
                                                            (segment.StartPoint.Y < point.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, segment.StartPoint.Y), new MapPoint(0.0, point.Y))); //y distance

            var startPoint = new MapPoint(segment.StartPoint.X + distance.Key, segment.StartPoint.Y + distance.Value);
            var endPoint =   new MapPoint(segment.EndPoint.X   + distance.Key, segment.EndPoint.Y   + distance.Value);
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

        public Polygon CalculateMinkowskiSumPolygons(Polygon polygon1, Polygon polygon2, MapView mapview)
        {
            var bottomLeftPoint =  GetBottomLeftPoint(polygon1);

            PartCollection sumParts = new PartCollection(polygon1.SpatialReference);
        
            foreach (var part in polygon2.Parts)
            {
                foreach(var point in part.GetPoints())
                {
                    var distance = new KeyValuePair<double, double>((bottomLeftPoint.X < point.X ? 1 : -1) * GeometryEngine.Distance(new MapPoint(bottomLeftPoint.X, 0.0), new MapPoint(point.X, 0.0)),
                                                                    (bottomLeftPoint.Y < point.Y ? 1 : -1) * GeometryEngine.Distance(new MapPoint(0.0, bottomLeftPoint.Y), new MapPoint(0.0, point.Y)));

                    var translatedPolygon = TranslatePolygon(polygon1, distance);


                    sumParts.AddParts(translatedPolygon.Parts);
                  
                    AddPolygonToMap(translatedPolygon, mapview, Colors.Purple);
                }
            }

            var sumPolygon = GeometryEngine.ConvexHull(new Polygon(sumParts));

            return sumPolygon as Polygon;
        }

        private Polygon TranslatePolygon(Polygon polygon1, KeyValuePair<double,double> distance)
        {
            var polygonBuilder = new PolygonBuilder(polygon1.SpatialReference);
            foreach(var part in polygon1.Parts)
            {
                foreach(var point in part.GetPoints())
                {
                    polygonBuilder.AddPoint(distance.Key + point.X, distance.Value + point.Y);
                }
            }

            return polygonBuilder.ToGeometry();
        }

        public Polygon CalculateMinkowskiSumPolylines(Polyline polygon1, Polyline polygon2)
        {



            return null;
        }

        public Polygon CalculateMinkowskiSumPolygonPolyline(Polygon polygon1, Polyline polygon2)
        {



            return null;
        }

        public Boolean CanSelectManyFeatures
        {
            get
            {
           
                return this.canSelectManyFeatures;
            }

            set
            {
                this.canSelectManyFeatures = value;
                OnPropertyChanged("CanSelectManyFeatures");
            }

        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Graphic firstSelectedGraphic;
        private Graphic secondSelectedGraphic;

        public Graphic FirstSelectedGraphic
        {
            get
            {

                return this.firstSelectedGraphic;
            }

            set
            {
                this.firstSelectedGraphic = value;
                OnPropertyChanged("FirstSelectedGraphic");
                CalculateMinkowskiSum.RaiseCanExecuteChanged();
            }
        }

    public Graphic SecondSelectedGraphic
        {
            get
            {

                return this.secondSelectedGraphic;
            }

            set
            {
                this.secondSelectedGraphic = value;
                OnPropertyChanged("SecondSelectedGraphic");
                CalculateMinkowskiSum.RaiseCanExecuteChanged();
            }
        }
    }
}
