using AnyBind.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ExampleWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            var dependencyManager = ((App)App.Current).AnyBindDependencyManager;
            dependencyManager.InitializeInstance(this);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private MainWindowModel _Model = new MainWindowModel();
        public MainWindowModel Model
        {
            get => _Model;
            set
            {
                _Model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        [DependsOn("Model.SelectedColor")]
        public byte Red
        {
            get => Model.SelectedColor.R;
            set => Model.SelectedColor = new System.Windows.Media.Color()
            {
                R = value,
                G = Model.SelectedColor.G,
                B = Model.SelectedColor.B,
                A = Model.SelectedColor.A
            };
        }

        [DependsOn("Model.SelectedColor")]
        public byte Green
        {
            get => Model.SelectedColor.G;
            set => Model.SelectedColor = new System.Windows.Media.Color()
            {
                R = Model.SelectedColor.R,
                G = value,
                B = Model.SelectedColor.B,
                A = Model.SelectedColor.A
            };
        }

        [DependsOn("Model.SelectedColor")]
        public byte Blue
        {
            get => Model.SelectedColor.B;
            set => Model.SelectedColor = new System.Windows.Media.Color()
            {
                R = Model.SelectedColor.R,
                G = Model.SelectedColor.G,
                B = value,
                A = Model.SelectedColor.A
            };
        }

        [DependsOn("Model.SelectedColor")]
        public byte Alpha
        {
            get => Model.SelectedColor.A;
            set => Model.SelectedColor = new System.Windows.Media.Color()
            {
                R = Model.SelectedColor.R,
                G = Model.SelectedColor.G,
                B = Model.SelectedColor.B,
                A = value
            };
        }

        [DependsOn("Model.SelectedColor")]
        public SolidColorBrush ColorBrush
        {
            get => new SolidColorBrush(Model.SelectedColor);
        }

        private bool _IsValidColorText = true;
        public bool IsValidColorText
        {
            get => _IsValidColorText;
            set
            {
                _IsValidColorText = value;
                OnPropertyChanged(nameof(IsValidColorText));
            }
        }

        [DependsOn("Model.SelectedColor")]
        public string ColorText
        {
            get => Model.SelectedColor.ToString();
            set
            {
                try
                {
                    Model.SelectedColor = (Color)ColorConverter.ConvertFromString(value);
                    IsValidColorText = true;
                }
                catch (Exception)
                {
                    IsValidColorText = false;
                    OnPropertyChanged("ColorName");
                }
            }
        }

        [DependsOn("IsValidColorText")]
        public SolidColorBrush ColorTextBorderBrush
        {
            get => new SolidColorBrush(IsValidColorText ? Colors.Gray : Colors.Red);
        }
    }
}
