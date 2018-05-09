using System;
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
            var vm = new MainViewModel(view,pm);
            view.Closing += (o,a)=> Current.Shutdown();
            view.DataContext = vm;

            vm.ShowView();
        }
    }
}