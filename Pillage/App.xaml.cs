using System;
using System.IO;
using System.Windows;
using Pillage.ViewModels;
using Pillage.Views;

namespace Pillage
{
    public partial class App 
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            
            //Inject view into viewmodel
            var pm = new PersistanceManager();
            var view = Activator.CreateInstance<MainView>();

            MainViewModel vm;

            if (e.Args.Length == 1)
            {
                var folder = e.Args[0];

                vm = Directory.Exists(folder) 
                    ? new MainViewModel(view, pm, folder) 
                    : new MainViewModel(view, pm);
            }
            else
            {
                vm = new MainViewModel(view, pm);
            }
                
            view.Closing += (o,a)=> Current.Shutdown();
            view.DataContext = vm;

            vm.ShowView();
        }
    }
}