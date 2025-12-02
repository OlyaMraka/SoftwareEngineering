using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using KeyKeepers.BLL.Commands.Passwords.Create;
using KeyKeepers.BLL.Commands.Passwords.Delete;
using KeyKeepers.BLL.Commands.Passwords.Update;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepersClient.Commands;
using MediatR;

namespace KeyKeepersClient.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IMediator mediator;
    private readonly Action refreshPasswordsCallback;
    private readonly Action<bool> updatePasswordCardsButtonsCallback;

    private bool isPasswordEditPanelVisible;
    private bool isPasswordEditButtonsPanelVisible;
    private string passwordName = string.Empty;
    private string passwordLogin = string.Empty;
    private string passwordValue = string.Empty;
    private bool isPasswordVisible;
    private string selectedPasswordIcon = "Images/Icons/internet_2.png";
    private PasswordData? currentEditingPassword;
    private long currentCategoryId;

    public MainWindowViewModel(
        IMediator mediator,
        Action refreshPasswordsCallback,
        Action<bool> updatePasswordCardsButtonsCallback)
    {
        this.mediator = mediator;
        this.refreshPasswordsCallback = refreshPasswordsCallback;
        this.updatePasswordCardsButtonsCallback = updatePasswordCardsButtonsCallback;

        SavePasswordCommand = new RelayCommand(async _ => await SavePasswordAsync(), _ => CanSavePassword());
        DeletePasswordCommand = new RelayCommand(async _ => await DeletePasswordAsync(), _ => CanDeletePassword());
        ExitPasswordEditModeCommand = new RelayCommand(_ => ExitPasswordEditMode());
        TogglePasswordVisibilityCommand = new RelayCommand(_ => TogglePasswordVisibility());
        SelectPasswordIconCommand = new RelayCommand(_ => SelectPasswordIcon());
    }

    public ICommand SavePasswordCommand { get; }

    public ICommand DeletePasswordCommand { get; }

    public ICommand ExitPasswordEditModeCommand { get; }

    public ICommand TogglePasswordVisibilityCommand { get; }

    public ICommand SelectPasswordIconCommand { get; }

    public bool IsPasswordEditPanelVisible
    {
        get => isPasswordEditPanelVisible;
        set => SetProperty(ref isPasswordEditPanelVisible, value);
    }

    public bool IsPasswordEditButtonsPanelVisible
    {
        get => isPasswordEditButtonsPanelVisible;
        set => SetProperty(ref isPasswordEditButtonsPanelVisible, value);
    }

    public string PasswordName
    {
        get => passwordName;
        set
        {
            if (SetProperty(ref passwordName, value))
            {
                ((RelayCommand)SavePasswordCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string PasswordLogin
    {
        get => passwordLogin;
        set
        {
            if (SetProperty(ref passwordLogin, value))
            {
                ((RelayCommand)SavePasswordCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string PasswordValue
    {
        get => passwordValue;
        set
        {
            if (SetProperty(ref passwordValue, value))
            {
                ((RelayCommand)SavePasswordCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsPasswordVisible
    {
        get => isPasswordVisible;
        set => SetProperty(ref isPasswordVisible, value);
    }

    public string SelectedPasswordIcon
    {
        get => selectedPasswordIcon;
        set => SetProperty(ref selectedPasswordIcon, value);
    }

    public void OpenAddPasswordMode(long categoryId)
    {
        currentCategoryId = categoryId;
        currentEditingPassword = null;

        IsPasswordEditPanelVisible = true;
        IsPasswordEditButtonsPanelVisible = true;

        PasswordName = string.Empty;
        PasswordLogin = string.Empty;
        PasswordValue = string.Empty;
        IsPasswordVisible = false;
        SelectedPasswordIcon = "Images/Icons/internet_2.png";

        updatePasswordCardsButtonsCallback(true);
    }

    public void OpenEditPasswordMode(PasswordData passwordData, long categoryId)
    {
        currentCategoryId = categoryId;
        currentEditingPassword = passwordData;

        IsPasswordEditPanelVisible = true;
        IsPasswordEditButtonsPanelVisible = true;

        PasswordName = passwordData.Name;
        PasswordLogin = passwordData.Login;
        PasswordValue = passwordData.Password;
        IsPasswordVisible = false;
        SelectedPasswordIcon = passwordData.IconPath;

        updatePasswordCardsButtonsCallback(true);
    }

    private bool CanSavePassword()
    {
        return !string.IsNullOrWhiteSpace(PasswordName) &&
               !string.IsNullOrWhiteSpace(PasswordLogin) &&
               !string.IsNullOrWhiteSpace(PasswordValue);
    }

    private bool CanDeletePassword()
    {
        return currentEditingPassword != null;
    }

    private async System.Threading.Tasks.Task SavePasswordAsync()
    {
        string name = PasswordName.Trim();
        string login = PasswordLogin.Trim();
        string password = PasswordValue;

        if (name.Length < 3)
        {
            MessageBox.Show(
                "Мінімальна довжина назви додатку 3 символів!",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (name.Length > 30)
        {
            MessageBox.Show(
                "Максимальна довжина назви додатку 30 символів!",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (login.Length > 50)
        {
            MessageBox.Show(
                "Максимальна довжина логіну 50 символів!",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (password.Length > 30)
        {
            MessageBox.Show(
                "Максимальна довжина паролю 30 символів!",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (currentCategoryId == 0)
        {
            MessageBox.Show(
                "Будь ласка, оберіть категорію для паролю!",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            if (currentEditingPassword != null)
            {
                var updateRequest = new UpdatePasswordRequest
                {
                    Id = currentEditingPassword.Id,
                    AppName = name,
                    Login = login,
                    Password = password,
                    LogoUrl = SelectedPasswordIcon,
                    CategoryId = currentCategoryId,
                };

                var updateCommand = new UpdatePasswordCommand(updateRequest);
                var updateResult = await mediator.Send(updateCommand);

                if (updateResult.IsSuccess)
                {
                    MessageBox.Show(
                        "Пароль успішно оновлено!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    refreshPasswordsCallback();

                    currentEditingPassword = null;
                    PasswordName = string.Empty;
                    PasswordLogin = string.Empty;
                    PasswordValue = string.Empty;
                    IsPasswordVisible = false;
                    SelectedPasswordIcon = "Images/Icons/internet_2.png";
                    updatePasswordCardsButtonsCallback(true);
                }
                else
                {
                    MessageBox.Show(
                        $"Помилка оновлення паролю: {string.Join(", ", updateResult.Errors.Select(e => e.Message))}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            else
            {
                var createRequest = new CreatePasswordRequest
                {
                    AppName = name,
                    Login = login,
                    Password = password,
                    LogoUrl = SelectedPasswordIcon,
                    CategoryId = currentCategoryId,
                };

                var createCommand = new CreatePasswordCommand(createRequest);
                var createResult = await mediator.Send(createCommand);

                if (createResult.IsSuccess)
                {
                    MessageBox.Show(
                        "Пароль успішно збережено!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    refreshPasswordsCallback();

                    // Очищаємо поля для додавання наступного паролю
                    PasswordName = string.Empty;
                    PasswordLogin = string.Empty;
                    PasswordValue = string.Empty;
                    IsPasswordVisible = false;
                    SelectedPasswordIcon = "Images/Icons/internet_2.png";
                }
                else
                {
                    MessageBox.Show(
                        $"Помилка збереження паролю: {string.Join(", ", createResult.Errors.Select(e => e.Message))}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception saving password: {httpEx}");
            MessageBox.Show(
                $"Помилка з'єднання з сервером при збереженні пароля.\nПеревірте підключення до інтернету.\n\nДеталі: {httpEx.Message}",
                "Помилка з'єднання",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Saving password timed out");
            MessageBox.Show(
                "Перевищено час очікування при збереженні пароля.\nСпробуйте ще раз.",
                "Тайм-аут",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation saving password: {invEx}");
            MessageBox.Show(
                $"Некоректна операція при збереженні пароля.\nПеревірте коректність введених даних.\n\nДеталі: {invEx.Message}",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception saving password: {ex}");
            MessageBox.Show(
                $"Виникла непередбачена помилка при збереженні пароля.\n\nТип помилки: {ex.GetType().Name}\nПовідомлення: {ex.Message}",
                "Критична помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async System.Threading.Tasks.Task DeletePasswordAsync()
    {
        if (currentEditingPassword == null)
        {
            return;
        }

        var result = MessageBox.Show(
            "Ви впевнені, що хочете видалити цей пароль?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var deleteCommand = new DeletePasswordCommand(currentEditingPassword.Id);
            var deleteResult = await mediator.Send(deleteCommand);

            if (deleteResult.IsSuccess)
            {
                MessageBox.Show(
                    "Пароль успішно видалено!",
                    "Успіх",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                refreshPasswordsCallback();

                // Очищаємо поля після видалення
                currentEditingPassword = null;
                PasswordName = string.Empty;
                PasswordLogin = string.Empty;
                PasswordValue = string.Empty;
                IsPasswordVisible = false;
                SelectedPasswordIcon = "Images/Icons/internet_2.png";
                updatePasswordCardsButtonsCallback(true);
            }
            else
            {
                MessageBox.Show(
                    $"Помилка видалення паролю: {string.Join(", ", deleteResult.Errors.Select(e => e.Message))}",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception deleting password: {httpEx}");
            MessageBox.Show(
                $"Помилка з'єднання з сервером при видаленні пароля.\nПеревірте підключення до інтернету.\n\nДеталі: {httpEx.Message}",
                "Помилка з'єднання",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation deleting password: {invEx}");
            MessageBox.Show(
                $"Не вдалося видалити пароль.\nМожливо, він вже видалений або не існує.\n\nДеталі: {invEx.Message}",
                "Помилка операції",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception deleting password: {ex}");
            MessageBox.Show(
                $"Виникла помилка при видаленні пароля.\n\nТип помилки: {ex.GetType().Name}\nПовідомлення: {ex.Message}",
                "Критична помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ExitPasswordEditMode()
    {
        bool hasChanges = !string.IsNullOrWhiteSpace(PasswordName) ||
                          !string.IsNullOrWhiteSpace(PasswordLogin) ||
                          !string.IsNullOrWhiteSpace(PasswordValue);

        if (hasChanges && currentEditingPassword == null)
        {
            var result = MessageBox.Show(
                "У вас є незбережені зміни. Зберегти їх перед виходом?",
                "Незбережені зміни",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _ = SavePasswordAsync();
                return;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        IsPasswordEditPanelVisible = false;
        IsPasswordEditButtonsPanelVisible = false;

        PasswordName = string.Empty;
        PasswordLogin = string.Empty;
        PasswordValue = string.Empty;
        IsPasswordVisible = false;
        SelectedPasswordIcon = "Images/Icons/internet_2.png";

        currentEditingPassword = null;
        updatePasswordCardsButtonsCallback(false);
    }

    private void TogglePasswordVisibility()
    {
        IsPasswordVisible = !IsPasswordVisible;
    }

    private void SelectPasswordIcon()
    {
        MessageBox.Show(
            "Вибір іконки буде реалізовано в майбутньому.",
            "Вибір іконки",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
