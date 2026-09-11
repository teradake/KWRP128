namespace KWRP.Avalonia.Backend.Enums
{
    public enum RollerHeadingType
    {
        Right = 1 << 0,    // 作業進捗方向が下から上に向かうとして、ローラが右向きであることを示す
        Left = 1 << 1,     // 作業進捗方向が下から上に向かうとして、ローラが左向きであることを示す
        Forward = 1 << 2,  // ローラの向きが作業進捗方向と一致することを示す
        Backward = 1 << 3,
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
    }
}
