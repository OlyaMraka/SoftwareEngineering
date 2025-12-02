using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.Users.LogIn;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class LogInWindow : Window
{
    private readonly IMediator? mediator;

    public LogInWindow()
    {
        this.InitializeComponent();
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();
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
            this.ValidateUsernameRealTime(textBox.Text);
        }
    }

    private void ValidateUsernameRealTime(string username)
    {
        if (!string.IsNullOrWhiteSpace(username) && username.Trim().Length >= 4)
        {
            this.UsernameBorder.BorderBrush = Brushes.Green;
        }
    }

    private async void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.ValidateLoginData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string username = this.UsernameTextBox.Text.Trim();
        string password = this.PasswordTextBox.Password;

        try
        {
            var logInDto = new UserLogInDto
            {
                Username = username,
                Password = password,
            };

            var command = new UserLogInCommand(logInDto);
            var result = await mediator!.Send(command);

            if (result.IsSuccess)
            {
                var mainWindow = new MainWindow(result.Value.Id);
                mainWindow.Left = this.Left;
                mainWindow.Top = this.Top;
                mainWindow.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Invalid username or password. Please try again.",
                    "Login Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception in Login: {httpEx}");
            MessageBox.Show(
                $"Server connection error.\nCheck your internet connection.\n\nDetails: {httpEx.Message}",
                "Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Login request timed out");
            MessageBox.Show(
                "Server response timeout exceeded.\nTry again later.",
                "Timeout",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception in Login: {ex}");
            MessageBox.Show(
                $"An unexpected error occurred during login.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private bool ValidateLoginData(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(this.UsernameTextBox.Text))
        {
            errorMessage = "Username is required.";
            this.UsernameTextBox.Focus();
            return false;
        }

        string username = this.UsernameTextBox.Text.Trim();
        if (username.Length < 4)
        {
            errorMessage = "Username must be at least 4 characters long.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (username.Length > 40)
        {
            errorMessage = "Username cannot be longer than 40 characters.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_.-]+$"))
        {
            errorMessage = "Username can only contain letters, numbers, dots, hyphens, and underscores.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrEmpty(this.PasswordTextBox.Password))
        {
            errorMessage = "Password is required.";
            this.PasswordTextBox.Focus();
            return false;
        }

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

    private void SignUpLink_Click(object sender, RoutedEventArgs e)
    {
        var signUpWindow = new SignUpWindow();
        signUpWindow.Left = this.Left;
        signUpWindow.Top = this.Top;
        signUpWindow.Show();
        this.Close();
    }
}
