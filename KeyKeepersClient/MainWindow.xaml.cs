using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class MainWindow : Window
{
    private readonly IMediator mediator;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly long userId;
    private long? communityUserId;
    private long? communityId;
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

        // Load categories from database
        LoadCategoriesAsync();

        // Set the first button as active by default
        SetActiveCategory((Button)CategoriesPanel.Children[0]);
    }

    private async void LoadCategoriesAsync()
    {
        try
        {
            var queryOptions = new QueryOptions<KeyKeepers.DAL.Entities.PrivateCategory>
            {
                Filter = c => c.CommunityUser.UserId == userId,
                AsNoTracking = true, // Don't track to avoid issues with User/RefreshToken
            };

            var categories = await repositoryWrapper.PrivatePasswordCategoryRepository.GetAllAsync(queryOptions);

            foreach (var category in categories)
            {
                // Store communityUserId and communityId from the first category
                if (!communityUserId.HasValue && category.CommunityUserId > 0)
                {
                    communityUserId = category.CommunityUserId;
                    var categoryItem = new CategoryItem
                    {
                        Id = category.Id,
                        Name = category.Name,
                    };

                    customCategories.Add(categoryItem);
                    var button = CreateCategoryButton(categoryItem);
                    CategoriesPanel.Children.Add(button);
                }

                // Don't create Community/CommunityUser here - do it only when user creates first category
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

    private async Task EnsureCommunityUserExistsAsync()
    {
        try
        {
            // Try to find existing CommunityUser
            var communityUserOptions = new QueryOptions<KeyKeepers.DAL.Entities.CommunityUser>
            {
                Filter = cu => cu.UserId == userId,
                AsNoTracking = true, // Don't track to avoid issues with User/RefreshToken
            };

            var communityUser = await repositoryWrapper.CommunityUserRepository.GetFirstOrDefaultAsync(communityUserOptions);

            if (communityUser != null)
            {
                communityUserId = communityUser.Id;
                communityId = communityUser.CommunityId;
            }
            else
            {
                // Create default community for user WITHOUT loading User entity to avoid RefreshToken DateTime issues
                var defaultCommunity = new KeyKeepers.DAL.Entities.Community
                {
                    Name = "Personal",
                    CreatedAt = DateTime.UtcNow,
                };

                await repositoryWrapper.CommunityRepository.CreateAsync(defaultCommunity);
                await repositoryWrapper.SaveChangesAsync();

                // Store the communityId right after creation
                communityId = defaultCommunity.Id;

                // Create CommunityUser with completely unique UserName
                var timestamp = DateTime.UtcNow.Ticks;
                var newCommunityUser = new KeyKeepers.DAL.Entities.CommunityUser
                {
                    UserId = userId,
                    UserName = $"cu_{userId}_{timestamp}",
                    PasswordHash = $"placeholder_{timestamp}",
                    CommunityId = defaultCommunity.Id,
                    Role = KeyKeepers.DAL.Enums.CommunityRole.Owner,
                    CreatedAt = DateTime.UtcNow,
                };

                await repositoryWrapper.CommunityUserRepository.CreateAsync(newCommunityUser);
                await repositoryWrapper.SaveChangesAsync();

                communityUserId = newCommunityUser.Id;
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error creating user community:\n{ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\n\nInner Inner Exception: {ex.InnerException.InnerException.Message}";
                }
            }

            MessageBox.Show(
                errorMessage,
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

    private async void DeleteEditButton_Click(object sender, RoutedEventArgs e)
    {
        if (currentEditingCategory != null)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the '{currentEditingCategory.Name}' category?",
                "Delete Category",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await DeleteCategoryAsync(currentEditingCategory);
                ExitEditMode();
            }
        }
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
            var queryOptions = new QueryOptions<KeyKeepers.DAL.Entities.PrivateCategory>
            {
                Filter = c => c.Id == category.Id,
            };

            var existingCategory = await repositoryWrapper.PrivatePasswordCategoryRepository.GetFirstOrDefaultAsync(queryOptions);

            if (existingCategory != null)
            {
                existingCategory.Name = newName;
                repositoryWrapper.PrivatePasswordCategoryRepository.Update(existingCategory);
                await repositoryWrapper.SaveChangesAsync();

                category.Name = newName;
                UpdateCategoryButton(category);
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
            var queryOptions = new QueryOptions<KeyKeepers.DAL.Entities.PrivateCategory>
            {
                Filter = c => c.Id == category.Id,
            };

            var existingCategory = await repositoryWrapper.PrivatePasswordCategoryRepository.GetFirstOrDefaultAsync(queryOptions);

            if (existingCategory != null)
            {
                repositoryWrapper.PrivatePasswordCategoryRepository.Delete(existingCategory);
                await repositoryWrapper.SaveChangesAsync();

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
        try
        {
            // If community info is missing, create it first
            if (!communityUserId.HasValue || !communityId.HasValue)
            {
                await EnsureCommunityUserExistsAsync();
            }

            if (!communityUserId.HasValue)
            {
                MessageBox.Show(
                    "Unable to create category: User community information not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (!communityId.HasValue)
            {
                MessageBox.Show(
                    "Unable to create category: Community ID not found.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Create entity directly since mapping might not work correctly
            var newCategory = new KeyKeepers.DAL.Entities.PrivateCategory
            {
                Name = categoryName,
                CommunityId = communityId.Value,
                CommunityUserId = communityUserId.Value,
                CreatedAt = DateTime.UtcNow,
            };

            var createdCategory = await repositoryWrapper.PrivatePasswordCategoryRepository.CreateAsync(newCategory);
            await repositoryWrapper.SaveChangesAsync();

            var categoryItem = new CategoryItem
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
            };

            customCategories.Add(categoryItem);

            // Create button for the new category
            var button = CreateCategoryButton(categoryItem);
            CategoriesPanel.Children.Add(button);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Exception: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
            }

            MessageBox.Show(
                errorMessage,
                "Error creating category",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
}
