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
        public const String PolygonOverlayName = "PolygonGraphicsOverlay";
        public const String PointOverlayName = "PointGraphicsOverlay";
        public const String PolylineOverlayName = "PolylineGraphicsOverlay";

        /// <summary>
        /// Adds a polygon to the MapView
        /// </summary>
        /// <param name="polygon">polygon to add</param>
        /// <param name="mapView">the map view to add it to</param>
        /// <param name="color">the color of the polygon</param>
        public static void AddPolygonToMap(this Polygon polygon, MapView mapView, Color color)
        {
            var PolygonOverlay = mapView.GraphicsOverlays[PolygonOverlayName];

            var graphic = new Graphic(polygon);
            var symbol = new SimpleFillSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            PolygonOverlay.Graphics.Add(graphic);
        }

        /// <summary>
        /// Adds a polyline to the map
        /// </summary>
        /// <param name="polyline">the polyline to add to the map</param>
        /// <param name="mapView">the map view</param>
        /// <param name="color">the color of the segment</param>
        public static void AddPolylineToMap(this Polyline polyline, MapView mapView, Color color)
        {
            var PolygonOverlay = mapView.GraphicsOverlays[PolylineOverlayName];

            var graphic = new Graphic(polyline);
            var symbol = new SimpleLineSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            PolygonOverlay.Graphics.Add(graphic);
        }

        /// <summary>
        /// Adds a point to the map
        /// </summary>
        /// <param name="point">the point to add</param>
        /// <param name="mapView">the map</param>
        /// <param name="color">the color of the point</param>
        public static void AddPointToMap(this MapPoint point, MapView mapView, Color color)
        {
            var pointOverlay = mapView.GraphicsOverlays[PointOverlayName];

            var graphic = new Graphic() { Geometry = point };
            var symbol = new SimpleMarkerSymbol()
            {
                Color = color
            };
            graphic.Symbol = symbol;

            pointOverlay.Graphics.Add(graphic);
        }

        /// <summary>
        /// converts a segment to a polyline
        /// </summary>
        /// <param name="segment">teh segment to convert</param>
        /// <returns>the equivalent polyline</returns>
        public static Polyline ToPolyine(this Segment segment)
        {
            return new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint });
        }

        /// <summary>
        /// Adds a segment to the map
        /// </summary>
        /// <param name="segment">the segment to add to the map</param>
        /// <param name="mapView">the map</param>
        /// <param name="color">the color of the segment</param>
        /// <param name="label">the label to add to the segment's midpoint</param>
        public static void AddSegmentToMap(this Segment segment, MapView mapView, Color color, String label)
        {
            var polylineOverlay = mapView.GraphicsOverlays[PolylineOverlayName];
            var graphic = new Graphic(new Polyline(new List<MapPoint>() { segment.StartPoint, segment.EndPoint }));

            // create a text symbol: define color, font, size, and text for the label
            var textSym = new TextSymbol();
            textSym.Color = Colors.Black;
            textSym.Font = new SymbolFont("Arial", 16);
            textSym.Text = label;
            textSym.Angle = -60;
            var textGraphic = new Graphic(new MapPoint((segment.StartPoint.X + segment.EndPoint.X) / 2.0, (segment.StartPoint.Y + segment.EndPoint.Y) / 2.0), textSym);

            var symbol = new SimpleLineSymbol
            {
                Color = color
            };
            graphic.Symbol = symbol;

            var pointOverlay = mapView.GraphicsOverlays[PointOverlayName];
            pointOverlay.Graphics.Add(textGraphic);
            polylineOverlay.Graphics.Add(graphic);
        }
    }
}
