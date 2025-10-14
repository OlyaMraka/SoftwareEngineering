using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.DTOs.Users;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class MainWindow : Window
{
    private readonly IMediator mediator;
    private Button? currentActiveButton;
    private ObservableCollection<CategoryItem> customCategories;

    public MainWindow()
    {
        InitializeComponent();
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();
        customCategories = new ObservableCollection<CategoryItem>();

        // Set the first button as active by default
        SetActiveCategory((Button)CategoriesPanel.Children[0]);
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

            // TODO: Load content for the selected category
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
        // TODO: Implement search functionality
        string searchText = SearchTextBox.Text.ToLower();

        // Filter categories based on search text
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
        // Show input dialog for category name
        var inputDialog = new Window
        {
            Title = "Add New Category",
            Width = 400,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = this,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#171C26")),
            WindowStyle = WindowStyle.ToolWindow,
        };

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

        var label = new TextBlock
        {
            Text = "Category Name:",
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 10),
        };
        Grid.SetRow(label, 0);

        var textBox = new TextBox
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E121B")),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 14,
            Padding = new Thickness(10),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39")),
            BorderThickness = new Thickness(1),
        };
        Grid.SetRow(textBox, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 20, 0, 0),
        };
        Grid.SetRow(buttonPanel, 3);

        var okButton = new Button
        {
            Content = "Add",
            Width = 80,
            Height = 30,
            Margin = new Thickness(0, 0, 10, 0),
            Background = new LinearGradientBrush(
                (Color)ColorConverter.ConvertFromString("#815A11"),
                (Color)ColorConverter.ConvertFromString("#FDE053"),
                0),
            Foreground = Brushes.Black,
            FontFamily = new FontFamily("Bahnschrift"),
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
        };
        okButton.Click += (s, args) =>
        {
            if (!string.IsNullOrWhiteSpace(textBox.Text))
            {
                AddCustomCategory(textBox.Text.Trim());
                inputDialog.DialogResult = true;
                inputDialog.Close();
            }
            else
            {
                MessageBox.Show(
                    "Please enter a category name.",
                    "Invalid Input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        };

        var cancelButton = new Button
        {
            Content = "Cancel",
            Width = 80,
            Height = 30,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39")),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Bahnschrift"),
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
        };
        cancelButton.Click += (s, args) =>
        {
            inputDialog.DialogResult = false;
            inputDialog.Close();
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(label);
        grid.Children.Add(textBox);
        grid.Children.Add(buttonPanel);

        inputDialog.Content = grid;
        inputDialog.ShowDialog();
    }

    private void AddCustomCategory(string categoryName)
    {
        var categoryItem = new CategoryItem
        {
            Name = categoryName,
            Icon = "📁", // Default icon
        };

        customCategories.Add(categoryItem);

        // Create button for the new category
        var button = CreateCategoryButton(categoryItem);
        CategoriesPanel.Children.Add(button);
    }

    private Button CreateCategoryButton(CategoryItem category)
    {
        var button = new Button
        {
            Style = (Style)FindResource("CategoryButtonStyle"),
            Tag = category.Name,
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
        };

        var icon = new TextBlock
        {
            Text = category.Icon,
            FontSize = 20,
            Margin = new Thickness(0, 0, 10, 0),
        };

        var name = new TextBlock
        {
            Text = category.Name,
            VerticalAlignment = VerticalAlignment.Center,
        };

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
            Margin = new Thickness(10, 0, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = category,
        };
        deleteButton.Click += DeleteCategoryButton_Click;

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(icon, 0);
        Grid.SetColumn(name, 1);
        Grid.SetColumn(deleteButton, 2);

        grid.Children.Add(icon);
        grid.Children.Add(name);
        grid.Children.Add(deleteButton);

        button.Content = grid;
        button.Click += CategoryButton_Click;

        return button;
    }

    private void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
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
                // Find and remove the category button
                Button? buttonToRemove = null;
                foreach (var child in CategoriesPanel.Children)
                {
                    if (child is Button btn && btn.Tag?.ToString() == category.Name)
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
