using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace MathematicalMorphology.src.Utility
{
    public static class MapUtility
    {
        public static void AddPolygonToMap(this Polygon polygon, MapView mapView, Color color)
        {
            var PolygonOverlay = mapView.GraphicsOverlays["PolygonGraphicsOverlay"];

            var graphic = new Graphic(polygon);
            var symbol = new SimpleFillSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            PolygonOverlay.Graphics.Add(graphic);
        }

        public static void AddPolylineToMap(this Polyline polyline, MapView mapView, Color color)
        {
            var PolygonOverlay = mapView.GraphicsOverlays["PolylineGraphicsOverlay"];

            var graphic = new Graphic(polyline);
            var symbol = new SimpleLineSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            PolygonOverlay.Graphics.Add(graphic);
        }


        public static void AddPointToMap(this MapPoint point, MapView mapView, Color color)
        {
            var pointOverlay = mapView.GraphicsOverlays["PointGraphicsOverlay"];

            var graphic = new Graphic() { Geometry = point };
            var symbol = new SimpleMarkerSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            pointOverlay.Graphics.Add(graphic);
        }


        public static Polyline ToPolyine(this Segment segment)
        {
            return new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint });
        }


        public static void AddSegmentToMap(this Segment segment, MapView mapView, Color color, String label)
        {
            var polylineOverlay = mapView.GraphicsOverlays["PolylineGraphicsOverlay"];
            var graphic = new Graphic(new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint }));

            // create a text symbol: define color, font, size, and text for the label
            var textSym = new TextSymbol();
            textSym.Color = Colors.Black;
            textSym.Font = new SymbolFont("Arial", 16);
            textSym.Text = label;// + String.Format("\n{0:F5}", segment.CalculateAngle());
            textSym.Angle = -60;
            var textGraphic = new Graphic(new MapPoint((segment.StartPoint.X + segment.EndPoint.X) / 2.0, (segment.StartPoint.Y + segment.EndPoint.Y) / 2.0), textSym);

            var symbol = new SimpleLineSymbol
            {
                Color = color
            };
            graphic.Symbol = symbol;

            var pointOverlay = mapView.GraphicsOverlays["PointGraphicsOverlay"];
            pointOverlay.Graphics.Add(textGraphic);
            polylineOverlay.Graphics.Add(graphic);
        }
    }
}
