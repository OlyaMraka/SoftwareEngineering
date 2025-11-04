using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using KeyKeepers.BLL.Commands.Users.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient
{
    /// <summary>
    /// Interaction logic for EditUserWindow.xaml.
    /// </summary>
    public partial class EditUserWindow : Window
    {
        private readonly IMediator mediator;
        private readonly long userId;
        private readonly UserResponseDto currentUser;
        private bool isPasswordVisible = false;

        public EditUserWindow(long userId, UserResponseDto user)
        {
            InitializeComponent();
            this.userId = userId;
            this.currentUser = user;
            mediator = App.ServiceProvider.GetRequiredService<IMediator>();

            LoadUserData();
        }

        private void LoadUserData()
        {
            NameTextBox.Text = currentUser.Name;
            SurnameTextBox.Text = currentUser.Surname;
            EmailTextBox.Text = currentUser.Email;
            UsernameTextBox.Text = currentUser.UserName;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateAllFields())
            {
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;
                SaveButton.Content = "Збереження...";

                // If password is empty, use a special marker that backend will recognize
                string password = string.IsNullOrWhiteSpace(PasswordBox.Password)
                    ? string.Empty
                    : PasswordBox.Password;

                var dto = new UpdateUserDto
                {
                    UserId = userId,
                    Name = NameTextBox.Text.Trim(),
                    Surname = SurnameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    UserName = UsernameTextBox.Text.Trim(),
                    Password = password,
                };

                var command = new UpdateUserCommand(dto);
                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    MessageBox.Show(
                        "Профіль успішно оновлено!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    string errorMsg = result.Errors.Any() ? string.Join("\n", result.Errors) : "Невідома помилка";
                    MessageBox.Show(
                        $"Помилка оновлення профілю:\n{errorMsg}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in SaveButton_Click: {ex}");
                MessageBox.Show(
                    $"Виникла помилка при збереженні:\n{ex.Message}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                SaveButton.Content = "Зберегти зміни";
            }
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
                PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ValidatePassword(PasswordBox.Password);
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePassword(PasswordTextBox.Text);
        }

        private bool ValidateAllFields()
        {
            bool isValid = true;

            // Validate Name
            if (!ValidateName(NameTextBox.Text))
            {
                isValid = false;
            }

            // Validate Surname
            if (!ValidateSurname(SurnameTextBox.Text))
            {
                isValid = false;
            }

            // Validate Email
            if (!ValidateEmail(EmailTextBox.Text))
            {
                isValid = false;
            }

            // Validate Username
            if (!ValidateUsername(UsernameTextBox.Text))
            {
                isValid = false;
            }

            // Validate Password (only if user entered something)
            string password = isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            if (!string.IsNullOrWhiteSpace(password))
            {
                if (!ValidatePassword(password))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError(NameErrorText, UserConstants.NameRequiredErrorMessage);
                return false;
            }

            if (name.Length < UserConstants.MinNameLength)
            {
                ShowError(NameErrorText, UserConstants.MinNameLengthErrorMessage);
                return false;
            }

            if (name.Length > UserConstants.MaxNameLength)
            {
                ShowError(NameErrorText, UserConstants.MaxNameLengthErrorMessage);
                return false;
            }

            HideError(NameErrorText);
            return true;
        }

        private bool ValidateSurname(string surname)
        {
            if (string.IsNullOrWhiteSpace(surname))
            {
                ShowError(SurnameErrorText, UserConstants.SurnameRequiredErrorMessage);
                return false;
            }

            if (surname.Length < UserConstants.MinSurnameLength)
            {
                ShowError(SurnameErrorText, UserConstants.MinSurnameLengthErrorMessage);
                return false;
            }

            if (surname.Length > UserConstants.MaxSurnameLength)
            {
                ShowError(SurnameErrorText, UserConstants.MaxSurnameLengthErrorMessage);
                return false;
            }

            HideError(SurnameErrorText);
            return true;
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowError(EmailErrorText, UserConstants.EmailRequiredErrorMessage);
                return false;
            }

            if (email.Length < UserConstants.MinEmailLength)
            {
                ShowError(EmailErrorText, UserConstants.MinEmailLengthErrorMessage);
                return false;
            }

            if (email.Length > UserConstants.MaxEmailLength)
            {
                ShowError(EmailErrorText, UserConstants.MaxEmailLengthErrorMessage);
                return false;
            }

            HideError(EmailErrorText);
            return true;
        }

        private bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError(UsernameErrorText, UserConstants.UserNameRequiredErrorMessage);
                return false;
            }

            if (username.Length < UserConstants.MinUserNameLength)
            {
                ShowError(UsernameErrorText, UserConstants.MinUserNameErrorMessage);
                return false;
            }

            if (username.Length > UserConstants.MaxUserNameLength)
            {
                ShowError(UsernameErrorText, UserConstants.MaxUserNameErrorMessage);
                return false;
            }

            HideError(UsernameErrorText);
            return true;
        }

        private bool ValidatePassword(string password)
        {
            // If password is empty, it's valid (means no change)
            if (string.IsNullOrWhiteSpace(password))
            {
                HideError(PasswordErrorText);
                return true;
            }

            if (password.Length < UserConstants.MinPasswordLength)
            {
                ShowError(PasswordErrorText, UserConstants.PasswordLengthErrorMessage);
                return false;
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                ShowError(PasswordErrorText, UserConstants.PasswordUppercaseLetterErrorMessage);
                return false;
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                ShowError(PasswordErrorText, UserConstants.PasswordDigitErrorMessage);
                return false;
            }

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                ShowError(PasswordErrorText, UserConstants.PasswordSpecialCharacterErrorMessage);
                return false;
            }

            HideError(PasswordErrorText);
            return true;
        }

        private void ShowError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError(TextBlock errorTextBlock)
        {
            errorTextBlock.Text = string.Empty;
            errorTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}
