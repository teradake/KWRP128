namespace KWRP.Avalonia.Backend.Enums
{
    public enum GoalAreaLocation
    {
        Prev,
        Next,
    }

    public enum WorkAreaLaneTrimType
    {
        None,
        Cut,
    }

    public enum WorkAreaOrientation
    {
        Up,
        Down,
    }


    public static class WorkAreaEnumExtensions
    {
        public static WorkAreaOrientation Swap(this WorkAreaOrientation orientation)
        {
            return orientation switch
            {
                WorkAreaOrientation.Up => WorkAreaOrientation.Down,
                WorkAreaOrientation.Down => WorkAreaOrientation.Up,
                _ => WorkAreaOrientation.Up
            };
        }
    }
}
