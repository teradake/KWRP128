using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Trdk.Geometry
{
    public static class Utils
    {
        /// <summary>
        /// sign関数
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] 
        public static int Sign(double x) => x < -Constants.C_EPS ? -1 : (x > Constants.C_EPS ? 1 : 0);

        /// <summary>
        /// xがゼロに十分近いか判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsZero(double x) => Sign(x) == 0;

        /// <summary>
        /// xが正か判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPositive(double x) => Sign(x) == 1;

        /// <summary>
        /// xが負か判定
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNegative(double x) => Sign(x) == -1;

        /// <summary>
        /// a, b, cの位置関係を返す。<br/>
        /// -1: clockwise (直線a->bについてcが右側)<br/>
        /// +1: counter clockwise (直線a->bについてcが左側)<br/>
        /// -2: c~a~b<br/>
        /// +2: a~b~c<br/>
        ///  0: a~c~b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ISP(Vec2 a, Vec2 b, Vec2 c)
        {
            var cross = Sign(Vec2.Cross(b - a, c - a));
            if (cross != 0)
            {
                return cross;
            }
            if (Sign(Vec2.Dot(b - a, c - a)) == -1) return -2;
            if (Sign(Vec2.Dot(a - b, c - b)) == -1) return +2;
            return 0;
        }

        /// <summary>
        /// angleを0から2*piの範囲に丸める
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RoundRadian(double angle)
        {
            var rem = angle % (Math.PI * 2);
            if (rem < 0)
                rem += Math.PI * 2;
            return rem;
        }

        /// <summary>
        /// angleを0から360の範囲に丸める
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double RoundDegree(double angle)
        {
            var rem = angle % 360.0;
            if (rem < 0)
                rem += 360;
            return rem;
        }
    }
}
