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

            // Валідація email в реальному часі
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

            // Валідація в реальному часі
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
        // Базова валідація для візуальної індикації
        if (!string.IsNullOrWhiteSpace(username))
        {
            string trimmedUsername = username.Trim();
            if (trimmedUsername.Length >= 3 &&
                System.Text.RegularExpressions.Regex.IsMatch(trimmedUsername, @"^[a-zA-Z][a-zA-Z0-9_.-]*$"))
            {
                // Username виглядає добре - можна додати зелений border
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

    private void PersonalButton_Click(object sender, RoutedEventArgs e)
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

        // TODO: Implement actual personal account registration logic
        string message = $"Реєстрація особистого акаунта успішна!\n\nІм'я: {firstName}\nПрізвище: {lastName}\nEmail: {email}\nІм'я користувача: {username}\nТип акаунта: Особистий";
        MessageBox.Show(message, "Успішна реєстрація", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void TeamButton_Click(object sender, RoutedEventArgs e)
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

        // TODO: Implement actual team account registration logic
        string message = $"Реєстрація командного акаунта успішна!\n\nІм'я: {firstName}\nПрізвище: {lastName}\nEmail: {email}\nІм'я користувача: {username}\nТип акаунта: Командний";
        MessageBox.Show(message, "Успішна реєстрація", MessageBoxButton.OK, MessageBoxImage.Information);
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
        if (firstName.Length < 2)
        {
            errorMessage = "Ім'я повинно містити мінімум 2 символи.";
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
        if (lastName.Length < 2)
        {
            errorMessage = "Прізвище повинно містити мінімум 2 символи.";
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
        string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(email, emailPattern))
        {
            errorMessage = "Введіть правильну email адресу.";
            this.EmailTextBox.Focus();
            return false;
        }

        if (email.Length > 254)
        {
            errorMessage = "Email адреса занадто довга.";
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
        if (username.Length < 3)
        {
            errorMessage = "Ім'я користувача повинно містити мінімум 3 символи.";
            this.UsernameTextBox.Focus();
            return false;
        }

        if (username.Length > 50)
        {
            errorMessage = "Ім'я користувача не може бути довше 50 символів.";
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

        if (password.Length > 128)
        {
            errorMessage = "Пароль не може бути довше 128 символів.";
            this.PasswordTextBox.Focus();
            return false;
        }

        // Перевірка складності password
        bool hasUpper = System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
        bool hasLower = System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]");
        bool hasDigit = System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
        bool hasSpecial = System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]");

        if (!hasUpper)
        {
            errorMessage = "Пароль має містити хоча б одну велику літеру.";
            this.PasswordTextBox.Focus();
            return false;
        }

        if (!hasLower)
        {
            errorMessage = "Пароль має містити хоча б одну малу літеру.";
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
            errorMessage = "Пароль має містити хоча б один спеціальний символ (!@#$%^&*()_+-=[]{}|;:,.<>?).";
            this.PasswordTextBox.Focus();
            return false;
        }

        // Перевірка що password не містить username
        if (password.ToLower().Contains(username.ToLower()))
        {
            errorMessage = "Пароль не може містити ім'я користувача.";
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
