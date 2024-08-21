using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talk.Domain
{
    [AddINotifyPropertyChangedInterface]

    public class TalkContent
    {
     
        public string Message {  get; set; }

        public string Sender { get; set; }
        public long   Id { get; set; }

        
    }
}
