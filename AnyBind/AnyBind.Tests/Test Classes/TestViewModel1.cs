using AnyBind.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyBind.Tests.Test_Classes
{
    public class TestViewModel1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _FirstInt = 0;
        [DependsOn("Multiplication")]
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
        [DependsOn("Multiplication")]
        public int SecondInt
        {
            get => _SecondInt;
            set
            {
                _SecondInt = value;
                OnPropertyChanged(nameof(SecondInt));
            }
        }

        //[DependsOn("FirstInt", "SecondInt")]
        public int Multiplication => FirstInt * SecondInt;
    }
}
