# What is AnyBind?
AnyBind is a library for .NET that uses custom attributes to specify a dependency chain for ordinary CLR properties on classes. This allows you to take a class that implements ```INotifyPropertyChanged```, and make certain properties dependent on other properties, either in the object or in a sub property of the object. ```PropertyChanged``` will then fire for the dependent property whenever a property it depends on changes. For example:
```C#
    public class Multiplier: INotifyPropertyChanged
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
```

The magic happens in ```[DependsOn("FirstInt", "SecondInt")]```. This will cause ```PropertyChanged``` to fire for ```Multiplication``` whenever ```FirstInt``` or ```SecondInt``` change. Since many UI and binding frameworks support binding to instances of ```INotifyPropertyChanged```, this has a wide application for scenarios involving viewmodels and calculated properties. Here are some examples:

* Binding UI to complex models with changing data
* Binding UI to multiple models or multiple properties in a model with a calculated property in the viewmodel, essentially acting like a ```MultiBinding``` in WPF (even if your framework doesn't support ```MultiBinding```)
* Binding to two different properties that share the same data (i.e., redundant accessors), or allowing setting a value via a calculated property and applying the change to the original property (AnyBind supports recursive references within a class.


AnyBind also works with subproperties. In fact, the following are all valid DependsOn attributes:

* ```[DependsOn("Property")]```
	* Property updates when SubProperty2 changes
* ```[DependsOn("Property.SubProperty1.Subrperty2")]```
	* Property updates when SubProperty2 changes
	* <del> Property updates when SubProperty2 and Property change </del> **Coming Soon!** 
* Prop1:```[DependsOn("Prop2", "Prop3")]```, Prop2:```[DependsOn("Prop1", "Prop3")]```, Prop3:```[DependsOn("Prop2", "Prop1")]```
	* Prop1 updates when Prop2 or Prop3 changes
	* Prop2 updates when... you get the idea
	* AnyBind is smart enough to avoid infinite recursion in this case
	* 
* ```[DependsOn("ObservableCollection[5]")]```
	* <del> Property updates when fifth element of ObservableCollection changes </del> **Coming Soon!** 
* ```[DependsOn("ObservableCollection[<CurIndex>]")]```
	* <del> Property updates when CurIndex'th element of ObservableCollection changes </del> **Coming Soon!** 
	* <del> Property updates when CurIndex property changes </del> **Coming Soon!** 