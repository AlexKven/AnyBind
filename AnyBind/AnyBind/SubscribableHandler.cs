using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AnyBind
{
    internal class SubscribableHandler
    {
        private WeakReference<ISubscribable> Instance;
        private Type InstanceType;
        private TypeInfo InstanceTypeInfo;
        private Dictionary<string, PropertyChangedEventHandler> ChangeHandlerDelegates = new Dictionary<string, PropertyChangedEventHandler>();
        private Dictionary<string, List<string>> PropertyDependencies = new Dictionary<string, List<string>>();

        public SubscribableHandler(ISubscribable instance)
        {
            Instance = new WeakReference<ISubscribable>(instance);
            InstanceType = instance.GetType();
            InstanceTypeInfo = InstanceType.GetTypeInfo();
            instance.PropertyChanged += GetChangeHandlerDelegate("");
            
            foreach (var dependency in DependencyManager.Registrations[InstanceType])
            {
                switch (dependency.Key)
                {
                    case PropertyDependency propertyDependency:
                        PropertyDependencies.Add(propertyDependency.PropertyName, dependency.Value);
                        break;
                }
            }
        }

        private PropertyChangedEventHandler GetChangeHandlerDelegate(string senderPath)
        {
            PropertyChangedEventHandler result;
            if (ChangeHandlerDelegates.TryGetValue(senderPath, out result))
                return result;
            else
                result = (s, e) => OnPropertyChanged(senderPath, e);
            ChangeHandlerDelegates.Add(senderPath, result);
            return result;
        }

        private void RaisePropertyChanged(string propertyPath, IEnumerable<string> previousProperties)
        {
            string objectPath = "";
            string propertyName = propertyPath;
            if (propertyPath.Contains("."))
            {
                var lastIndex = propertyPath.LastIndexOf('.');
                objectPath = propertyPath.Substring(0, lastIndex);
                propertyName = propertyPath.Substring(lastIndex + 1);
            }

            if (objectPath != "")
                return;

            var e = new DependentPropertyChangedEventArgs(objectPath, propertyName, previousProperties.ToArray());
            ISubscribable instance;
            if (Instance.TryGetTarget(out instance))
            {
                instance.RaisePropertyChanged(e);
            }
        }

        private void OnPropertyChanged(string path, PropertyChangedEventArgs e)
        {
            List<string> previousProperties = new List<string>();

            if (e is DependentPropertyChangedEventArgs)
            {
                var typedE = (DependentPropertyChangedEventArgs)e;
                if (typedE.CurrentPath != path)
                    return;
                previousProperties.AddRange(typedE.PreviousPropertyPaths);
            }

            string propertyPath = $"{path}.{e.PropertyName}".Trim('.');
            if (PropertyDependencies.TryGetValue(propertyPath, out var dependents))
            {
                foreach (var dependent in dependents)
                {
                    IEnumerable<string> dependentsEnumerable() { yield return dependent; }

                    if (previousProperties.Contains(dependent))
                        continue;
                    RaisePropertyChanged(dependent, previousProperties.Concat(dependentsEnumerable()));
                }
            }
        }
    }
}
