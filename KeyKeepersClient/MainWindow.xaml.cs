using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;
using KeyKeepers.BLL.Commands.PasswordCategory.Delete;
using KeyKeepers.BLL.Commands.PasswordCategory.Update;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Queries.PasswordCategories.GetAll;
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

    private void AddPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Open dialog or window to add a new password
        MessageBox.Show(
            "Add Password functionality will be implemented here.",
            "Add Password",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        // Enter edit mode for creating new category
        isEditMode = true;
        currentEditingCategory = null;
        CategoryNameTextBox.Text = string.Empty;
        CategoryEditPanel.Visibility = Visibility.Visible;
        NormalButtonsPanel.Visibility = Visibility.Collapsed;
        EditButtonsPanel.Visibility = Visibility.Visible;
        AddCategoryButton.Visibility = Visibility.Collapsed;
        UpdateAllCategoryButtonsVisibility();
        CategoryNameTextBox.Focus();
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
        MessageBox.Show(
            "Функціонал копіювання паролю буде реалізовано в майбутньому.",
            "Копіювання",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Функціонал додавання до улюблених буде реалізовано в майбутньому.",
            "Улюблені",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
