﻿using System.Collections.Generic;
using Esri.ArcGISRuntime.Geometry;
using MathematicalMorphology.src.Utility;

namespace MathematicalMorphology.src
{
    internal class MapPointEqualityComparison : IEqualityComparer<MapPoint>
    {
        public bool Equals(MapPoint x, MapPoint y)
        {
            return x.MapPointEpsilonEquals(y);
        }

        public int GetHashCode(MapPoint obj)
        {
            int hash = 17;

            hash = hash * 23 + obj.X.GetHashCode();
            hash = hash * 23 + obj.Y.GetHashCode();
            hash = hash * 23 + obj.Z.GetHashCode();

            return hash;
        }
    }
}