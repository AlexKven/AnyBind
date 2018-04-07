using AnyBind;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExampleWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public DependencyManager AnyBindDependencyManager { get; } = new DependencyManager();

        public App()
        {
            AnyBindDependencyManager.RegisterClass(typeof(MainWindowViewModel));
            AnyBindDependencyManager.RegisterClass(typeof(MainWindowModel));

            AnyBindDependencyManager.FinalizeRegistrations();
        }
    }
}
