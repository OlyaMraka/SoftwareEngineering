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

    private async Task SavePasswordAsync()
    {
        string name = PasswordName.Trim();
        string login = PasswordLogin.Trim();
        string password = PasswordValue;

        if (name.Length < 3)
        {
            var errorWindow = new ErrorWindow("Minimum app name length is 3 characters!");
            errorWindow.ShowDialog();
            return;
        }

        if (name.Length > 30)
        {
            var errorWindow = new ErrorWindow("Maximum app name length is 30 characters!");
            errorWindow.ShowDialog();
            return;
        }

        if (login.Length > 50)
        {
            var errorWindow = new ErrorWindow("Maximum login length is 50 characters!");
            errorWindow.ShowDialog();
            return;
        }

        if (password.Length > 30)
        {
            var errorWindow = new ErrorWindow("Maximum password length is 30 characters!");
            errorWindow.ShowDialog();
            return;
        }

        if (currentCategoryId == 0)
        {
            var msg = new MessageWindow("Please select a category for the password!");
            msg.ShowDialog();
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
                    var msg = new MessageWindow("Password successfully updated!");
                    msg.ShowDialog();

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
                    var errorWindow = new ErrorWindow($"Password update error:" +
                                                      $" {string.Join(", ", updateResult.Errors.Select(e => e.Message))}");
                    errorWindow.ShowDialog();
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
                    var msg = new MessageWindow("Password successfully saved!");
                    msg.ShowDialog();

                    refreshPasswordsCallback();

                    PasswordName = string.Empty;
                    PasswordLogin = string.Empty;
                    PasswordValue = string.Empty;
                    IsPasswordVisible = false;
                    SelectedPasswordIcon = "Images/Icons/internet_2.png";
                }
                else
                {
                    var errorWindow = new ErrorWindow($"Password save error:" +
                                                      $" {string.Join(", ", createResult.Errors.Select(e => e.Message))}");
                    errorWindow.ShowDialog();
                }
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception saving password: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when saving password.\n" +
                                              $"Check your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.ShowDialog();
        }
        catch (TaskCanceledException)
        {
            var errorWindow = new ErrorWindow("Timeout exceeded when saving password.\nTry again.");
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation saving password: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation when saving password.\n" +
                                              $"Check the correctness of the entered data.\n\nDetails: {invEx.Message}");
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception saving password: {ex}");
            var errorWindow = new ErrorWindow($"An unexpected error occurred when saving password.\n\n" +
                                              $"Error type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.ShowDialog();
        }
    }

    private async Task DeletePasswordAsync()
    {
        if (currentEditingPassword == null)
        {
            return;
        }

        var confirm = new ConfirmDialog("Are you sure you want to delete this password?");

        if (confirm.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var deleteCommand = new DeletePasswordCommand(currentEditingPassword.Id);
            var deleteResult = await mediator.Send(deleteCommand);

            if (deleteResult.IsSuccess)
            {
                var msg = new MessageWindow("Password successfully deleted!");
                msg.ShowDialog();

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
                var errorWindow = new ErrorWindow($"Password deletion error:" +
                                                  $" {string.Join(", ", deleteResult.Errors.Select(e => e.Message))}");
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            var errorWindow = new ErrorWindow($"Server connection error when deleting password.\n" +
                                              $"Check your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation deleting password: {invEx}");
            var errorWindow = new ErrorWindow($"Failed to delete password.\n" +
                                              $"It may already be deleted or does not exist.\n\nDetails: {invEx.Message}");
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception deleting password: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when deleting password.\n\n" +
                                              $"Error type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.ShowDialog();
        }
    }

    private void ExitPasswordEditMode()
    {
        bool hasChanges = !string.IsNullOrWhiteSpace(PasswordName) ||
                          !string.IsNullOrWhiteSpace(PasswordLogin) ||
                          !string.IsNullOrWhiteSpace(PasswordValue);

        if (hasChanges && currentEditingPassword == null)
        {
            var confirm = new ConfirmDialog("You have unsaved changes. Save them before exiting?");

            if (confirm.ShowDialog() == true)
            {
                _ = SavePasswordAsync();
                return;
            }

            if (confirm.ShowDialog() != true)
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
        var msg = new MessageWindow("Icon selection will be implemented in the future.");
        msg.ShowDialog();
    }
}
