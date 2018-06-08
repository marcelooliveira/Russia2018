using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// O modelo de item de Controle de Usuário está documentado em https://go.microsoft.com/fwlink/?LinkId=234236

namespace Russia2018
{
    public sealed partial class UserControlContainer : UserControl
    {
        List<Page> executingPages = new List<Page>();
        public UserControlContainer()
        {
        }

        public void SwitchToView(Type type, Dictionary<string, object> parameters)
        {
            Page switchToPage = null;
            foreach (Page page in executingPages)
            {
                if (typeof(Page).FullName == type.FullName)
                {
                    switchToPage = page;
                }
            }

            if (switchToPage == null)
            {
                switchToPage = (Page)Activator.CreateInstance(type, new object[] { parameters });
                executingPages.Add(switchToPage);
            }

            //LayoutRoot.Children.Clear();
            //Height = switchToPage.Height;
            //Width = switchToPage.Width;
            //LayoutRoot.Children.Add(switchToPage);
        }

        public void CloseCurrentView()
        {
            executingPages.RemoveAt(executingPages.Count - 1);
            Page previousPage = executingPages[executingPages.Count - 1];
            //LayoutRoot.Children.Clear();
            //Height = previousPage.Height;
            //Width = previousPage.Width;
            //LayoutRoot.Children.Add(previousPage);
        }
    }
}
