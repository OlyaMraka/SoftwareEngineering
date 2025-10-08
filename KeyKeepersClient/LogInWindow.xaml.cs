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

            // Валідація в реальному часі
            this.ValidateUsernameRealTime(textBox.Text);
        }
    }

    private void ValidateUsernameRealTime(string username)
    {
        // Візуальна індикація валідації (можна додати border color зміни)
        // Зараз просто базова перевірка довжини
        if (!string.IsNullOrWhiteSpace(username) && username.Trim().Length >= 3)
        {
            // Username виглядає добре - можна додати зелений border
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
        // Перевірка цілісності даних
        if (!this.ValidateLoginData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string username = this.UsernameTextBox.Text.Trim();
        string password = this.PasswordTextBox.Password;

        // TODO: Implement actual login logic with server communication
        MessageBox.Show($"Login successful for: {username}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private bool ValidateLoginData(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Перевірка наявності username
        if (string.IsNullOrWhiteSpace(this.UsernameTextBox.Text))
        {
            errorMessage = "Username is required.";
            this.UsernameTextBox.Focus();
            return false;
        }

        // Перевірка довжини username
        string username = this.UsernameTextBox.Text.Trim();
        if (username.Length < 3)
        {
            errorMessage = "Username must be at least 3 characters long.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (username.Length > 50)
        {
            errorMessage = "Username cannot be longer than 50 characters.";
            this.UsernameTextBox.Focus();
            return false;
        }

        // Перевірка валідних символів в username
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]+$"))
        {
            errorMessage = "Username can only contain letters, numbers, dots, hyphens, and underscores.";
            this.UsernameTextBox.Focus();
            return false;
        }

        // Перевірка наявності password
        if (string.IsNullOrEmpty(this.PasswordTextBox.Password))
        {
            errorMessage = "Password is required.";
            this.PasswordTextBox.Focus();
            return false;
        }

        // Перевірка довжини password
        if (this.PasswordTextBox.Password.Length < 6)
        {
            errorMessage = "Password must be at least 6 characters long.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (this.PasswordTextBox.Password.Length > 128)
        {
            errorMessage = "Password cannot be longer than 128 characters.";
            this.PasswordTextBox.Focus();
            return false;
        }

        return true;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            var firstWindow = new FirstWindow();
            firstWindow.Left = this.Left;
            firstWindow.Top = this.Top;
            firstWindow.Show();
            this.Close();
        }
    }
}
