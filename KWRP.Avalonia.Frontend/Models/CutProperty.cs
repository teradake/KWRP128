using KWRP.Avalonia.Frontend.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Models
{
    public class CutProperty : ViewModelBase
    {
        private int _ratio = 1;
        public int Ratio
        {
            get => _ratio;
            set
            {
                _ratio = Math.Clamp(value, 1, 100);
                OnPropertyChanged(nameof(Ratio));
            }
        }

        private double _percentage;
        public double Percentage
        {
            get => _percentage;
            set
            {
                _percentage = value;
                OnPropertyChanged(nameof(Percentage));
            }
        }
    }
}
