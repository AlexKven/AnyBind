using AnyBind.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyBind.Tests.TestClasses
{
    public class TestViewModel1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _FirstInt = 0;
        public int FirstInt
        {
            get => _FirstInt;
            set
            {
                _FirstInt = value;
                OnPropertyChanged(nameof(FirstInt));
            }
        }

        private int _SecondInt = 0;
        public int SecondInt
        {
            get => _SecondInt;
            set
            {
                _SecondInt = value;
                OnPropertyChanged(nameof(SecondInt));
            }
        }

        [DependsOn("FirstInt", "SecondInt")]
        public int Multiplication => FirstInt * SecondInt;
    }

    public class TestViewModel2 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private TestViewModel1 _Base = new TestViewModel1();
        public TestViewModel1 Base
        {
            get => _Base;
            set
            {
                _Base = value;
                OnPropertyChanged(nameof(Base));
            }
        }

        private int _ToAdd = 0;
        public int ToAdd
        {
            get => _ToAdd;
            set
            {
                _ToAdd = value;
                OnPropertyChanged(nameof(ToAdd));
            }
        }

        [DependsOn("ToAdd", "Base.Multiplication")]
        public int Addition => ToAdd + Base.Multiplication;

        [DependsOn("ToAdd", "Base.FirstInt", "Base.SecondInt")]
        public int SumAll => ToAdd + Base.FirstInt + Base.SecondInt;
    }
}
