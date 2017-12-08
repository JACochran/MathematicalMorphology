
using System;
using GalaSoft.MvvmLight.Command;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Controls;
using System.ComponentModel;
using System.Windows.Media;
using MathematicalMorphology.src.Utility;

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
            Polygon polygon = null;
            if (geometry1 is Polygon && geometry2 is Polygon)
            {
                polygon = ((Polygon)geometry1).CalculateMinkowskiSumNonConvexPolygons((Polygon)geometry2, mapView);
                polygon.AddPolygonToMap(mapView, Colors.Tan);
                polygon = ((Polygon)geometry1).CalculateMinkowskiSumPolygons((Polygon)geometry2, mapView);
            }
            
            if(polygon == null)
            {
                return;
            }

            polygon.AddPolygonToMap(mapView, Color.FromArgb(22, Colors.Snow.R, Colors.Snow.G, Colors.Snow.B));
            mapView.SetViewAsync(polygon);
            FirstSelectedGraphic.IsSelected = false;
            SecondSelectedGraphic.IsSelected = false;
            FirstSelectedGraphic = null;
            SecondSelectedGraphic = null;
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
