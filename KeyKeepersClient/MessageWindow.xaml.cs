using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient;

public partial class MessageWindow : Window
{
    public MessageWindow(string message)
    {
        InitializeComponent();
        MessageTextBlock.Text = message;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
        this.Close();
    }
}
