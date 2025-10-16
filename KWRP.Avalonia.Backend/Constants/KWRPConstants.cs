namespace KWRP.Avalonia.Backend
{
    public static class KWRPConstants
    {
        public const double C_INF = double.MaxValue / 10.0;
        public const double C_EPS = 1e-9;
        public const int C_IINF = 1_000_000_000;

        public const int C_DEFAULT_PAIRCOUNT_MIN = 2;
        public const int C_DEFAULT_PAIRCOUNT_MAX = 3;

        public const double C_SCALE_UP = 1.15;
        public const double C_SCALE_DOWN = 1.0 / 1.15;

        public const int C_LANE_COUNT_LIMIT = 1000; // レーンの数の上限
        public const double C_LANEARRANGE_THROTTLE_MSEC = 150;  // パラメータ変更が150ミリ秒以内に連続して行われた場合はレーン割を行わない
        
        public const double C_LINE_THICKNESS = 1.0;
        public const double C_LANE_PERIM_THICKNESS = 0.50;
        public const double C_PAIREDLANE_PERIM_THICKNESS = 1.2;
        public const double C_ACTIVITYGROUP_PERIM_THICKNESS = 2.3;
        public const double C_POCHIRULER_FONTSIZE = 10;
        public const double C_ACT_STROKE_THICKNESS = 4.5;

        public const int C_REPEAT_MAX = 10;
        public const double C_SPEED_MAX = 30;
        public const double C_MARGIN_MAX = 100;
    }
}
