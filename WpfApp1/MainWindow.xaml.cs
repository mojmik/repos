using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 




    public static class ExtensionMethods

    {
        private static Action EmptyDelegate = delegate () { };


        public static void Refresh(this UIElement uiElement)

        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
    }

    public partial class MainWindow : Window

    {
        public MainWindow()
        {
            
            InitializeComponent();
           
            

            
          
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int n = 0;
            for (int v=0; v<5 ; v++ )
            {
                //this.UpdateLayout();
                n = n + 1;
                mText.Text = "ahoj " + n;
                mText.Refresh();
                Thread.Sleep(50);
            }
        }
    }
}
