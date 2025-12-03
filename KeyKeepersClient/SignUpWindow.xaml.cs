using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class SignUpWindow : Window
{
    private readonly IMediator? mediator;

    public SignUpWindow()
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

    private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.ValidateEmailRealTime(textBox.Text);
        }
    }

    private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.ValidateUsernameRealTime(textBox.Text);
        }
    }

    private void ValidateEmailRealTime(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            KeyKeepersClient.Helpers.TextBoxHelper.SetIsValid(this.EmailTextBox, null);
        }
        else
        {
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            bool isValid = System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), emailPattern);
            KeyKeepersClient.Helpers.TextBoxHelper.SetIsValid(this.EmailTextBox, isValid);
        }
    }

    private void ValidateUsernameRealTime(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            KeyKeepersClient.Helpers.TextBoxHelper.SetIsValid(this.UsernameTextBox, null);
        }
        else
        {
            string trimmedUsername = username.Trim();
            bool isValid = trimmedUsername.Length >= 4 &&
                System.Text.RegularExpressions.Regex.IsMatch(trimmedUsername, @"^[a-zA-Z][a-zA-Z0-9_.-]*$");
            KeyKeepersClient.Helpers.TextBoxHelper.SetIsValid(this.UsernameTextBox, isValid);
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.ValidateRegistrationData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string firstName = this.FirstNameTextBox.Text.Trim();
        string lastName = this.LastNameTextBox.Text.Trim();
        string email = this.EmailTextBox.Text.Trim();
        string username = this.UsernameTextBox.Text.Trim();
        string password = this.PasswordTextBox.Password;

        try
        {
            if (this.mediator == null)
            {
                MessageBox
                    .Show("Database is not configured. Registration is temporarily unavailable.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var registerDto = new UserRegisterDto
            {
                Name = firstName,
                Surname = lastName,
                Email = email,
                UserName = username,
                Password = password,
            };

            var createUserCommand = new CreateUserCommand(registerDto);

            var result = await this.mediator.Send(createUserCommand);

            if (result.IsSuccess)
            {
                var logInWindow = new LogInWindow();
                logInWindow.Left = this.Left;
                logInWindow.Top = this.Top;
                logInWindow.Show();
                this.Close();
            }
            else
            {
                string errorMsg = result.Errors.Any() ? string.Join(", ", result.Errors) : "Error creating user";
                var errorWindow = new ErrorWindow($"Registration error: {errorMsg}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception in Registration: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error during registration.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Registration request timed out");
            var errorWindow = new ErrorWindow($"Server response timeout exceeded.\nTry again later.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid Operation in Registration: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation during registration.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception in Registration: {ex}");
            var errorWindow = new ErrorWindow($"An unexpected error occurred during registration.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}\n\nStack trace: {ex.StackTrace?.Substring(0, Math.Min(200, ex.StackTrace?.Length ?? 0))}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private bool ValidateRegistrationData(out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(this.FirstNameTextBox.Text))
        {
            errorMessage = "Name is required.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        string firstName = this.FirstNameTextBox.Text.Trim();
        if (firstName.Length < 3)
        {
            errorMessage = "Name must contain at least 3 characters.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        if (firstName.Length > 30)
        {
            errorMessage = "Name cannot be longer than 30 characters.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-ZА-Яа-яІіЇїЄєʼ\s]+$"))
        {
            errorMessage = "Name must contain only letters.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.LastNameTextBox.Text))
        {
            errorMessage = "Surname is required.";
            this.LastNameTextBox.Focus();
            return false;
        }

        string lastName = this.LastNameTextBox.Text.Trim();
        if (lastName.Length < 3)
        {
            errorMessage = "Surname must contain at least 3 characters.";
            this.LastNameTextBox.Focus();
            return false;
        }

        if (lastName.Length > 30)
        {
            errorMessage = "Surname cannot be longer than 30 characters.";
            this.LastNameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-ZА-Яа-яІіЇїЄєʼ\s]+$"))
        {
            errorMessage = "Surname must contain only letters.";
            this.LastNameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.EmailTextBox.Text))
        {
            errorMessage = "Email is required.";
            this.EmailTextBox.Focus();
            return false;
        }

        string email = this.EmailTextBox.Text.Trim();
        if (email.Length < 4)
        {
            errorMessage = "Email must contain at least 4 characters.";
            this.EmailTextBox.Focus();
            return false;
        }

        if (email.Length > 40)
        {
            errorMessage = "Email cannot be longer than 40 characters.";
            this.EmailTextBox.Focus();
            return false;
        }

        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
        {
            errorMessage = "Enter a valid email address.";
            this.EmailTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(this.UsernameTextBox.Text))
        {
            errorMessage = "Username is required.";
            this.UsernameTextBox.Focus();
            return false;
        }

        string username = this.UsernameTextBox.Text.Trim();
        if (username.Length < 4)
        {
            errorMessage = "Username must contain at least 4 characters.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (username.Length > 40)
        {
            errorMessage = "Username cannot be longer than 40 characters.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_.-]*$"))
        {
            errorMessage = "Username must start with a letter and contain only letters, numbers, _, -, .";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrEmpty(this.PasswordTextBox.Password))
        {
            errorMessage = "Password is required.";
            this.PasswordTextBox.Focus();
            return false;
        }

        string password = this.PasswordTextBox.Password;
        if (password.Length < 8)
        {
            errorMessage = "Password must contain at least 8 characters.";
            this.PasswordTextBox.Focus();
            return false;
        }

        bool hasUpper = System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
        bool hasDigit = System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]");
        bool hasSpecial = System.Text.RegularExpressions.Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        if (!hasUpper)
        {
            errorMessage = "Password must contain at least one uppercase letter.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (!hasDigit)
        {
            errorMessage = "Password must contain at least one digit.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (!hasSpecial)
        {
            errorMessage = "Password must contain at least one special character.";
            this.PasswordTextBox.Focus();
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        this.FirstNameTextBox.Text = string.Empty;
        this.LastNameTextBox.Text = string.Empty;
        this.EmailTextBox.Text = string.Empty;
        this.UsernameTextBox.Text = string.Empty;
        this.PasswordTextBox.Password = string.Empty;

        // Placeholder visibility now controlled by DataTrigger automatically
        this.FirstNameTextBox.Focus();
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

    private void LogInLink_Click(object sender, RoutedEventArgs e)
    {
        var logInWindow = new LogInWindow();
        logInWindow.Left = this.Left;
        logInWindow.Top = this.Top;
        logInWindow.Show();
        this.Close();
    }
}
