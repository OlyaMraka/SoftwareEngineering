using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient;

public partial class SettingsWindow : Window
{
    private readonly MainWindow mainWindow;

    public SettingsWindow(MainWindow owner)
    {
        InitializeComponent();
        this.Owner = owner;
        mainWindow = owner;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            this.DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void AddPasswordButton_Click(object sender, RoutedEventArgs e)
    {
        // Call the main window's method to add password
        mainWindow.OpenAddPasswordMode();
        this.Close();
    }

    private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
    {
        // Call the main window's method to add category
        mainWindow.OpenAddCategoryMode();
        this.Close();
    }

    private void AddCommunityButton_Click(object sender, RoutedEventArgs e)
    {
        // Call the main window's method to add community
        mainWindow.OpenAddCommunityMode();
        this.Close();
    }

    private void AccountSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // Open edit user window
        mainWindow.OpenEditUserMode();
        this.Close();
    }

    private void LogOutButton_Click(object sender, RoutedEventArgs e)
    {
        // Call the main window's logout method
        mainWindow.LogOutButton_Click(sender, e);
        this.Close();
    }
}
