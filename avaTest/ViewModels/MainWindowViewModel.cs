using Talk.App.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Talk.Domain;
namespace Talk.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {
            NowViewModel = new TalkViewModel();
            Navicommand = ReactiveCommand.Create<string>(Navigate);
      }

        private ViewModelBase nowViewModel;
        public ViewModelBase NowViewModel
        {
            get => nowViewModel;
            set => this.RaiseAndSetIfChanged(ref nowViewModel, value);
        }
      

        public ReactiveCommand<string, Unit> Navicommand { get; }

        private void Navigate(string destination)
        {
            switch (destination)
            {
                case "Main":
                    NowViewModel = new MainViewModel();
                    break;
                case "Talk":
                    NowViewModel = new TalkViewModel();
                    break;
                default:
                    NowViewModel = new MainViewModel();
                    break;
            }
        }
    }
}
