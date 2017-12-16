
using System;
using GalaSoft.MvvmLight.Command;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using System.ComponentModel;
using System.Windows.Media;
using MathematicalMorphology.src.Utility;
using System.Linq;

namespace MathematicalMorphology.src
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private bool canSelectManyFeatures;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            CalculateMinkowskiSum = new RelayCommand<MapView>(ExecuteCalculateMinkowskiSum, CanExecuteMinkowskiSum);
            CalculateMinkowskiSumConvexHull = new RelayCommand<MapView>(ExecuteCalculateMinkowskiSumConvexHull, CanExecuteMinkowskiSum);
            GetModifiedSegments = new RelayCommand<MapView>(ExecuteGetModifiedSegments, CanExecuteMinkowskiSum);
            ShowAnglesCommand = new RelayCommand<MapView>(ExecuteShowAngles, CanExecuteMinkowskiSum);
            ClearAllCommand = new RelayCommand<MapView>(ExecuteClearMap);
            CalculateArrangement = new RelayCommand<MapView>(ExecuteCalculateArrangement, CanExecuteMinkowskiSum);
            SimplifyPolygon = new RelayCommand<MapView>(ExecuteSimplifyPolygon, CanExecuteMinkowskiSum);
        }       

        private void ExecuteSimplifyPolygon(MapView mapView)
        {
            var polygonA = (FirstSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygonB = (SecondSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var seg1 = MinkowskiSumUtility.GetAugmentationForPolygon(polygonA, polygonB);
            var seg2 = MinkowskiSumUtility.GetAugmentationForPolygon(polygonB, polygonA);

            seg1.AddRange(seg2);
            var arrangement = MinkowskiSumUtility.BreakUpPolygon(seg1);
            var polygon  = MinkowskiSumUtility.SimplifyPolygon(arrangement);
            polygon.AddPolygonToMap(mapView, Color.FromArgb(70, Colors.Green.R, Colors.Green.G, Colors.Green.B));
        }

        private void ExecuteCalculateArrangement(MapView mapView)
        {
            var polygonA = (FirstSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygonB = (SecondSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var seg1 = MinkowskiSumUtility.GetAugmentationForPolygon(polygonA, polygonB);
            var seg2 = MinkowskiSumUtility.GetAugmentationForPolygon(polygonB, polygonA);

            seg1.AddRange(seg2);
            var arrangement = MinkowskiSumUtility.BreakUpPolygon(seg1);
            
            foreach(var segment in arrangement)
            {
                segment.StartPoint.AddPointToMap(mapView, Colors.Red);
                segment.EndPoint.AddPointToMap(mapView, Colors.Blue);
                segment.AddSegmentToMap(mapView, Colors.Black, "");
            }

        }

        private void ExecuteGetModifiedSegments(MapView mapView)
        {
            var polygonA = (FirstSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygonB = (SecondSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();

            var segmentsA = MinkowskiSumUtility.GetAugmentationForPolygon(polygonA, polygonB);
            var segmentsB = MinkowskiSumUtility.GetAugmentationForPolygon(polygonB, polygonA);

            foreach(var segA in segmentsA)
            {
                segA.AddSegmentToMap(mapView, Colors.DarkGreen, "Modified by A verticies");
            }

            foreach (var segB in segmentsB)
            {
                segB.AddSegmentToMap(mapView, Colors.DarkGreen, "Modified by B verticies");
            }
        }

        private void ExecuteCalculateMinkowskiSumConvexHull(MapView mapView)
        {
            var polygonA = (FirstSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygonB = (SecondSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygon = polygonA.CalculateMinkowskiSumPolygons(polygonB, mapView);
            polygon.AddPolygonToMap(mapView, Color.FromArgb(85, Colors.Gray.R, Colors.Gray.G, Colors.Gray.B));
        }

        private void ExecuteClearMap(MapView mapview)
        {
            foreach(var overlay in mapview.GraphicsOverlays)
            {
                overlay.Graphics = new GraphicCollection();
            }
            firstSelectedGraphic = null;
            SecondSelectedGraphic = null;
        }

        private void ExecuteShowAngles(MapView mapview)
        {
            var polygonA = (FirstSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();
            var polygonB = (SecondSelectedGraphic.Geometry as Polygon).OrderVerticiesCounterClockwise();

            foreach(var segment in polygonA.Parts.First())
            {
                segment.AddSegmentToMap(mapview, Colors.Red, "Polygon A " + String.Format("\n{0:F5}", segment.CalculateAngle()));
            }

            foreach (var segment in polygonB.Parts.First())
            {
                segment.AddSegmentToMap(mapview, Colors.Blue, "Polygon B" + String.Format("\n{0:F5}", segment.CalculateAngle()));
            }
        }

        private bool CanExecuteMinkowskiSum(MapView mapView)
        {
            return FirstSelectedGraphic != null && SecondSelectedGraphic != null;
        }

        public RelayCommand<MapView> CalculateMinkowskiSum { get; set; }
        public RelayCommand<MapView> CalculateMinkowskiSumConvexHull { get; private set; }
        public RelayCommand<MapView> GetModifiedSegments { get; private set; }
        public RelayCommand<MapView> ShowAnglesCommand { get; set; }
        public RelayCommand<MapView> ClearAllCommand { get; set; }
        public RelayCommand<MapView> CalculateArrangement { get; private set; }
        public RelayCommand<MapView> SimplifyPolygon { get; private set; }

        private void ExecuteCalculateMinkowskiSum(MapView mapView)
        {
            var geometry1 = FirstSelectedGraphic.Geometry;
            var geometry2 = SecondSelectedGraphic.Geometry;

            Polygon polygon = null;
            if (geometry1 is Polygon && geometry2 is Polygon)
            {
                //Order the vertices in the correct direction
                var polygonA = (geometry1 as Polygon).OrderVerticiesCounterClockwise();
                var polygonB = (geometry2 as Polygon).OrderVerticiesCounterClockwise();
                //rotate if angles are too close
                while(GeometryUtility.HasEqualSegmentAngles(polygonA, polygonB))
                {
                    polygonA = polygonA.RotatePolygon();
                }
                polygon = polygonA.CalculateMinkowskiSumNonConvexPolygons(polygonB);
            }
            
            if(polygon == null)
            {
                return;
            }

            polygon.AddPolygonToMap(mapView, Color.FromArgb(75, Colors.Gray.R, Colors.Gray.G, Colors.Gray.B));
            mapView.SetViewAsync(polygon);
        }  

        

        public Boolean CanSelectManyFeatures
        {
            get
            {
           
                return canSelectManyFeatures;
            }

            set
            {
                canSelectManyFeatures = value;
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

                return firstSelectedGraphic;
            }

            set
            {
                firstSelectedGraphic = value;
                OnPropertyChanged("FirstSelectedGraphic");
                CalculateMinkowskiSum.RaiseCanExecuteChanged();
                CalculateMinkowskiSumConvexHull.RaiseCanExecuteChanged();
                ShowAnglesCommand.RaiseCanExecuteChanged();
                GetModifiedSegments.RaiseCanExecuteChanged();
                CalculateArrangement.RaiseCanExecuteChanged();
                SimplifyPolygon.RaiseCanExecuteChanged();
            }
        }

    public Graphic SecondSelectedGraphic
        {
            get
            {

                return secondSelectedGraphic;
            }

            set
            {
                secondSelectedGraphic = value;
                OnPropertyChanged("SecondSelectedGraphic");
                CalculateMinkowskiSum.RaiseCanExecuteChanged();
                CalculateMinkowskiSumConvexHull.RaiseCanExecuteChanged();
                ShowAnglesCommand.RaiseCanExecuteChanged();
                GetModifiedSegments.RaiseCanExecuteChanged();
                CalculateArrangement.RaiseCanExecuteChanged();
                SimplifyPolygon.RaiseCanExecuteChanged();
            }
        }
    }
}
