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

    private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.FirstNamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.LastNamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }

    private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.EmailPlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;

            this.ValidateEmailRealTime(textBox.Text);
        }
    }

    private void UsernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            this.UsernamePlaceholder.Visibility = string.IsNullOrEmpty(textBox.Text)
                ? Visibility.Visible
                : Visibility.Hidden;

            this.ValidateUsernameRealTime(textBox.Text);
        }
    }

    private void ValidateEmailRealTime(string email)
    {
        if (!string.IsNullOrWhiteSpace(email))
        {
            // Базова перевірка формату email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (System.Text.RegularExpressions.Regex.IsMatch(email.Trim(), emailPattern))
            {
                // Email виглядає правильно
            }
        }
    }

    private void ValidateUsernameRealTime(string username)
    {
        if (!string.IsNullOrWhiteSpace(username))
        {
            string trimmedUsername = username.Trim();
            if (trimmedUsername.Length >= 4 &&
                System.Text.RegularExpressions.Regex.IsMatch(trimmedUsername, @"^[a-zA-Z][a-zA-Z0-9_.-]*$"))
            {
                this.UsernameBorder.BorderBrush = Brushes.Green;
            }
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

    private async void PersonalButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.ValidateRegistrationData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string firstName = this.FirstNameTextBox.Text.Trim();
        string lastName = this.LastNameTextBox.Text.Trim();
        string email = this.EmailTextBox.Text.Trim();
        string username = this.UsernameTextBox.Text.Trim();
        string password = this.PasswordTextBox.Password;

        try
        {
            // Перевірка чи налаштований mediator (база даних та DI)
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Створюємо DTO для реєстрації користувача
            var registerDto = new UserRegisterDto
            {
                Name = firstName,
                Surname = lastName,
                Email = email,
                UserName = username,
                Password = password,
            };

            // Створюємо команду для реєстрації користувача
            var createUserCommand = new CreateUserCommand(registerDto);

            // Виконуємо команду через mediator
            var result = await this.mediator.Send(createUserCommand);

            if (result.IsSuccess)
            {
                string message = $"Реєстрація особистого акаунта успішна!\n\nІм'я: {firstName}\nПрізвище: {lastName}\nEmail: {email}\nІм'я користувача: {username}\nТип акаунта: Особистий\n\nКористувач збережений у базі даних!";
                MessageBox.Show(message, "Успішна реєстрація", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищуємо форму після успішної реєстрації
                this.ClearForm();
            }
            else
            {
                string errorMsg = result.Errors.Any() ? string.Join(", ", result.Errors) : "Помилка при створенні користувача";
                MessageBox.Show($"Помилка реєстрації: {errorMsg}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Виникла помилка при реєстрації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void TeamButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.ValidateRegistrationData(out string errorMessage))
        {
            MessageBox.Show(errorMessage, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string firstName = this.FirstNameTextBox.Text.Trim();
        string lastName = this.LastNameTextBox.Text.Trim();
        string email = this.EmailTextBox.Text.Trim();
        string username = this.UsernameTextBox.Text.Trim();
        string password = this.PasswordTextBox.Password;

        try
        {
            // Перевірка чи налаштований mediator (база даних та DI)
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Створюємо DTO для реєстрації користувача
            var registerDto = new UserRegisterDto
            {
                Name = firstName,
                Surname = lastName,
                Email = email,
                UserName = username,
                Password = password,
            };

            // Створюємо команду для реєстрації користувача
            var createUserCommand = new CreateUserCommand(registerDto);

            // Виконуємо команду через mediator
            var result = await this.mediator.Send(createUserCommand);

            if (result.IsSuccess)
            {
                string message = $"Реєстрація командного акаунта успішна!\n\nІм'я: {firstName}\nПрізвище: {lastName}\nEmail: {email}\nІм'я користувача: {username}\nТип акаунта: Командний\n\nКористувач збережений у базі даних!";
                MessageBox.Show(message, "Успішна реєстрація", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищуємо форму після успішної реєстрації
                this.ClearForm();
            }
            else
            {
                string errorMsg = result.Errors.Any() ? string.Join(", ", result.Errors) : "Помилка при створенні користувача";
                MessageBox.Show($"Помилка реєстрації: {errorMsg}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Виникла помилка при реєстрації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateRegistrationData(out string errorMessage)
    {
        errorMessage = string.Empty;

        // Валідація First Name
        if (string.IsNullOrWhiteSpace(this.FirstNameTextBox.Text))
        {
            errorMessage = "Ім'я є обов'язковим.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        string firstName = this.FirstNameTextBox.Text.Trim();
        if (firstName.Length < 3)
        {
            errorMessage = "Ім'я повинно містити мінімум 3 символи.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        if (firstName.Length > 30)
        {
            errorMessage = "Ім'я не може бути довше 30 символів.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(firstName, @"^[a-zA-ZА-Яа-яІіЇїЄєʼ\s]+$"))
        {
            errorMessage = "Ім'я має містити лише літери.";
            this.FirstNameTextBox.Focus();
            return false;
        }

        // Валідація Last Name
        if (string.IsNullOrWhiteSpace(this.LastNameTextBox.Text))
        {
            errorMessage = "Прізвище є обов'язковим.";
            this.LastNameTextBox.Focus();
            return false;
        }

        string lastName = this.LastNameTextBox.Text.Trim();
        if (lastName.Length < 3)
        {
            errorMessage = "Прізвище повинно містити мінімум 3 символи.";
            this.LastNameTextBox.Focus();
            return false;
        }

        if (lastName.Length > 30)
        {
            errorMessage = "Прізвище не може бути довше 30 символів.";
            this.LastNameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(lastName, @"^[a-zA-ZА-Яа-яІіЇїЄєʼ\s]+$"))
        {
            errorMessage = "Прізвище має містити лише літери.";
            this.LastNameTextBox.Focus();
            return false;
        }

        // Валідація Email
        if (string.IsNullOrWhiteSpace(this.EmailTextBox.Text))
        {
            errorMessage = "Email є обов'язковим.";
            this.EmailTextBox.Focus();
            return false;
        }

        string email = this.EmailTextBox.Text.Trim();
        if (email.Length < 4)
        {
            errorMessage = "Email повинен містити мінімум 4 символи.";
            this.EmailTextBox.Focus();
            return false;
        }

        if (email.Length > 40)
        {
            errorMessage = "Email не може бути довше 40 символів.";
            this.EmailTextBox.Focus();
            return false;
        }

        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
        {
            errorMessage = "Введіть правильну email адресу.";
            this.EmailTextBox.Focus();
            return false;
        }

        // Валідація Username
        if (string.IsNullOrWhiteSpace(this.UsernameTextBox.Text))
        {
            errorMessage = "Ім'я користувача є обов'язковим.";
            this.UsernameTextBox.Focus();
            return false;
        }

        string username = this.UsernameTextBox.Text.Trim();
        if (username.Length < 4)
        {
            errorMessage = "Ім'я користувача повинно містити мінімум 4 символи.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (username.Length > 40)
        {
            errorMessage = "Ім'я користувача не може бути довше 40 символів.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_.-]*$"))
        {
            errorMessage = "Ім'я користувача має починатися з літери і містити лише літери, цифри, _, -, .";
            this.UsernameTextBox.Focus();
            return false;
        }

        // Валідація Password
        if (string.IsNullOrEmpty(this.PasswordTextBox.Password))
        {
            errorMessage = "Пароль є обов'язковим.";
            this.PasswordTextBox.Focus();
            return false;
        }

        string password = this.PasswordTextBox.Password;
        if (password.Length < 8)
        {
            errorMessage = "Пароль повинен містити мінімум 8 символів.";
            this.PasswordTextBox.Focus();
            return false;
        }

        // Перевірка обов'язкових елементів паролю згідно з бекенд валідатором
        bool hasUpper = System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
        bool hasDigit = System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]");
        bool hasSpecial = System.Text.RegularExpressions.Regex.IsMatch(password, @"[^a-zA-Z0-9]");

        if (!hasUpper)
        {
            errorMessage = "Пароль має містити хоча б одну велику літеру.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (!hasDigit)
        {
            errorMessage = "Пароль має містити хоча б одну цифру.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (!hasSpecial)
        {
            errorMessage = "Пароль має містити хоча б один спеціальний символ.";
            this.PasswordTextBox.Focus();
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        // Очищуємо всі поля форми
        this.FirstNameTextBox.Text = string.Empty;
        this.LastNameTextBox.Text = string.Empty;
        this.EmailTextBox.Text = string.Empty;
        this.UsernameTextBox.Text = string.Empty;
        this.PasswordTextBox.Password = string.Empty;

        // Показуємо placeholder'и знову
        this.FirstNamePlaceholder.Visibility = Visibility.Visible;
        this.LastNamePlaceholder.Visibility = Visibility.Visible;
        this.EmailPlaceholder.Visibility = Visibility.Visible;
        this.UsernamePlaceholder.Visibility = Visibility.Visible;
        this.PasswordPlaceholder.Visibility = Visibility.Visible;

        // Встановлюємо фокус на перше поле
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
}
