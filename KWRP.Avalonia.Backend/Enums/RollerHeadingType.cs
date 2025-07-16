namespace KWRP.Avalonia.Backend.Enums
{
    public enum RollerHeadingType
    {
        ToRight = 1 << 0,    // 作業進捗方向が下から上に向かうとして、ローラが右向きであることを示す
        ToLeft = 1 << 1,     // 作業進捗方向が下から上に向かうとして、ローラが左向きであることを示す
        Straight = 1 << 2,  // ローラの向きが作業進捗方向と一致することを示す
    }

    public static class RollerHeadingTypeExtensions
    {
        public static double ToAngleRadian(this RollerHeadingType type)
        {
            return type switch
            {
                RollerHeadingType.ToRight => -Math.PI * 0.5,
                RollerHeadingType.ToLeft => Math.PI * 0.5,
                RollerHeadingType.Straight => 0.0,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
