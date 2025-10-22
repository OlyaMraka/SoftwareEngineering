using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;
using KeyKeepers.BLL.Commands.PasswordCategory.Delete;
using KeyKeepers.BLL.Commands.PasswordCategory.Update;
using KeyKeepers.BLL.Commands.Passwords.Create;
using KeyKeepers.BLL.Commands.Passwords.Delete;
using KeyKeepers.BLL.Commands.Passwords.Update;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Queries.PasswordCategories.GetAll;
using KeyKeepers.BLL.Queries.Passwords.GetAllById;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class MainWindow : Window
{
    private readonly IMediator mediator;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly long userId;
    private Button? currentActiveButton;
    private ObservableCollection<CategoryItem> customCategories;
    private bool isEditMode = false;
    private CategoryItem? currentEditingCategory = null;
    private long currentCategoryId = 0; // 0 means "All items"

#pragma warning disable CS0414 // Field is assigned but its value is never used
    private bool isPasswordEditMode = false;
#pragma warning restore CS0414

    private string selectedPasswordIcon = "Images/Icons/internet_2.png";
    private string selectedCategoryIcon = "Images/Icons/internet";
    private Border? currentEditingPasswordCard = null;
    private PasswordData? currentEditingPassword = null;

    public MainWindow(long userId)
    {
        InitializeComponent();
        this.userId = userId;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();
        repositoryWrapper = App.ServiceProvider.GetRequiredService<IRepositoryWrapper>();
        customCategories = new ObservableCollection<CategoryItem>();

        this.Loaded += MainWindow_Loaded;

        SetActiveCategory((Button)CategoriesPanel.Children[0]);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadCategoriesAsync();

        // Load all passwords by default (category ID = 0 means "All items")
        await LoadPasswordsAsync(0);
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var query = new GetAllPasswordCategoriesQuery(userId);

            var result = await this.mediator.Send(query);

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
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading categories: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadPasswordsAsync(long categoryId)
    {
        try
        {
            if (this.mediator == null)
            {
                return;
            }

            // Clear current passwords
            PasswordsPanel.Children.Clear();

            var query = new GetCredentialsByIdQuery(categoryId);
            var result = await this.mediator.Send(query);

            if (result.IsSuccess)
            {
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
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading passwords: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
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

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button clickedButton)
        {
            SetActiveCategory(clickedButton);

            // Load passwords for this category
            if (clickedButton.Tag is CategoryItem category)
            {
                currentCategoryId = category.Id;
                _ = LoadPasswordsAsync(category.Id);
            }
            else
            {
                // "All items" button
                currentCategoryId = 0;
                _ = LoadPasswordsAsync(0);
            }
        }
    }

    private void SetActiveCategory(Button button)
    {
        // Reset previous active button to normal style
        if (currentActiveButton != null)
        {
            currentActiveButton.Style = (Style)FindResource("CategoryButtonStyle");
        }

        // Set new active button
        currentActiveButton = button;
        button.Style = (Style)FindResource("ActiveCategoryButtonStyle");
    }

    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string searchText = SearchTextBox.Text.ToLower();
    }

    private void CategoryNameTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Allow scrolling in the TextBox without showing scrollbar
        var textBox = sender as TextBox;
        if (textBox != null)
        {
            // Scroll the text horizontally
            if (e.Delta > 0)
            {
                // Scroll left
                textBox.ScrollToHorizontalOffset(textBox.HorizontalOffset - 20);
            }
            else
            {
                // Scroll right
                textBox.ScrollToHorizontalOffset(textBox.HorizontalOffset + 20);
            }

            e.Handled = true;
        }
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        // Enter edit mode for creating new category
        isEditMode = true;
        currentEditingCategory = null;
        CategoryNameTextBox.Text = string.Empty;

        // Set default icon
        selectedCategoryIcon = "Images/Icons/internet";
        CategoryIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + selectedCategoryIcon + "_2.png"));

        CategoryEditPanel.Visibility = Visibility.Visible;
        NormalButtonsPanel.Visibility = Visibility.Collapsed;
        EditButtonsPanel.Visibility = Visibility.Visible;
        AddCategoryButton.Visibility = Visibility.Collapsed;
        UpdateAllCategoryButtonsVisibility();
        CategoryNameTextBox.Focus();
    }

    private void AddPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        isPasswordEditMode = true;
        PasswordEditPanel.Visibility = Visibility.Visible;
        CategoryEditPanel.Visibility = Visibility.Collapsed;
        NormalButtonsPanel.Visibility = Visibility.Collapsed;
        PasswordEditButtonsPanel.Visibility = Visibility.Visible;
        AddCategoryButton.Visibility = Visibility.Collapsed;

        // Clear form
        PasswordNameTextBox.Text = string.Empty;
        PasswordLoginTextBox.Text = string.Empty;
        PasswordValueBox.Password = string.Empty;
        PasswordValueTextBox.Text = string.Empty;
        PasswordValueBox.Visibility = Visibility.Visible;
        PasswordValueTextBox.Visibility = Visibility.Collapsed;
        selectedPasswordIcon = "Images/Icons/internet_2.png";
        PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + selectedPasswordIcon));

        // Update all password cards to show edit button
        UpdatePasswordCardsButtons(true);
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
                // Create new category
                await AddCustomCategoryAsync(CategoryNameTextBox.Text.Trim());
            }
            else
            {
                // Update existing category
                await UpdateCategoryAsync(currentEditingCategory, CategoryNameTextBox.Text.Trim());
            }

            // Clear the textbox and reset currentEditingCategory, but stay in edit mode
            CategoryNameTextBox.Text = string.Empty;
            currentEditingCategory = null;
            CategoryNameTextBox.Focus();
        }
        else
        {
            MessageBox.Show(
                "Please enter a category name.",
                "Invalid Input",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private async void SaveEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (!isEditMode)
        {
            return;
        }

        // If there's text in the textbox, save it
        if (!string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
        {
            if (currentEditingCategory == null)
            {
                // Create new category
                await AddCustomCategoryAsync(CategoryNameTextBox.Text.Trim());
            }
            else
            {
                // Update existing category
                await UpdateCategoryAsync(currentEditingCategory, CategoryNameTextBox.Text.Trim());
            }
        }

        // Always exit edit mode when Save is clicked
        ExitEditMode();
    }

    private void ExitEditMode()
    {
        isEditMode = false;
        currentEditingCategory = null;
        CategoryNameTextBox.Text = string.Empty;
        CategoryEditPanel.Visibility = Visibility.Collapsed;
        NormalButtonsPanel.Visibility = Visibility.Visible;
        EditButtonsPanel.Visibility = Visibility.Collapsed;
        AddCategoryButton.Visibility = Visibility.Visible;
        UpdateAllCategoryButtonsVisibility();
    }

    private void UpdateAllCategoryButtonsVisibility()
    {
        // Update visibility of edit/delete buttons on all custom categories
        foreach (var child in CategoriesPanel.Children)
        {
            if (child is Button btn && btn.Content is Grid grid)
            {
                // Find the button panel (third child in grid)
                if (grid.Children.Count >= 3 && grid.Children[2] is StackPanel buttonPanel)
                {
                    buttonPanel.Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }

    private void EditCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button editButton && editButton.Tag is CategoryItem category)
        {
            // Enter edit mode for existing category
            isEditMode = true;
            currentEditingCategory = category;
            CategoryNameTextBox.Text = category.Name;
            CategoryEditPanel.Visibility = Visibility.Visible;
            NormalButtonsPanel.Visibility = Visibility.Collapsed;
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
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dto = new UpdatePrivateCategoryDto()
            {
                UserId = userId,
                Id = category.Id,
                Name = newName,
            };

            var command = new UpdatePrivateCategoryCommand(dto);

            var result = await this.mediator.Send(command);

            if (result.IsSuccess)
            {
                category.Name = newName;
                UpdateCategoryButton(category);
            }
            else
            {
                MessageBox.Show(
                    $"Error",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error updating category: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void UpdateCategoryButton(CategoryItem category)
    {
        // Find and update the category button
        foreach (var child in CategoriesPanel.Children)
        {
            if (child is Button btn && btn.Tag is CategoryItem tag && tag.Id == category.Id)
            {
                // Recreate the button with updated name
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
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var command = new DeletePrivateCategoryCommand(category.Id);

            var result = await this.mediator.Send(command);

            if (result.IsSuccess)
            {
                // Find and remove the category button
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
                    // If the deleted category was active, set "All items" as active
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
                MessageBox.Show(
                    "Category not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error deleting category: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task AddCustomCategoryAsync(string categoryName)
    {
        string categoryNamee = this.CategoryNameTextBox.Text.Trim();
        try
        {
            if (this.mediator == null)
            {
                MessageBox
                    .Show("База даних не налаштована. Реєстрація тимчасово недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dto = new CreatePrivateCategoryDto()
            {
                UserId = userId,
                Name = categoryNamee,
            };

            var command = new CreatePrivateCategoryCommand(dto);

            var result = await this.mediator.Send(command);

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
                string errorMsg = result.Errors.Any() ? string.Join(", ", result.Errors) : "Помилка при створенні користувача";
                MessageBox.Show($"Помилка реєстрації: {errorMsg}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Виникла помилка при реєстрації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // Empty space for icon (instead of actual icon)
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

        // Container for edit/delete buttons (on the right)
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = isEditMode ? Visibility.Visible : Visibility.Collapsed,
        };

        // Edit button
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

        // Delete button
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
        e.Handled = true; // Prevent category button click

        if (sender is Button deleteButton && deleteButton.Tag is CategoryItem category)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the '{category.Name}' category?",
                "Delete Category",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteCategoryAsync(category);
            }
        }
    }

    private async void LogOutButton_Click(object sender, RoutedEventArgs e)
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
                    MessageBox.Show(
                        "Error during logout. Please try again.",
                        "Logout Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred during logout: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
            // Find the password card (Border) that contains this button
            var button = sender as Button;
            if (button == null)
            {
                return;
            }

            // Navigate up to find the Grid that contains the button
            var grid = button.Parent as Grid;
            if (grid == null)
            {
                return;
            }

            // Navigate up to find the Border (password card)
            var border = grid.Parent as Border;
            if (border == null || border.Tag == null)
            {
                return;
            }

            // Extract password data from Tag
            var passwordData = border.Tag as PasswordData;
            if (passwordData == null)
            {
                return;
            }

            // Copy password to clipboard
            Clipboard.SetText(passwordData.Password);

            MessageBox.Show(
                "Пароль скопійовано в буфер обміну!",
                "Успіх",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка копіювання: {ex.Message}",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Функціонал додавання до улюблених буде реалізовано в майбутньому.",
            "Улюблені",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void EditPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        // Find the password card (Border) that contains this button
        var button = sender as Button;
        if (button == null)
        {
            return;
        }

        // Navigate up to find the Border (password card)
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

        // Extract password data from Tag
        var passwordData = border.Tag as PasswordData;
        if (passwordData == null)
        {
            return;
        }

        // Enter edit mode if not already in it
        if (!isPasswordEditMode)
        {
            isPasswordEditMode = true;
            CategoryEditPanel.Visibility = Visibility.Collapsed;
            NormalButtonsPanel.Visibility = Visibility.Collapsed;
            PasswordEditButtonsPanel.Visibility = Visibility.Visible;
            AddCategoryButton.Visibility = Visibility.Collapsed;

            // Update all password cards to show edit button
            UpdatePasswordCardsButtons(true);
        }

        // Store reference to the card being edited
        currentEditingPasswordCard = border;
        currentEditingPassword = passwordData;

        // Populate form fields
        PasswordNameTextBox.Text = passwordData.Name;
        PasswordLoginTextBox.Text = passwordData.Login;
        PasswordValueBox.Password = passwordData.Password;
        PasswordValueTextBox.Text = passwordData.Password;
        selectedPasswordIcon = passwordData.IconPath;
        PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + passwordData.IconPath));

        // Show edit panel
        PasswordEditPanel.Visibility = Visibility.Visible;

        // Scroll to top to show the edit panel
        if (PasswordsScrollViewer != null)
        {
            PasswordsScrollViewer.ScrollToTop();
        }
    }

    private void PasswordIconButton_Click(object sender, RoutedEventArgs e)
    {
        // Open icon selection popup
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }

    private void SelectIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string iconPath)
        {
            selectedPasswordIcon = iconPath;
            PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + iconPath));

            // Close the popup
            if (PasswordIconButton.ContextMenu != null)
            {
                PasswordIconButton.ContextMenu.IsOpen = false;
            }

            // Update border style for all icon buttons in the context menu
            UpdateIconSelectionHighlight(iconPath);
        }
    }

    private void UpdateIconSelectionHighlight(string selectedIconPath)
    {
        if (PasswordIconButton.ContextMenu == null)
        {
            return;
        }

        // Find the WrapPanel in the ContextMenu template
        var contextMenu = PasswordIconButton.ContextMenu;
        if (contextMenu.IsLoaded)
        {
            var border = System.Windows.Media.VisualTreeHelper.GetChild(contextMenu, 0) as Border;
            if (border != null)
            {
                var stackPanel = border.Child as StackPanel;
                if (stackPanel != null && stackPanel.Children.Count > 1)
                {
                    var wrapPanel = stackPanel.Children[1] as System.Windows.Controls.WrapPanel;
                    if (wrapPanel != null)
                    {
                        foreach (var child in wrapPanel.Children)
                        {
                            if (child is Button btn)
                            {
                                if (btn.Tag?.ToString() == selectedIconPath)
                                {
                                    // Highlight selected icon
                                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38BDF8"));
                                    btn.BorderThickness = new Thickness(3);
                                }
                                else
                                {
                                    // Reset other icons
                                    btn.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39"));
                                    btn.BorderThickness = new Thickness(2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void HighlightSelectedIcon(string iconPath)
    {
        // This method now just updates the highlight when the popup opens
        UpdateIconSelectionHighlight(iconPath);
    }

    private void CategoryIconButton_Click(object sender, RoutedEventArgs e)
    {
        // Open category icon selection popup
        if (sender is Button button && button.ContextMenu != null)
        {
            button.ContextMenu.PlacementTarget = button;
            button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            button.ContextMenu.IsOpen = true;
        }
    }

    private void SelectCategoryIcon_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string iconPath)
        {
            // Store icon path without _1 or _2 suffix
            selectedCategoryIcon = iconPath;

            // Update the icon image to show _2 version in edit mode
            CategoryIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + iconPath + "_2.png"));

            // Close the popup
            if (CategoryIconButton.ContextMenu != null)
            {
                CategoryIconButton.ContextMenu.IsOpen = false;
            }
        }
    }

    private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordValueBox.Visibility == Visibility.Visible)
        {
            // Show password
            PasswordValueTextBox.Text = PasswordValueBox.Password;
            PasswordValueBox.Visibility = Visibility.Collapsed;
            PasswordValueTextBox.Visibility = Visibility.Visible;
        }
        else
        {
            // Hide password
            PasswordValueBox.Password = PasswordValueTextBox.Text;
            PasswordValueTextBox.Visibility = Visibility.Collapsed;
            PasswordValueBox.Visibility = Visibility.Visible;
        }
    }

    private async void SavePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        // Check if all fields are filled
        string name = PasswordNameTextBox.Text.Trim();
        string login = PasswordLoginTextBox.Text.Trim();
        string password = PasswordValueBox.Visibility == Visibility.Visible
            ? PasswordValueBox.Password
            : PasswordValueTextBox.Text;

        // Validation
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(
                "Ім'я додатку обов'язкове!",
                "Помилка валідації",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

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

        if (string.IsNullOrWhiteSpace(login))
        {
            MessageBox.Show(
                "Логін обов'язковий!",
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

        if (string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show(
                "Пароль обов'язковий!",
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
            if (this.mediator == null)
            {
                MessageBox.Show(
                    "База даних не налаштована.",
                    "Помилка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Calculate password strength
            string strength = CalculatePasswordStrength(password);

            // Check if editing existing password or creating new one
            if (currentEditingPasswordCard != null && currentEditingPassword != null)
            {
                // Update existing password
                var updateRequest = new UpdatePasswordRequest
                {
                    Id = currentEditingPassword.Id,
                    AppName = name,
                    Login = login,
                    Password = password,
                    LogoUrl = selectedPasswordIcon,
                    CategoryId = currentCategoryId,
                };

                var updateCommand = new UpdatePasswordCommand(updateRequest);
                var updateResult = await this.mediator.Send(updateCommand);

                if (updateResult.IsSuccess)
                {
                    // Update the password card in UI
                    UpdatePasswordCard(currentEditingPasswordCard, name, login, password, selectedPasswordIcon, strength);

                    MessageBox.Show(
                        "Пароль успішно оновлено!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    // Clear editing references
                    currentEditingPasswordCard = null;
                    currentEditingPassword = null;
                }
                else
                {
                    MessageBox.Show(
                        $"Помилка оновлення паролю: {string.Join(", ", updateResult.Errors.Select(e => e.Message))}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                // Create new password
                var createRequest = new CreatePasswordRequest
                {
                    AppName = name,
                    Login = login,
                    Password = password,
                    LogoUrl = selectedPasswordIcon,
                    CategoryId = currentCategoryId,
                };

                var createCommand = new CreatePasswordCommand(createRequest);
                var createResult = await this.mediator.Send(createCommand);

                if (createResult.IsSuccess)
                {
                    // Create new password card in UI
                    CreatePasswordCard(
                        createResult.Value.Id,
                        createResult.Value.AppName,
                        createResult.Value.Login,
                        createResult.Value.Password,
                        createResult.Value.LogoUrl ?? "Images/Icons/internet_2.png",
                        strength,
                        createResult.Value.CategoryId);

                    MessageBox.Show(
                        "Пароль успішно збережено!",
                        "Успіх",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Помилка збереження паролю: {string.Join(", ", createResult.Errors.Select(e => e.Message))}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }
            }

            // Clear fields
            PasswordNameTextBox.Text = string.Empty;
            PasswordLoginTextBox.Text = string.Empty;
            PasswordValueBox.Password = string.Empty;
            PasswordValueTextBox.Text = string.Empty;
            selectedPasswordIcon = "Images/Icons/internet_2.png";
            PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + selectedPasswordIcon));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Помилка: {ex.Message}",
                "Помилка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private string CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return "none";
        }

        int score = 0;

        // Length
        if (password.Length >= 8)
        {
            score++;
        }

        if (password.Length >= 12)
        {
            score++;
        }

        // Contains lowercase
        if (password.Any(char.IsLower))
        {
            score++;
        }

        // Contains uppercase
        if (password.Any(char.IsUpper))
        {
            score++;
        }

        // Contains digits
        if (password.Any(char.IsDigit))
        {
            score++;
        }

        // Contains special characters
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
#pragma warning disable SA1413 // Use trailing comma in multi-line initializers
        // Create main border
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

        // Store password data in Tag
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

        // Create grid
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });

        // Icon with gradient background
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

        // Name and Login
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

        // Strength badge
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

        // Copy button
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

        // Edit or Favorite button (depending on mode)
        var actionButton = new Button
        {
            Style = (Style)FindResource("IconButtonStyle")
        };

        if (isPasswordEditMode)
        {
            // Edit button
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
            // Favorite button
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

        // Add all elements to grid
        grid.Children.Add(iconBorder);
        grid.Children.Add(stackPanel);
        grid.Children.Add(copyButton);
        grid.Children.Add(actionButton);

        border.Child = grid;

        // Add to passwords panel
        PasswordsPanel.Children.Add(border);
#pragma warning restore SA1413
    }

    private void UpdatePasswordCard(Border border, string name, string login, string password, string iconPath, string strength)
    {
        // Update the Tag with new data
        var passwordData = new PasswordData
        {
            Name = name,
            Login = login,
            Password = password,
            IconPath = iconPath,
            Strength = strength,
        };
        border.Tag = passwordData;

        // Get the grid inside the border
        var grid = border.Child as Grid;
        if (grid == null)
        {
            return;
        }

        // Update icon (column 0)
        var iconBorder = grid.Children[0] as Border;
        if (iconBorder != null)
        {
            var iconImage = iconBorder.Child as Image;
            if (iconImage != null)
            {
                iconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/" + iconPath));
            }
        }

        // Update name and login (column 1)
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

        // Update strength badge (column 2) - might need to be recreated if strength changed
        // Find and remove old strength badge if exists
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

        // Add new strength badge if needed
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

    private async void DeletePasswordButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Ви впевнені, що хочете видалити цей пароль?",
            "Підтвердження",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Delete password card if editing existing password
            if (currentEditingPasswordCard != null && currentEditingPassword != null)
            {
                try
                {
                    if (this.mediator == null)
                    {
                        MessageBox.Show(
                            "База даних не налаштована.",
                            "Помилка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }

                    var deleteCommand = new DeletePasswordCommand(currentEditingPassword.Id);
                    var deleteResult = await this.mediator.Send(deleteCommand);

                    if (deleteResult.IsSuccess)
                    {
                        PasswordsPanel.Children.Remove(currentEditingPasswordCard);
                        currentEditingPasswordCard = null;
                        currentEditingPassword = null;

                        // Clear fields
                        PasswordNameTextBox.Text = string.Empty;
                        PasswordLoginTextBox.Text = string.Empty;
                        PasswordValueBox.Password = string.Empty;
                        PasswordValueTextBox.Text = string.Empty;
                        PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/Icons/internet_2.png"));

                        MessageBox.Show(
                            "Пароль успішно видалено!",
                            "Успіх",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
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
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Помилка: {ex.Message}",
                        "Помилка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }

    private void ExitPasswordEditMode_Click(object sender, RoutedEventArgs e)
    {
        // Check if there are unsaved changes
        bool hasChanges = !string.IsNullOrWhiteSpace(PasswordNameTextBox.Text) ||
                          !string.IsNullOrWhiteSpace(PasswordLoginTextBox.Text) ||
                          !string.IsNullOrWhiteSpace(PasswordValueBox.Password) ||
                          !string.IsNullOrWhiteSpace(PasswordValueTextBox.Text);

        if (hasChanges)
        {
            var result = MessageBox.Show(
                "У вас є незбережені зміни. Зберегти їх перед виходом?",
                "Незбережені зміни",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SavePasswordButton_Click(sender, e);
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        // Exit password edit mode
        isPasswordEditMode = false;
        PasswordEditPanel.Visibility = Visibility.Collapsed;
        NormalButtonsPanel.Visibility = Visibility.Visible;
        PasswordEditButtonsPanel.Visibility = Visibility.Collapsed;
        AddCategoryButton.Visibility = Visibility.Visible;

        // Update all password cards to show favorite button
        UpdatePasswordCardsButtons(false);

        // Clear editing references
        currentEditingPasswordCard = null;
        currentEditingPassword = null;

        // Clear fields
        PasswordNameTextBox.Text = string.Empty;
        PasswordLoginTextBox.Text = string.Empty;
        PasswordValueBox.Password = string.Empty;
        PasswordValueTextBox.Text = string.Empty;
        PasswordIconImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Images/Icons/internet_2.png"));
    }

    private void UpdatePasswordCardsButtons(bool showEditButton)
    {
        // Iterate through all password cards in PasswordsPanel
        foreach (var child in PasswordsPanel.Children)
        {
            if (child is Border border && border.Child is Grid grid)
            {
                // Find the action button (column 4)
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
                    // Remove old button
                    grid.Children.Remove(actionButton);

                    // Create new button
                    var newButton = new Button
                    {
                        Style = (Style)FindResource("IconButtonStyle"),
                    };

                    if (showEditButton)
                    {
                        // Edit button
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
                        // Favorite button
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
}
