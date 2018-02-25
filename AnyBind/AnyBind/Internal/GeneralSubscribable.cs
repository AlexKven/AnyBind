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
            if (!ReflectionHelpers.TryGetMemberPathValue(Instance, InstanceTypeInfo, propertyPath, out var result, false, true))
                return null;
            return result;
        }

        public void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            Instance.RaiseEvent("PropertyChanged", e);
        }
    }
}
