namespace KWRP.Avalonia.Backend.Services
{
    public interface IKWRPApplicationService
    {
        Task SaveMachineConfigAsync();
        Task SaveLaneArrangementConfigAsync();
    }
}
