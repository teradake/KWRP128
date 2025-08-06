namespace Trdk.Geometry.NTS
{
    public static class PolygonMergerOptions
    {
        private static double _tolerance = 0.0;
        public static double Tolerance
        {
            get => _tolerance;
            set => _tolerance = Math.Clamp(value, 0.0, 2.0);
        }
    }
}
