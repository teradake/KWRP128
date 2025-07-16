namespace Trdk.Geometry.NTS
{
    public static class OffsetExtensions
    {
        /// <summary>
        /// refs: 
        ///     https://nettopologysuite.github.io/NetTopologySuite/api/NetTopologySuite.Operation.Buffer.BufferOp.html
        /// </summary>
        /// <param name="polygonWithHoles">オフセットしたい多角形</param>
        /// <param name="distance">オフセット量。正なら拡大方向にオフセット</param>
        /// <param name="mitleLimit">角の鋭さを決める数値。0に近いほどまるい</param>
        /// <returns></returns>
        public static IEnumerable<PolygonWithHoles> Offset(this PolygonWithHoles polygonWithHoles, double distance, double mitleLimit = 1)
        {
            var ntsPolygon = polygonWithHoles.ToNTSPolygon();

            var offsetParameter = new NetTopologySuite.Operation.Buffer.BufferParameters
            {
                QuadrantSegments = 3,
                JoinStyle = NetTopologySuite.Operation.Buffer.JoinStyle.Round,
                EndCapStyle = NetTopologySuite.Operation.Buffer.EndCapStyle.Round,
                MitreLimit = mitleLimit,
            };

            var bufferOp = new NetTopologySuite.Operation.Buffer.BufferOp(ntsPolygon, offsetParameter);
            var offsetGeom = bufferOp.GetResultGeometry(distance);

            return offsetGeom.ToNTSPolygonList().Select(p => p.ToPolygonWithHoles());
        }
    }
}
