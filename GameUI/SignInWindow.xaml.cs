using CodenamesClient.GameUI.ViewModels;
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
using System.Windows.Shapes;

namespace CodenamesClient.GameUI
{
    /// <summary>
    /// Lógica de interacción para SignInWindow.xaml
    /// </summary>
    public partial class SignInWindow : Window
    {
        private readonly SignInViewModel _vm = new SignInViewModel();

        public SignInWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        private void Click_SignIn(object sender, RoutedEventArgs e)
        {
            _vm.ValidateAll();
            if (_vm.CanSubmit)
            {
                // TODO: Llamar a la API/servicio de registro
                // Cerrar o navegar
                DialogResult = true;
            }
        }
    }
}
