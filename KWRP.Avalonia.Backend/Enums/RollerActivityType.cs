namespace KWRP.Avalonia.Backend.Enums
{
    public enum RollerActivityType
    {
        Move = 1 << 0,
        NonCompaction = 1 << 1,
        Compaction = 1 << 2,
    }

    public static class RollerActivityTypeExtensions
    {
        public static string ToTag(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => "移動",
                RollerActivityType.NonCompaction => "無起振転圧",
                RollerActivityType.Compaction => "転圧",
                _ => throw new NotImplementedException(),
            };
        }

        public static int GetID(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => 1,
                RollerActivityType.NonCompaction => 2,
                RollerActivityType.Compaction => 3,
                _ => throw new NotImplementedException()
            };
        }

        public static int ToWorkType(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => 2,
                RollerActivityType.NonCompaction => 1,
                RollerActivityType.Compaction => 1,
                _ => throw new NotImplementedException()
            };
        }

        public static bool ToComp(this RollerActivityType type)
        {
            return type switch
            {
                RollerActivityType.Move => false,
                RollerActivityType.NonCompaction => false,
                RollerActivityType.Compaction => true,
                _ => throw new NotImplementedException()
            };
        }
    }
}
