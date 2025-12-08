using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using KeyKeepers.BLL.Commands.Communities.Create;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;
using KeyKeepers.BLL.Commands.PasswordCategory.Delete;
using KeyKeepers.BLL.Commands.PasswordCategory.Update;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Queries.CommunityUsers.GetByUserId;
using KeyKeepers.BLL.Queries.JoinRequests.GetByRecipientId;
using KeyKeepers.BLL.Queries.PasswordCategories.GetAll;
using KeyKeepers.BLL.Queries.Passwords.GetAllById;
using KeyKeepers.BLL.Queries.Users.GetById;
using KeyKeepers.DAL.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using KeyKeepersClient.ViewModels;

namespace KeyKeepersClient;

public partial class MainWindow : Window
{
    private readonly IMediator mediator;
    private readonly long userId;
    private readonly MainWindowViewModel viewModel;
    private Button? currentActiveButton;
    private Button? currentActiveCommunityButton;
    private ObservableCollection<CategoryItem> customCategories;
    private ObservableCollection<CommunityItem> communities;
    private CommunityItem? currentCommunity = null;
    private bool isEditMode = false;
    private CategoryItem? currentEditingCategory = null;
    private long currentCategoryId = 0;
    private bool currentCategoryHasPasswords = false;

    private Border? currentEditingPasswordCard = null;
    private PasswordData? currentEditingPassword = null;
    private DispatcherTimer? invitationCheckTimer;

    public MainWindow(long userId)
    {
        InitializeComponent();
        this.userId = userId;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();
        customCategories = new ObservableCollection<CategoryItem>();
        communities = new ObservableCollection<CommunityItem>();

        viewModel = new MainWindowViewModel(
            mediator,
            async () => await LoadPasswordsForCategory(currentCategoryId),
            UpdatePasswordCardsButtons);
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        DataContext = viewModel;

        this.Loaded += MainWindow_Loaded;

        currentActiveButton = (Button)CategoriesPanel.Children[0];
        currentActiveButton.Style = (Style)FindResource("ActiveCategoryButtonStyle");
        SetActiveCommunity(PrivateCommunityButton);
    }

    public void OpenAddPasswordMode()
    {
        if (!CanEditCurrentCommunity())
        {
            var msg = new MessageWindow("You don't have permission to add passwords in this community.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (isEditMode)
        {
            ExitEditMode();
        }

        CategoryEditPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        AddPasswordButton.Visibility = Visibility.Collapsed;
        viewModel.OpenAddPasswordMode(currentCategoryId);

        if (PasswordsScrollViewer != null)
        {
            PasswordsScrollViewer.ScrollToTop();
        }
    }

    public void OpenAddCategoryMode()
    {
        if (!CanEditCurrentCommunity())
        {
            var msg = new MessageWindow("You don't have permission to add categories in this community.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (viewModel.IsPasswordEditPanelVisible)
        {
            viewModel.ExitPasswordEditModeCommand.Execute(null);
        }

        isEditMode = true;
        currentEditingCategory = null;
        CategoryNameTextBox.Text = string.Empty;
        CategoryEditPanel.Visibility = Visibility.Visible;
        EditButtonsPanel.Visibility = Visibility.Visible;
        AddCategoryButton.Visibility = Visibility.Collapsed;
        UpdateAllCategoryButtonsVisibility();
        CategoryNameTextBox.Focus();
    }

    public async void OpenAddCommunityMode()
    {
        var addCommunityWindow = new AddCommunityWindow
        {
            Owner = this,
        };

        if (addCommunityWindow.ShowDialog() == true)
        {
            var communityName = addCommunityWindow.CommunityName;

            if (!string.IsNullOrWhiteSpace(communityName))
            {
                await CreateCommunityAsync(communityName);
            }
        }
    }

    public async void OpenEditUserMode()
    {
        try
        {
            var query = new GetUserByIdQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                var editUserWindow = new EditUserWindow(userId, result.Value)
                {
                    Owner = this,
                };

                if (editUserWindow.ShowDialog() == true)
                {
                    await LoadCurrentUser();
                }
            }
            else
            {
                var errorWindow = new ErrorWindow($"Failed to load user data");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in OpenEditUserMode: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    public async void LogOutButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string refreshToken = GetStoredRefreshToken();

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var logOutDto = new UserLogOutDto
                {
                    RefreshToken = refreshToken,
                };

                var command = new UserLogOutCommand(logOutDto);
                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    ClearStoredTokens();

                    var firstWindow = new FirstWindow();
                    firstWindow.Left = this.Left;
                    firstWindow.Top = this.Top;
                    firstWindow.Show();
                    this.Close();
                }
                else
                {
                    var errorWindow = new ErrorWindow($"Error during logout. Please try again.");
                    errorWindow.Owner = this;
                    errorWindow.ShowDialog();
                }
            }
            else
            {
                var firstWindow = new FirstWindow();
                firstWindow.Left = this.Left;
                firstWindow.Top = this.Top;
                firstWindow.Show();
                this.Close();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception during logout: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error during logout.\\nYou will be logged out locally.\\n\\nDetails: \" + httpEx.Message,");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();

            ClearStoredTokens();
            var firstWindow = new FirstWindow();
            firstWindow.Left = this.Left;
            firstWindow.Top = this.Top;
            firstWindow.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception during logout: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred during logout.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.IsPasswordEditPanelVisible))
        {
            UpdateEditButtonsVisibility();
        }
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCommunitiesAsync();
        await LoadCategoriesAsync();

        await LoadPasswordsAsync(0, isSpecialCategory: true);
        await LoadCurrentUser();

        StartInvitationCheckTimer();

        await CheckForInvitationsAsync();
    }

    private async Task LoadCurrentUser()
    {
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured. Registration is temporarily unavailable.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var query = new GetUserByIdQuery(userId);

            var result = await this.mediator.Send(query);

            if (result.IsSuccess)
            {
                var userDto = result.Value;

                UserContainer.Children.Clear();

                UIElement userProfilePanel = CreateUserProfilePanel(userDto.Name, userDto.Surname, userDto.Email);
                UserContainer.Children.Add(userProfilePanel);
            }
            else
            {
                var errorWindow = new ErrorWindow($"Error loading user data");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            var errorWindow = new ErrorWindow($"Error loading categories: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private UIElement CreateUserProfilePanel(string firstName, string lastName, string email)
    {
        var button = new Button
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        };

        button.Click += UserProfileButton_Click;

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 0, 0),
        };

        var icon = new Border
        {
            Width = 60,
            Height = 60,
            CornerRadius = new CornerRadius(30),
            Background = CreateSoftGreenGradientBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Child = new TextBlock
            {
                Text = $"{(firstName.Length > 0 ? firstName[0].ToString() : string.Empty)}{(lastName.Length > 0 ? lastName[0].ToString() : string.Empty)}".ToUpper(),
                Foreground = Brushes.White,
                FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        panel.Children.Add(icon);

        var nameBlock = new TextBlock
        {
            Text = $"{firstName} {lastName}",
            Foreground = Brushes.White,
            FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 5),
        };
        panel.Children.Add(nameBlock);

        var emailBlock = new TextBlock
        {
            Text = email,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 15),
        };
        panel.Children.Add(emailBlock);

        button.Content = panel;

        var container = new StackPanel
        {
            Orientation = Orientation.Vertical,
        };

        container.Children.Add(button);

        var separator = new Border
        {
            Height = 1,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39")),
            Margin = new Thickness(0, 0, 0, 20),
        };
        container.Children.Add(separator);

        return container;
    }

    private void UserProfileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenEditUserMode();
    }

