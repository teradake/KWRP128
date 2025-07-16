using System.Runtime.CompilerServices;

namespace Trdk.Geometry
{
    public class Direction2 : IEquatable<Direction2>
    {
        private readonly double _raw;
        public double DegreeValue => _raw * (180 / Math.PI);
        public double RadianValue => _raw;

        private Direction2(double radian_value)
        {
            _raw = Utils.RoundRadian(radian_value);
        }

        public static Direction2 ByRadian(double radian_value) => new Direction2(radian_value);
        public static Direction2 ByDegree(double degree_value) => new Direction2(degree_value * (Math.PI / 180));

        public static Direction2 operator +(Direction2 a, Direction2 b) => ByRadian(a.RadianValue + b.RadianValue);
        public static Direction2 operator -(Direction2 a, Direction2 b) => ByRadian(a.RadianValue - b.RadianValue);

        public override string ToString()
        {
            var deg = DegreeValue;
            if (deg < 0) deg += 360;
            return $"{deg:f3} deg.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Direction2? other)
        {
            if (other == null)
                return false;
            return Utils.IsZero(RadianValue - other.RadianValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object? obj) => Equals(obj as Direction2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => _raw.GetHashCode();

    }
}
