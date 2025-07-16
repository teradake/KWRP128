namespace KWRP.Avalonia.Backend.Services
{
    public interface ILaneArrangementParameterService
    {
        Task<string?> SaveParamsAsync();
        Task<bool> LoadParamsAsync();
    }
}
