using KWRP.Avalonia.Backend.Model.Shapes.Activity;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IActivityService
    {
        void CreateActivityGroups();
        void SetCurrentGroup(int groupId);
        Task SetOutputFolderPathAsync();
        ActivityModel? GetActivityModel(int groupId, int index);
        Task OutputActivitiesAsync();
    }
}
