using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Backend.Services
{
    public interface IWorkAreaCutService
    {
        void InitializeCutProperties(int count);
        void RefleshCutProperties();
        void Cut();
    }
}
