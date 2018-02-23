using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AnyBind
{
    internal class DependentPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public string ObjectPath { get; private set; }

        public bool SecondaryEvent { get; private set; }

        public DependentPropertyChangedEventArgs(string propertyName) : base(propertyName) { }

        public DependentPropertyChangedEventArgs(string propertyName, string objectPath, bool secondaryEvent)
            : base(propertyName)
        {
            ObjectPath = objectPath;
            SecondaryEvent = secondaryEvent;
        }
    }
}
