using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient
{
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string message)
        {
            InitializeComponent();
            ErrorMessageTextBlock.Text = message;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
