using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal class GeneralSubscribable : ISubscribable
    {
        private object Instance { get; }
        private Type InstanceType { get; }
        private TypeInfo InstanceTypeInfo { get; }
        private List<string> Properties { get; } = new List<string>();

        public GeneralSubscribable(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
        }

        public IEnumerable<string> SubscribableProperties => Properties;

        public event PropertyChangedEventHandler PropertyChanged;

        public object GetPropertyValue(string propertyPath)
        {
            string propertyName = propertyPath.Substring(propertyPath.LastIndexOf('.') + 1);
            object parent = Instance;
            parent = ReflectionHelpers.GetParentOfSubentity(parent, InstanceTypeInfo, propertyPath);
            if (parent == null)
                return null;

            return ReflectionHelpers.SearchTypeAndBase(parent.GetType().GetTypeInfo(), t => t.DeclaredProperties.FirstOrDefault(pi => pi.Name == propertyName))?.GetValue(parent);
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            Instance.RaiseEvent("PropertyChanged", e);
        }
    }
}
