using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KeyKeepersClient;

public partial class SignUpWindow : Window
{
    public SignUpWindow()
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

    private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.UsernamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        var passwordBox = sender as PasswordBox;
        if (passwordBox != null)
        {
            this.PasswordPlaceholder.Visibility = string.IsNullOrEmpty(passwordBox.Password)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private void PersonalButton_Click(object sender, RoutedEventArgs e)
    {
        string username = this.UsernameTextBox.Text;
        string password = this.PasswordTextBox.Password;

        string message = $"Personal account registration:\nUsername: {username}\nPassword: {password}";
        MessageBox.Show(message, "Personal Registration");
    }

    private void TeamButton_Click(object sender, RoutedEventArgs e)
    {
        string username = this.UsernameTextBox.Text;
        string password = this.PasswordTextBox.Password;

        string message = $"Team account registration:\nUsername: {username}\nPassword: {password}";
        MessageBox.Show(message, "Team Registration");
    }
}
