using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind.Internal
{
    internal abstract class GeneralSubscribableBase : ISubscribable
    {
        private object Instance { get; }
        private Type InstanceType { get; }
        private TypeInfo InstanceTypeInfo { get; }
        private List<string> Properties { get; } = new List<string>();

        public GeneralSubscribableBase(object instance)
        {
            Instance = instance;
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
        }

        public IEnumerable<string> SubscribableProperties => Properties;

        public abstract event PropertyChangedEventHandler PropertyChanged;

        public object GetPropertyValue(string propertyPath)
        {
            if (!ReflectionHelpers.TryGetMemberPathValue(Instance, InstanceTypeInfo, propertyPath, out var result, false, true))
                return null;
            return result;
        }

        public abstract void RaisePropertyChanged(PropertyChangedEventArgs e);
    }
}
