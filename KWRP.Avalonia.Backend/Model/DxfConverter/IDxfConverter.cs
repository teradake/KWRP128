namespace KWRP.Avalonia.Backend.Model.DxfConverter
{
    public interface IDxfConverter
    {
        void Convert();
        Task ConvertAsync();
    }
}
