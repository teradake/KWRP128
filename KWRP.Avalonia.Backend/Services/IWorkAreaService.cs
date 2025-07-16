using KWRP.Avalonia.Backend.Model.Shapes.WorkArea;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IWorkAreaService
    {
        void InitializeWorkAreas();
        void ClearSelectedWorkAreas();
        void UpdateWorkAreas(Func<WorkAreaModel, WorkAreaModel> workAreaUpdateFunc);
        void Undo();
    }
}
