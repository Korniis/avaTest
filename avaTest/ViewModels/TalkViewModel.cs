
using MsBox.Avalonia;

using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using Talk.Domain;

namespace Talk.App.ViewModels
{
    public class TalkViewModel : ViewModelBase
    {
        public TalkViewModel()
        {
            MyMessage = new TalkContent();
            ReciveMessage = new List<TalkContent>();
            Sendcommand = ReactiveCommand.Create(send);
            MyMessage.Id = 0;
            MyMessage.Sender = System.Guid.NewGuid().ToString();

        }


       
     
        private  TalkContent myMessage;

        public TalkContent MyMessage
        {
            get { return myMessage; }
            set { this.RaiseAndSetIfChanged(ref myMessage, value); }
        }
        private List<TalkContent> reciveMessage;

        public List<TalkContent> ReciveMessage
        {
            get { return reciveMessage; }
            set { this.RaiseAndSetIfChanged(ref reciveMessage, value); }
        }
        public ReactiveCommand<Unit, Unit> Sendcommand { get; }
        private void send()
        {

            var box = MessageBoxManager.GetMessageBoxStandard("MyMessage", MyMessage.Message);
            box.ShowAsync();
           

        }







    }
}
