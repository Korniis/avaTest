using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Talk.App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talk.App.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Talk.App
{
    public class ViewLocator : IDataTemplate
    {


        public Control Build(object? data)
        {
            if (data is null)
            {
                return new TextBlock { Text = "data was null" };
            }

            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }

    }
}