    private LinearGradientBrush CreateSoftGreenGradientBrush()
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(0, 1),
        };

        brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF81C784"), 0.0));
        brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF4CAF50"), 0.5));
        brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF1B5E20"), 1.0));

        return brush;
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured. Registration is temporarily unavailable.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var query = new GetAllPasswordCategoriesQuery(userId);

            var result = await mediator.Send(query);

            foreach (var category in result.Value)
            {
                var categoryItem = new CategoryItem
                {
                    Id = category.Id,
                    Name = category.Name,
                };

                customCategories.Add(categoryItem);
                var button = CreateCategoryButton(categoryItem);
                CategoriesPanel.Children.Add(button);
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception loading categories: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when loading categories.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Loading categories timed out");
            var errorWindow = new ErrorWindow($"Timeout exceeded when loading categories.\nTry refreshing the page.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception loading categories: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when loading categories.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task LoadPasswordsAsync(long categoryId, bool isSpecialCategory = false)
    {
        try
        {
            if (mediator == null)
            {
                return;
            }

            PasswordsPanel.Children.Clear();

            var query = new GetCredentialsByIdQuery(categoryId);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                int passwordCount = result.Value.Count();
                currentCategoryHasPasswords = passwordCount > 0;

                if (passwordCount == 0)
                {
                    // Don't show empty state for special categories (All items, Favorite)
                    if (isSpecialCategory)
                    {
                        EmptyStatePanel.Visibility = Visibility.Collapsed;
                        PasswordsPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        EmptyStatePanel.Visibility = Visibility.Visible;
                        PasswordsPanel.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    EmptyStatePanel.Visibility = Visibility.Collapsed;
                    PasswordsPanel.Visibility = Visibility.Visible;

                    foreach (var password in result.Value)
                    {
                        string strength = CalculatePasswordStrength(password.Password);
                        CreatePasswordCard(
                            password.Id,
                            password.AppName,
                            password.Login,
                            password.Password,
                            password.LogoUrl ?? "Images/Icons/internet_2.png",
                            strength,
                            password.CategoryId);
                    }
                }

                UpdateAddPasswordButtonVisibility();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception loading passwords: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when loading passwords.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Loading passwords timed out");
            var errorWindow = new ErrorWindow($"Timeout exceeded when loading passwords.\nTry refreshing the page.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception loading passwords: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when loading passwords.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task LoadPasswordsForCategory(long categoryId)
    {
        await LoadPasswordsAsync(categoryId);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void AddPasswordFromEmptyStateButton_Click(object sender, RoutedEventArgs e)
    {
        OpenAddPasswordMode();
    }

    private void AddPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        OpenAddPasswordMode();
    }

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (isEditMode)
        {
            var msg = new MessageWindow("Please finish editing the category first.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (viewModel.IsPasswordEditPanelVisible)
        {
            var msg = new MessageWindow("Please finish editing the password first.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (sender is Button clickedButton)
        {
            SetActiveCategory(clickedButton);

            if (clickedButton.Tag is CategoryItem category)
            {
                currentCategoryId = category.Id;
                _ = LoadPasswordsAsync(category.Id, isSpecialCategory: false);
            }
            else
            {
                bool isSpecialCategory = clickedButton.Tag is string tag &&
                                         (tag == "AllItems" || tag == "Favorite");
                currentCategoryId = 0;
                _ = LoadPasswordsAsync(0, isSpecialCategory: isSpecialCategory);
            }
        }
    }

    private void SetActiveCategory(Button button)
    {
        if (currentActiveButton != null)
        {
            currentActiveButton.Style = (Style)FindResource("CategoryButtonStyle");
        }

        currentActiveButton = button;
        button.Style = (Style)FindResource("ActiveCategoryButtonStyle");
    }

    private void SetActiveCommunity(Button button)
    {
        if (currentActiveCommunityButton != null)
        {
            currentActiveCommunityButton.Style = (Style)FindResource("CommunityButtonStyle");
        }

        currentActiveCommunityButton = button;
        button.Style = (Style)FindResource("ActiveCommunityButtonStyle");
    }

    private bool CanEditCurrentCommunity()
    {
        if (currentCommunity == null)
        {
            return true; // Private community
        }

        return currentCommunity.UserRole == CommunityRole.Owner ||
               currentCommunity.UserRole == CommunityRole.Admin;
    }

    private void UpdateEditButtonsVisibility()
    {
        bool canEdit = CanEditCurrentCommunity();
        AddCategoryButton.Visibility = (canEdit && !isEditMode && !viewModel.IsPasswordEditPanelVisible) ? Visibility.Visible : Visibility.Collapsed;

        if (viewModel.IsPasswordEditPanelVisible || isEditMode)
        {
            AdminPanelButton.Visibility = Visibility.Collapsed;
        }
        else if (currentCommunity != null &&
                 (currentCommunity.UserRole == CommunityRole.Owner || currentCommunity.UserRole == CommunityRole.Admin))
        {
            AdminPanelButton.Visibility = Visibility.Visible;
        }

        UpdateAddPasswordButtonVisibility();
    }

    private void UpdateAddPasswordButtonVisibility()
    {
        bool canEdit = CanEditCurrentCommunity();
        bool hasPasswords = currentCategoryHasPasswords;
        bool isSpecialCategory = currentCategoryId == 0 || IsCurrentCategorySpecial();

        AddPasswordButton.Visibility = (canEdit && hasPasswords && !isSpecialCategory && !isEditMode && !viewModel.IsPasswordEditPanelVisible)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private bool IsCurrentCategorySpecial()
    {
        if (currentActiveButton != null && currentActiveButton.Tag is string tag)
        {
            return tag == "AllItems" || tag == "Favorite";
        }

        return false;
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow(this);
        settingsWindow.ShowDialog();
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        OpenAddCategoryMode();
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox.Text.ToLower();
    }

    private void CategoryNameTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            if (e.Delta > 0)
            {
                textBox.ScrollToHorizontalOffset(textBox.HorizontalOffset - 20);
            }
            else
            {
                textBox.ScrollToHorizontalOffset(textBox.HorizontalOffset + 20);
            }

            e.Handled = true;
        }
    }

    private async void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isEditMode)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
        {
            if (currentEditingCategory == null)
            {
                await AddCustomCategoryAsync(CategoryNameTextBox.Text.Trim());
            }
            else
            {
                await UpdateCategoryAsync(currentEditingCategory, CategoryNameTextBox.Text.Trim());
            }

            CategoryNameTextBox.Text = string.Empty;
            currentEditingCategory = null;
            CategoryNameTextBox.Focus();
        }
        else
        {
            var msg = new MessageWindow("Please enter a category name.");
            msg.Owner = this;
            msg.ShowDialog();
        }
    }

    private async void SaveEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isEditMode)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
        {
            if (currentEditingCategory == null)
            {
                await AddCustomCategoryAsync(CategoryNameTextBox.Text.Trim());
            }
            else
            {
                await UpdateCategoryAsync(currentEditingCategory, CategoryNameTextBox.Text.Trim());
            }
        }

        ExitEditMode();
    }

    private void ExitEditMode()
    {
        isEditMode = false;
        currentEditingCategory = null;
        CategoryNameTextBox.Text = string.Empty;
        CategoryEditPanel.Visibility = Visibility.Collapsed;
        EditButtonsPanel.Visibility = Visibility.Collapsed;
        UpdateEditButtonsVisibility();
        UpdateAllCategoryButtonsVisibility();
    }

    private void UpdateAllCategoryButtonsVisibility()
    {
        foreach (var child in CategoriesPanel.Children)
        {
            if (child is Button btn && btn.Content is Grid grid)
            {
                if (grid.Children.Count >= 3 && grid.Children[2] is StackPanel buttonPanel)
                {
                    buttonPanel.Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }

    private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanEditCurrentCommunity())
        {
            var msg = new MessageWindow("You don't have permission to edit categories in this community.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (sender is Button editButton && editButton.Tag is CategoryItem category)
        {
            isEditMode = true;
            currentEditingCategory = category;
            CategoryNameTextBox.Text = category.Name;
            CategoryEditPanel.Visibility = Visibility.Visible;
            EditButtonsPanel.Visibility = Visibility.Visible;
            AddCategoryButton.Visibility = Visibility.Collapsed;
            CategoryNameTextBox.Focus();
            CategoryNameTextBox.SelectAll();
        }
    }

    private async Task UpdateCategoryAsync(CategoryItem category, string newName)
    {
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured. Registration is temporarily unavailable.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var dto = new UpdatePrivateCategoryDto()
            {
                UserId = userId,
                Id = category.Id,
                Name = newName,
            };

            var command = new UpdatePrivateCategoryCommand(dto);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                category.Name = newName;
                UpdateCategoryButton(category);
            }
            else
            {
                var errorWindow = new ErrorWindow($"Error");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception updating category: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when updating category.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation updating category: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation when updating category.\nCategory may no longer exist.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception updating category: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when updating category '{category.Name}'.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private void UpdateCategoryButton(CategoryItem category)
    {
        foreach (var child in CategoriesPanel.Children)
        {
            if (child is Button btn && btn.Tag is CategoryItem tag && tag.Id == category.Id)
            {
                var index = CategoriesPanel.Children.IndexOf(btn);
                CategoriesPanel.Children.Remove(btn);
                var newButton = CreateCategoryButton(category);
                CategoriesPanel.Children.Insert(index, newButton);
                break;
            }
        }
    }

    private async Task DeleteCategoryAsync(CategoryItem category)
    {
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured. Registration is temporarily unavailable.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var command = new DeletePrivateCategoryCommand(category.Id);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                Button? buttonToRemove = null;
                foreach (var child in CategoriesPanel.Children)
                {
                    if (child is Button btn && btn.Tag is CategoryItem tag && tag.Id == category.Id)
                    {
                        buttonToRemove = btn;
                        break;
                    }
                }

                if (buttonToRemove != null)
                {
                    if (buttonToRemove == currentActiveButton)
                    {
                        SetActiveCategory((Button)CategoriesPanel.Children[0]);
                    }

                    CategoriesPanel.Children.Remove(buttonToRemove);
                    customCategories.Remove(category);
                }
            }
            else
            {
                var errorWindow = new ErrorWindow($"Category not found.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception deleting category: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when deleting category.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation deleting category: {invEx}");
            var errorWindow = new ErrorWindow($"Failed to delete category.\nIt may contain saved passwords.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception deleting category: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when deleting category '{category.Name}'.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task AddCustomCategoryAsync(string categoryName)
    {
        string categoryNamee = CategoryNameTextBox.Text.Trim();
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured. Registration is temporarily unavailable.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            var dto = new CreatePrivateCategoryDto()
            {
                UserId = userId,
                Name = categoryNamee,
            };

            var command = new CreatePrivateCategoryCommand(dto);

            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                var categoryItem = new CategoryItem
                {
                    Id = result.Value.Id,
                    Name = result.Value.Name,
                };

                customCategories.Add(categoryItem);

                var button = CreateCategoryButton(categoryItem);
                CategoriesPanel.Children.Add(button);
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
            System.Diagnostics.Debug.WriteLine($"HTTP Exception creating category: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when creating category.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (TaskCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("Creating category timed out");
            var errorWindow = new ErrorWindow($"Timeout exceeded when creating category.\nPlease try again.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation creating category: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation when creating category.\nCategory with this name may already exist.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception creating category: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when creating category '{categoryNamee}'.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task CreateCommunityAsync(string communityName)
    {
        try
        {
            if (mediator == null)
            {
                var errorWindow = new ErrorWindow("Database is not configured.");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                return;
            }

            if (string.IsNullOrWhiteSpace(communityName))
            {
                var msg = new MessageWindow("Please enter community name.");
                msg.Owner = this;
                msg.ShowDialog();

                return;
            }

            var dto = new CreateCommunityDto
            {
                OwnerId = userId,
                Name = communityName.Trim(),
            };

            var command = new CreateCommunityCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                var communityItem = new CommunityItem
                {
                    CommunityId = result.Value.NewCommunity.Id,
                    CommunityUserId = result.Value.Owner.Id,
                    Name = result.Value.NewCommunity.Name,
                };

                communities.Add(communityItem);

                var button = CreateCommunityButton(communityItem);
                CommunitiesPanel.Children.Add(button);

                var msg = new MessageWindow($"Community '{communityName}' created successfully!");
                msg.Owner = this;
                msg.ShowDialog();

                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            }
            else
            {
                string errorMsg = result.Errors.Any() ? string.Join("\n", result.Errors) : "Unknown error when creating community";
                System.Diagnostics.Debug.WriteLine($"CreateCommunityAsync failed: {errorMsg}");
                var errorWindow = new ErrorWindow($"Community creation error:\n{errorMsg}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception in CreateCommunityAsync: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error:\n{httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid Operation in CreateCommunityAsync: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation:\n{invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in CreateCommunityAsync: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when creating community:\n{ex.Message}\n\nError type: {ex.GetType().Name}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task LoadCommunitiesAsync()
    {
        try
        {
            if (mediator == null)
            {
                return;
            }

            var query = new GetByUserIdQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                communities.Clear();

                for (int i = CommunitiesPanel.Children.Count - 1; i >= 1; i--)
                {
                    CommunitiesPanel.Children.RemoveAt(i);
                }

                foreach (var communityUser in result.Value)
                {
                    var communityItem = new CommunityItem
                    {
                        CommunityUserId = communityUser.Id,
                        CommunityId = communityUser.Community.Id,
                        Name = communityUser.Community.Name,
                        UserRole = communityUser.UserRole,
                    };

                    System.Diagnostics.Debug.WriteLine($"Community: {communityItem.Name}, Role: {communityItem.UserRole}");

                    communities.Add(communityItem);

                    var button = CreateCommunityButton(communityItem);
                    CommunitiesPanel.Children.Add(button);
                }
            }
            else
            {
                string errorMsg = result.Errors.Any() ? string.Join(", ", result.Errors) : "Unknown error";
                System.Diagnostics.Debug.WriteLine($"LoadCommunitiesAsync failed: {errorMsg}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception in LoadCommunitiesAsync: {ex}");
            var errorWindow = new ErrorWindow($"Error loading communities: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private Button CreateCommunityButton(CommunityItem community)
    {
        var button = new Button
        {
            Style = (Style)FindResource("CommunityButtonStyle"),
            Tag = community,
            Content = community.Name,
        };

        button.Click += CommunityButton_Click;

        return button;
    }

    private void CommunityButton_Click(object sender, RoutedEventArgs e)
    {
        if (isEditMode)
        {
            var msg = new MessageWindow("Please finish editing the category first.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (viewModel.IsPasswordEditPanelVisible)
        {
            var msg = new MessageWindow("Please finish editing the password first.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        try
        {
            if (sender is Button btn)
            {
                CommunityItem community;

                if (btn.Tag is string tagStr && tagStr == "0")
                {
                    community = new CommunityItem
                    {
                        CommunityId = 0,
                        Name = "Private",
                        UserRole = CommunityRole.Member,
                    };
                }
                else if (btn.Tag is CommunityItem commItem)
                {
                    community = commItem;
                }
                else
                {
                    return;
                }

                _ = EnterCommunityAsync(community);
                SetActiveCommunity(btn);
            }
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation opening community: {invEx}");
            var errorWindow = new ErrorWindow($"Failed to open community.\nCommunity may be deleted or you don't have access.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception opening community: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when opening community.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private async Task EnterCommunityAsync(CommunityItem community)
    {
        bool isPrivate = string.Equals(community.Name, "Private", StringComparison.OrdinalIgnoreCase);

        currentCommunity = isPrivate ? null : community;

        System.Diagnostics.Debug.WriteLine($"Entering community: {community.Name}, Role: {community.UserRole}");

        if (isPrivate)
        {
            await LoadCategoriesAsync();

            AdminPanelButton.Visibility = Visibility.Collapsed;

            if (CategoriesPanel.Children.Count > 0 && CategoriesPanel.Children[0] is Button firstButton)
            {
                SetActiveCategory(firstButton);
            }
        }
        else
        {
            CategoriesPanel.Children.Clear();

            var allBtn = new Button
            {
                Style = (Style)FindResource("CategoryButtonStyle"),
                Tag = "AllItems",
            };
            allBtn.Click += CategoryButton_Click;

            var allContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };

            var allIcon = new Border
            {
                Width = 30,
                Height = 20,
                Margin = new Thickness(0, 0, 10, 0),
            };

            var allText = new TextBlock
            {
                Text = "All items",
                VerticalAlignment = VerticalAlignment.Center,
            };

            allContent.Children.Add(allIcon);
            allContent.Children.Add(allText);
            allBtn.Content = allContent;
            CategoriesPanel.Children.Add(allBtn);

            var favBtn = new Button
            {
                Style = (Style)FindResource("CategoryButtonStyle"),
                Tag = "Favorite",
            };
            favBtn.Click += CategoryButton_Click;

            var favContent = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };

            var favIcon = new Border
            {
                Width = 30,
                Height = 20,
                Margin = new Thickness(0, 0, 10, 0),
            };

            var favText = new TextBlock
            {
                Text = "Favorite",
                VerticalAlignment = VerticalAlignment.Center,
            };

            favContent.Children.Add(favIcon);
            favContent.Children.Add(favText);
            favBtn.Content = favContent;
            CategoriesPanel.Children.Add(favBtn);

            // Set All items as active
            SetActiveCategory(allBtn);

            if (community.UserRole == CommunityRole.Owner)
            {
                AdminPanelButton.Visibility = Visibility.Visible;
                AdminPanelButton.Opacity = 1.0;
                AdminPanelButton.IsEnabled = true;
            }
            else
            {
                AdminPanelButton.Visibility = Visibility.Collapsed;
            }
        }

        UpdateEditButtonsVisibility();
    }

    private void AdminPanelButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentCommunity != null)
        {
            var adminWindow = new AdminPanelWindow(
                currentCommunity,
                userId,
                onDeleted: async () =>
                {
                    await LoadCommunitiesAsync();

                    CommunityButton_Click(PrivateCommunityButton, new RoutedEventArgs());
                },
                onUpdated: async () =>
                {
                    await LoadCommunitiesAsync();

                    if (currentActiveCommunityButton != null)
                    {
                        var textBlock = FindTextBlockInButton(currentActiveCommunityButton);
                        if (textBlock != null)
                        {
                            textBlock.Text = currentCommunity.Name;
                        }
                    }
                })
            {
                Owner = this,
            };
            adminWindow.ShowDialog();
        }
    }

    private Button CreateCategoryButton(CategoryItem category)
    {
        var button = new Button
        {
            Style = (Style)FindResource("CategoryButtonStyle"),
            Tag = category,
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var iconSpace = new Border
        {
            Width = 30,
            Height = 20,
            Margin = new Thickness(0, 0, 10, 0),
        };

        var name = new TextBlock
        {
            Text = category.Name,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed,
        };

        var editButton = new Button
        {
            Content = "✏",
            Width = 25,
            Height = 25,
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053")),
            BorderThickness = new Thickness(0),
            FontSize = 14,
            Cursor = Cursors.Hand,
            Margin = new Thickness(5, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Tag = category,
        };
        editButton.Click += EditCategoryButton_Click;

        var deleteButton = new Button
        {
            Content = "×",
            Width = 25,
            Height = 25,
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Cursor = Cursors.Hand,
            Margin = new Thickness(0, 0, 5, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Tag = category,
        };
        deleteButton.Click += DeleteCategoryButton_Click;

        buttonPanel.Children.Add(editButton);
        buttonPanel.Children.Add(deleteButton);

        Grid.SetColumn(iconSpace, 0);
        Grid.SetColumn(name, 1);
        Grid.SetColumn(buttonPanel, 2);

        grid.Children.Add(iconSpace);
        grid.Children.Add(name);
        grid.Children.Add(buttonPanel);

        button.Content = grid;
        button.Click += CategoryButton_Click;

        return button;
    }

    private async void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;

        if (!CanEditCurrentCommunity())
        {
            var msg = new MessageWindow("You don't have permission to delete categories in this community.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        if (sender is Button deleteButton && deleteButton.Tag is CategoryItem category)
        {
            var confirm = new ConfirmDialog($"Are you sure you want to delete the '{category.Name}' category?");
            confirm.Owner = this;

            if (confirm.ShowDialog() == true)
            {
                await DeleteCategoryAsync(category);
            }
        }
    }

    private string GetStoredRefreshToken()
    {
        return string.Empty;
    }

    private void ClearStoredTokens()
    {
    }

    private void CopyPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            var grid = button.Parent as Grid;
            if (grid == null)
            {
                return;
            }

            var border = grid.Parent as Border;
            if (border == null || border.Tag == null)
            {
                return;
            }

            var passwordData = border.Tag as PasswordData;
            if (passwordData == null)
            {
                return;
            }

            Clipboard.SetText(passwordData.Password);

            var msg = new MessageWindow("Password copied to clipboard!");
            msg.Owner = this;
            msg.ShowDialog();
        }
        catch (System.Runtime.InteropServices.ExternalException clipEx)
        {
            System.Diagnostics.Debug.WriteLine($"Clipboard exception: {clipEx}");
            var errorWindow = new ErrorWindow($"Failed to copy password to clipboard.\nClipboard may be in use by another application.\n\nPlease try again.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception copying password: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when copying password.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        var msg = new MessageWindow("Favorites feature will be implemented in the future.");
        msg.Owner = this;
        msg.ShowDialog();
    }

    private void EditPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        if (!CanEditCurrentCommunity())
        {
            var msg = new MessageWindow("You don't have permission to edit passwords in this community.");
            msg.Owner = this;
            msg.ShowDialog();
            return;
        }

        var button = sender as Button;
        if (button == null)
        {
            return;
        }

        var grid = button.Parent as Grid;
        if (grid == null)
        {
            return;
        }

        var border = grid.Parent as Border;
        if (border == null || border.Tag == null)
        {
            return;
        }

        var passwordData = border.Tag as PasswordData;
        if (passwordData == null)
        {
            return;
        }

        CategoryEditPanel.Visibility = Visibility.Collapsed;
        currentEditingPasswordCard = border;
        currentEditingPassword = passwordData;

        viewModel.OpenEditPasswordMode(passwordData, currentCategoryId);

        if (PasswordsScrollViewer != null)
        {
            PasswordsScrollViewer.ScrollToTop();
        }
    }

    private string CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return "none";
        }

        int score = 0;

        if (password.Length >= 8)
        {
            score++;
        }

        if (password.Length >= 12)
        {
            score++;
        }

        if (password.Any(char.IsLower))
        {
            score++;
        }

        if (password.Any(char.IsUpper))
        {
            score++;
        }

        if (password.Any(char.IsDigit))
        {
            score++;
        }

        if (password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            score++;
        }

        if (score >= 5)
        {
            return "strong";
        }
        else if (score >= 3)
        {
            return "medium";
        }
        else
        {
            return "weak";
        }
    }

    private void CreatePasswordCard(long id, string name, string login, string password, string iconPath, string strength, long categoryId)
    {
#pragma warning disable SA1413

        var border = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E121B")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 15),
            Height = 100
        };

        var passwordData = new PasswordData
        {
            Id = id,
            Name = name,
            Login = login,
            Password = password,
            IconPath = iconPath,
            Strength = strength,
            CategoryId = categoryId
        };
        border.Tag = passwordData;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        var iconBorder = new Border
        {
            Width = 60,
            Height = 60,
            CornerRadius = new CornerRadius(15),
            VerticalAlignment = VerticalAlignment.Center
        };

        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };
        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#38BDF8"), 0));
        gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FDE047"), 1));
        iconBorder.Background = gradient;

        var iconImage = new Image
        {
            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + iconPath)),
            Width = 35,
            Height = 35,
            Stretch = Stretch.Uniform
        };
        iconBorder.Child = iconImage;
        Grid.SetColumn(iconBorder, 0);

        var stackPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(20, 0, 0, 0)
        };

        var nameText = new TextBlock
        {
            Text = name,
            FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 5)
        };

        var loginText = new TextBlock
        {
            Text = login,
            FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
            FontSize = 14,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"))
        };

        stackPanel.Children.Add(nameText);
        stackPanel.Children.Add(loginText);
        Grid.SetColumn(stackPanel, 1);

        if (strength != "none")
        {
            var strengthBorder = new Border
            {
                Width = 85,
                Height = 30,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var strengthText = new TextBlock
            {
                Text = strength,
                FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            switch (strength)
            {
                case "strong":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EDAF"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EDAF"));
                    break;
                case "medium":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#713F12"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053"));
                    break;
                case "weak":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#620D0D"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6767"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6767"));
                    break;
            }

            strengthBorder.Child = strengthText;
            Grid.SetColumn(strengthBorder, 2);
            grid.Children.Add(strengthBorder);
        }

        var copyButton = new Button
        {
            Style = (Style)FindResource("IconButtonStyle"),
            Margin = new Thickness(0, 0, 5, 0)
        };
        copyButton.Click += CopyPasswordButton_Click;

        var copyImage = new Image
        {
            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/copy.png")),
            Width = 22,
            Height = 22
        };
        copyButton.Content = copyImage;
        Grid.SetColumn(copyButton, 3);

        var actionButton = new Button
        {
            Style = (Style)FindResource("IconButtonStyle")
        };

        bool canEdit = CanEditCurrentCommunity();
        if (viewModel.IsPasswordEditPanelVisible && canEdit)
        {
            actionButton.Click += EditPasswordButton_Click;
            var editImage = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/edit 1.png")),
                Width = 22,
                Height = 22
            };
            actionButton.Content = editImage;
        }
        else
        {
            actionButton.Click += FavoriteButton_Click;
            var starImage = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/star.png")),
                Width = 22,
                Height = 22
            };
            actionButton.Content = starImage;
        }

        Grid.SetColumn(actionButton, 4);

        grid.Children.Add(iconBorder);
        grid.Children.Add(stackPanel);
        grid.Children.Add(copyButton);
        grid.Children.Add(actionButton);

        border.Child = grid;

        PasswordsPanel.Children.Add(border);
#pragma warning restore SA1413
    }

    private void UpdatePasswordCard(Border border, string name, string login, string password, string iconPath, string strength)
    {
        var passwordData = new PasswordData
        {
            Name = name,
            Login = login,
            Password = password,
            IconPath = iconPath,
            Strength = strength,
        };
        border.Tag = passwordData;

        var grid = border.Child as Grid;
        if (grid == null)
        {
            return;
        }

        var iconBorder = grid.Children[0] as Border;
        if (iconBorder != null)
        {
            var iconImage = iconBorder.Child as Image;
            if (iconImage != null)
            {
                iconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + iconPath));
            }
        }

        var stackPanel = grid.Children[1] as StackPanel;
        if (stackPanel != null && stackPanel.Children.Count >= 2)
        {
            var nameText = stackPanel.Children[0] as TextBlock;
            if (nameText != null)
            {
                nameText.Text = name;
            }

            var loginText = stackPanel.Children[1] as TextBlock;
            if (loginText != null)
            {
                loginText.Text = login;
            }
        }

        UIElement? oldStrengthBadge = null;
        foreach (UIElement child in grid.Children)
        {
            if (Grid.GetColumn(child) == 2)
            {
                oldStrengthBadge = child;
                break;
            }
        }

        if (oldStrengthBadge != null)
        {
            grid.Children.Remove(oldStrengthBadge);
        }

        if (strength != "none")
        {
            var strengthBorder = new Border
            {
                Width = 85,
                Height = 30,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 15, 0),
                VerticalAlignment = VerticalAlignment.Center,
            };

            var strengthText = new TextBlock
            {
                Text = strength,
                FontFamily = new System.Windows.Media.FontFamily("Bahnschrift"),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
            };

            switch (strength)
            {
                case "strong":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EDAF"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#86EDAF"));
                    break;
                case "medium":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#713F12"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053"));
                    break;
                case "weak":
                    strengthBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#620D0D"));
                    strengthBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6767"));
                    strengthBorder.BorderThickness = new Thickness(2);
                    strengthText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6767"));
                    break;
            }

            strengthBorder.Child = strengthText;
            Grid.SetColumn(strengthBorder, 2);
            grid.Children.Add(strengthBorder);
        }
    }

    private void UpdatePasswordCardsButtons(bool showEditButton)
    {
        bool canEdit = CanEditCurrentCommunity();
        bool actualShowEdit = showEditButton && canEdit;

        foreach (var child in PasswordsPanel.Children)
        {
            if (child is Border border && border.Child is Grid grid)
            {
                Button? actionButton = null;
                foreach (UIElement element in grid.Children)
                {
                    if (element is Button btn && Grid.GetColumn(btn) == 4)
                    {
                        actionButton = btn;
                        break;
                    }
                }

                if (actionButton != null)
                {
                    grid.Children.Remove(actionButton);

                    var newButton = new Button
                    {
                        Style = (Style)FindResource("IconButtonStyle"),
                    };

                    if (actualShowEdit)
                    {
                        newButton.Click += EditPasswordButton_Click;
                        var editImage = new Image
                        {
                            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/edit 1.png")),
                            Width = 22,
                            Height = 22,
                        };
                        newButton.Content = editImage;
                    }
                    else
                    {
                        newButton.Click += FavoriteButton_Click;
                        var starImage = new Image
                        {
                            Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/star.png")),
                            Width = 22,
                            Height = 22,
                        };
                        newButton.Content = starImage;
                    }

                    Grid.SetColumn(newButton, 4);
                    grid.Children.Add(newButton);
                }
            }
        }
    }

    private TextBlock? FindTextBlockInButton(Button button)
    {
        if (button.Content is Grid grid)
        {
            foreach (var child in grid.Children)
            {
                if (child is TextBlock textBlock)
                {
                    return textBlock;
                }
            }
        }

        return null;
    }

    private void StartInvitationCheckTimer()
    {
        invitationCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(15),
        };
        invitationCheckTimer.Tick += async (s, e) => await CheckForInvitationsAsync();
        invitationCheckTimer.Start();
    }

    private async Task CheckForInvitationsAsync()
    {
        try
        {
            var query = new GetByRecipientIdQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsSuccess && result.Value != null)
            {
                var pendingInvitations = result.Value.Where(x => x.Status == RequestStatus.Pending).ToList();

                if (pendingInvitations.Any())
                {
                    InvitationsButton.Visibility = Visibility.Visible;
                    System.Diagnostics.Debug.WriteLine($"Pending invitations found: {pendingInvitations.Count}");
                }
                else
                {
                    InvitationsButton.Visibility = Visibility.Collapsed;
                    System.Diagnostics.Debug.WriteLine("No pending invitations - button hidden");
                }
            }
            else
            {
                InvitationsButton.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("No invitations - button hidden");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error checking invitations: {ex.Message}");

            InvitationsButton.Visibility = Visibility.Collapsed;
        }
    }

    private void InvitationsButton_Click(object sender, RoutedEventArgs e)
    {
        var invitationsWindow = new InvitationsWindow(userId, async () =>
        {
            await LoadCommunitiesAsync();
            await CheckForInvitationsAsync();
        })
        {
            Owner = this,
        };

        invitationsWindow.ShowDialog();

        _ = CheckForInvitationsAsync();
    }
}
