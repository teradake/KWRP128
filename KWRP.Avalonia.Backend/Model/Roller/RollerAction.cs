namespace KWRP.Avalonia.Backend.Models.Roller
{
    public class RollerAction
    {
        public double RefSpeed { get; set; }
        public double EdgeSpeed { get; set; }
        public int RepeatNum { get; set; }

        public static RollerAction CreateDefault()
        {
            return new RollerAction
            {
                RefSpeed = 3.0,
                EdgeSpeed = 1.5,
                RepeatNum = 1,
            };
        }
    }
}
