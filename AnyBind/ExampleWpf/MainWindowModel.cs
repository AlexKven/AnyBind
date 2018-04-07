using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ExampleWpf
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Color _SelectedColor = Colors.Black;
        public Color SelectedColor
        {
            get => _SelectedColor;
            set
            {
                _SelectedColor = value;
                OnPropertyChanged(nameof(SelectedColor));
            }
        }
    }
}
