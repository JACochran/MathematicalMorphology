using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using MathematicalMorphology.src;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MathematicalMorphology
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
            DrawShapes.ItemsSource = new DrawShape[]
            {
                DrawShape.Polygon
            };
            DrawShapes.SelectedIndex = 0;
        }

        Graphic _editGraphic = null;

        private async void OnDrawButtonClicked(object sender, RoutedEventArgs e)
        {
            if(canSelectManyFeatures())
            {
                return;
            }
            string message = null;
            var resultGeometry = _editGraphic == null ? null : _editGraphic.Geometry;

            var editCnfg = MyMapView.Editor.EditorConfiguration;
            editCnfg.AllowAddVertex = true;
            editCnfg.AllowDeleteVertex = true;
            editCnfg.AllowMoveGeometry = true;
            editCnfg.AllowMoveVertex = true;
            editCnfg.AllowRotateGeometry = true;
            editCnfg.AllowScaleGeometry = true;
            editCnfg.MaintainAspectRatio = true;
            editCnfg.VertexSymbol = new SimpleMarkerSymbol() { Style = SimpleMarkerStyle.Diamond, Color = Colors.Yellow, Size = 15 };

            try
            {
                var drawShape = (DrawShape)DrawShapes.SelectedItem;

                GraphicsOverlay graphicsOverlay;
                graphicsOverlay = drawShape == DrawShape.Point ? MyMapView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
                           ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
                  MyMapView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MyMapView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);

                var progress = new Progress<GeometryEditStatus>();
                progress.ProgressChanged += (a, b) =>
                {
                    //if (b.GeometryEditAction == GeometryEditAction..CompletedEdit)
                    //    if (_editGraphic != null)
                    //        _editGraphic.IsSelected = false;

                };

                var content = (sender as Button).Content.ToString();
                switch (content)
                {
                    case "Draw":
                        {
                            var r = await MyMapView.Editor.RequestShapeAsync(drawShape, null, progress);
                            graphicsOverlay.Graphics.Add(new Graphic() { Geometry = r });
                            break;
                        }
                    case "Edit":
                        {
                            if (_editGraphic == null)
                                return;
                            var g = _editGraphic;
                            g.IsVisible = false;
                            var r = await MyMapView.Editor.EditGeometryAsync(g.Geometry, null, progress);
                            resultGeometry = r ?? resultGeometry;
                            _editGraphic.Geometry = resultGeometry;
                            _editGraphic.IsSelected = false;
                            break;
                        }
                }

            }
            catch (TaskCanceledException)
            {
                // Ignore TaskCanceledException - usually happens if the editor gets cancelled or restarted
            }
            catch (Exception ex)
            {

                message = ex.Message;
            }
            finally
            {
                if (_editGraphic != null)
                {
                    _editGraphic.IsVisible = true;
                    _editGraphic = null;
                }
            }
            if (message != null)
                MessageBox.Show(message);
        }

        private bool canSelectManyFeatures()
        {
            return CanSelectManyFeatures.IsChecked ?? false;
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                return;

            if (canSelectManyFeatures())
            {
                var polylineOverlay = MyMapView.GraphicsOverlays["PolylineGraphicsOverlay"];
                var polylineGraphic = await polylineOverlay.HitTestAsync(MyMapView, e.Position);
                var polygonOverlay = MyMapView.GraphicsOverlays["PolygonGraphicsOverlay"];
                var polygonGraphic = await polygonOverlay.HitTestAsync(MyMapView, e.Position);

                var viewModel = this.DataContext as MainWindowViewModel;


                if (polygonGraphic != null)
                {
                    polygonGraphic.IsSelected = !polygonGraphic.IsSelected;
                    if(viewModel.FirstSelectedGraphic != null && viewModel.SecondSelectedGraphic != null && polygonGraphic.IsSelected)
                    {
                        viewModel.SecondSelectedGraphic.IsSelected = false;
                        viewModel.SecondSelectedGraphic = null;
                    }
                }

                if (polylineGraphic != null)
                {
                    polylineGraphic.IsSelected = !polylineGraphic.IsSelected;
                    if (viewModel.FirstSelectedGraphic != null && viewModel.SecondSelectedGraphic != null && polylineGraphic.IsSelected)
                    {
                        viewModel.SecondSelectedGraphic.IsSelected = false;
                        viewModel.SecondSelectedGraphic = null;
                    }
                }


             
                if(viewModel.FirstSelectedGraphic == null && viewModel.SecondSelectedGraphic == null)
                {
                    viewModel.FirstSelectedGraphic = polylineGraphic;
                    viewModel.SecondSelectedGraphic = polygonGraphic;
                }
                else if(viewModel.FirstSelectedGraphic == null) 
                {
                    viewModel.FirstSelectedGraphic = polygonGraphic != null ? polygonGraphic : polylineGraphic;
                }
                else if(viewModel.SecondSelectedGraphic == null)
                {
                    viewModel.SecondSelectedGraphic = polygonGraphic != null ? polygonGraphic : polylineGraphic;
                }
                           

            }
            else
            {
                var drawShape = (DrawShape)DrawShapes.SelectedItem;
                GraphicsOverlay graphicsOverlay;
                graphicsOverlay = drawShape == DrawShape.Point ? MyMapView.GraphicsOverlays["PointGraphicsOverlay"] as GraphicsOverlay :
                           ((drawShape == DrawShape.Polyline || drawShape == DrawShape.Freehand || drawShape == DrawShape.LineSegment) ?
                  MyMapView.GraphicsOverlays["PolylineGraphicsOverlay"] as GraphicsOverlay : MyMapView.GraphicsOverlays["PolygonGraphicsOverlay"] as GraphicsOverlay);


                var graphic = await graphicsOverlay.HitTestAsync(MyMapView, e.Position);

                if (graphic != null)
                {
                    //Clear previous selection
                    foreach (GraphicsOverlay gOLay in MyMapView.GraphicsOverlays)
                    {
                        gOLay.ClearSelection();
                    }

                    //Cancel editing if started
                    if (MyMapView.Editor.Cancel.CanExecute(null))
                        MyMapView.Editor.Cancel.Execute(null);

                    _editGraphic = graphic;
                    _editGraphic.IsSelected = true;
                }
            }
        }

          
    }
}
