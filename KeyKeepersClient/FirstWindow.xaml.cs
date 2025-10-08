using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient;

public partial class FirstWindow : Window
{
    public FirstWindow()
    {
        this.InitializeComponent();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        var loginWindow = new LogInWindow();
        loginWindow.Show();
        this.Close();
    }

    private void SignUpButton_Click(object sender, RoutedEventArgs e)
    {
        var signUpWindow = new SignUpWindow();
        signUpWindow.Show();
        this.Close();
    }
}
