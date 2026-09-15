namespace KWRP.Avalonia.Backend.Enums
{
    public enum RollerHeadingType
    {
        Right = 1 << 0,    // 作業進捗方向が下から上に向かうとして、ローラが右向きであることを示す
        Left = 1 << 1,     // 作業進捗方向が下から上に向かうとして、ローラが左向きであることを示す
        Forward = 1 << 2,  // ローラの向きが作業進捗方向と一致することを示す
        Backward = 1 << 3,
    }

    public enum RollerWorkingDirectionType
    {
        Parallel,
        Parpendicular,
    }



    public static class RollerHeadingTypeExtensions
    {
        public static double ToAngleRadian(this RollerHeadingType type)
        {
            return type switch
            {
                RollerHeadingType.Right => -Math.PI * 0.5,
                RollerHeadingType.Left => Math.PI * 0.5,
                RollerHeadingType.Forward => 0.0,
                RollerHeadingType.Backward => Math.PI,
                _ => throw new NotImplementedException(),
            };
        }

        public static RollerHeadingType RotateClockwise(this RollerHeadingType head)
        {
            return head switch
            {
                RollerHeadingType.Left => RollerHeadingType.Forward,
                RollerHeadingType.Forward => RollerHeadingType.Right,
                RollerHeadingType.Right => RollerHeadingType.Backward,
                RollerHeadingType.Backward => RollerHeadingType.Left,
                _ => throw new NotImplementedException(),
            };
        }

        public static RollerHeadingType RotateCounterclockwise(this RollerHeadingType head)
        {
            return head switch
            {
                RollerHeadingType.Left => RollerHeadingType.Backward,
                RollerHeadingType.Backward => RollerHeadingType.Right,
                RollerHeadingType.Right => RollerHeadingType.Forward,
                RollerHeadingType.Forward => RollerHeadingType.Left,
                _ => throw new NotImplementedException(),
            };
        }

        public static RollerHeadingType ReverseDirection(this RollerHeadingType head)
        {
            return head switch
            {
                RollerHeadingType.Left => RollerHeadingType.Right,
                RollerHeadingType.Backward => RollerHeadingType.Forward,
                RollerHeadingType.Right => RollerHeadingType.Left,
                RollerHeadingType.Forward => RollerHeadingType.Backward,
                _ => throw new NotImplementedException(),
            };
        }

        public static bool IsParpendicular(this RollerHeadingType head) => head == RollerHeadingType.Left || head == RollerHeadingType.Right;
        public static bool IsParallel(this RollerHeadingType head) => head == RollerHeadingType.Forward || head == RollerHeadingType.Backward;

        public static RollerWorkingDirectionType ToWorkingDirectionType(this RollerHeadingType head)
        {
            if (head.IsParallel()) return RollerWorkingDirectionType.Parallel;
            else return RollerWorkingDirectionType.Parpendicular;
        }
    }

    public static class RollerHeadingPatternExtensions
    {
        public static RollerHeadingType ToRollerHeadingType(this string? pattern)
        {
            return pattern?.FirstOrDefault() switch
            {
                'r' or 'R' => RollerHeadingType.Right,
                'l' or 'L' => RollerHeadingType.Left,
                'f' or 'F' => RollerHeadingType.Forward,
                'b' or 'B' => RollerHeadingType.Backward,
                _ => RollerHeadingType.Right,
            };
        }
    }
}
