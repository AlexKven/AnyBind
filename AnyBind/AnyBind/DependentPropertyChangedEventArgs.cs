using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AnyBind
{
    internal class DependentPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public IEnumerable<string> PreviousPropertyPaths { get; private set; }

        public string CurrentPath { get; private set; }

        public DependentPropertyChangedEventArgs(string propertyName) : base(propertyName) { }

        public DependentPropertyChangedEventArgs(string currentPath, string propertyName, params string[] previousPropertyPaths)
            : base(propertyName)
        {
            CurrentPath = currentPath;
            PreviousPropertyPaths = previousPropertyPaths;
        }
    }
}
