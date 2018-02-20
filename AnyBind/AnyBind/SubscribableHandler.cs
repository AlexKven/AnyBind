using AnyBind.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AnyBind
{
    internal class SubscribableHandler
    {
        private WeakReference<ISubscribable> Instance;
        private Type InstanceType;
        private Dictionary<string, PropertyChangedEventHandler> ChangeHandlerDelegates = new Dictionary<string, PropertyChangedEventHandler>();
        private Dictionary<string, List<string>> PropertyDependencies;

        public SubscribableHandler(ISubscribable instance)
        {
            Instance = new WeakReference<ISubscribable>(instance);
            InstanceType = instance.GetType();
            instance.PropertyChanged += GetChangeHandlerDelegate("");
            
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

        private void RaisePropertyChanged(DependentPropertyChangedEventArgs)
        {

        }

        private void OnPropertyChanged(string path, PropertyChangedEventArgs e)
        {
            string propertyPath = $"{path}.{e.PropertyName}";
            if (PropertyDependencies.TryGetValue(propertyPath, out var dependents))
            {
                var previousProperties = (e as DependentPropertyChangedEventArgs)?.PreviousPropertyPaths ?? new string[0];
                foreach (var dependent in dependents)
                {
                    if (previousProperties.Contains(dependent))
                        continue;
                    RaisePropertyChanged(new DependentPropertyChangedEventArgs(e, dependent));
                }
            }
        }
    }
}
