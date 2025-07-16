using System.Runtime.CompilerServices;

namespace Trdk.Geometry
{
    public class Vec2 
        : IEquatable<Vec2>, IComparable<Vec2>
    {
        public double X { get; }
        public double Y { get; }
        
        /// <summary>
        /// 点の初期化
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        public Vec2(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// 単位方向ベクトルの初期化
        /// </summary>
        /// <param name="argument">x軸正方向を正として反時計回り。radian</param>
        public Vec2(double argument)
        {
            X = Math.Cos(argument);
            Y = Math.Sin(argument);
        }

        public static Vec2 operator *(Vec2 p, double value) => new Vec2(p.X * value, p.Y * value);
        public static Vec2 operator *(double value, Vec2 p) => p * value;
        public static Vec2 operator +(Vec2 p, Vec2 q) => new Vec2(p.X + q.X, p.Y + q.Y);
        public static Vec2 operator -(Vec2 p, Vec2 q) => new Vec2(p.X - q.X, p.Y - q.Y);
        public static Vec2 operator -(Vec2 p) => new Vec2(-p.X, -p.Y);
        

        public static Vec2 Origin => new Vec2(0, 0);
        public double Length => Math.Sqrt(Vec2.Dot(this, this));
        public Vec2 Rot90() => new Vec2(-Y, X);
        public double Cross(Vec2 p) => Vec2.Cross(this, p);
        public double Dot(Vec2 p) => Vec2.Dot(this, p);
        public Vec2 Normalized
        {
            get
            {
                var len = this.Length;
                if (Utils.IsZero(len))
                {
                    throw new DivideByZeroException("長さ0のベクトルを正規化することはできません");
                }
                return this * (1 / len);
            }
        }
        public Vec2 Rot(double radian)
        {
            var s = Math.Sin(radian);
            var c = Math.Cos(radian);
            return new Vec2(
                x: c * X - s * Y, 
                y: s * X + c * Y
                );
        }
        public Vec2 Rot(Direction2 d) => Rot(d.RadianValue);


        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Cross(Vec2 p, Vec2 q) => p.X * q.Y - p.Y * q.X;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double Dot(Vec2 p, Vec2 q) => p.X * q.X + p.Y * q.Y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double EuclidDistance(Vec2 p, Vec2 q) => (q - p).Length;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double ManhattanDistance(Vec2 p, Vec2 q) => Math.Abs(p.X - q.X) + Math.Abs(p.Y - q.Y);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vec2? other)
        {
            if (other == null) 
                return false;
            return Utils.IsZero(X - other.X) && Utils.IsZero(Y - other.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override bool Equals(object obj) => Equals(obj as Vec2);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() << 2);


        public override string ToString()
        {
            return $"{X:f6},{Y:f6}";
        }

        public int CompareTo(Vec2? other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (X.CompareTo(other.X) == 0)
            {
                return Y.CompareTo(other.Y);
            }
            return X.CompareTo(other.X);
        }
    }
}
