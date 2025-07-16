namespace KWRP.Avalonia.Backend.Enums
{
    public enum FileType
    {
        None = 0,
        XML = 1 << 0,
        JSON = 1 << 1,
        CSV = 1 << 2,
        PNG = 1 << 3,
        DXF = 1 << 4,
    }
}
