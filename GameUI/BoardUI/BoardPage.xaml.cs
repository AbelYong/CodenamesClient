using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace CodenamesClient.GameUI.BoardUI
{
    /// <summary>
    /// Lógica de interacción para BoardPage.xaml
    /// </summary>
    public partial class BoardPage : Page
    {
        public BoardPage()
        {
            InitializeComponent();
        }

        private void Click_QuitMatch(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
