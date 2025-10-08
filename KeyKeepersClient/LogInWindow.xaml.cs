using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient;

public partial class LogInWindow : Window
{
    public LogInWindow()
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

    private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox textBox)
        {
            this.UsernamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text) ? Visibility.Visible : Visibility.Hidden;
        }
    }

    private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.PasswordBox passwordBox)
        {
            this.PasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password) ? Visibility.Visible : Visibility.Hidden;
        }
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement login logic
        string username = this.UsernameTextBox.Text;
        string password = this.PasswordTextBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Please enter both username and password!");
            return;
        }

        MessageBox.Show($"Login attempt for: {username}");
    }
}
