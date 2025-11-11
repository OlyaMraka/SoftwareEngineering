using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.JoinRequests.Create;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Queries.Users.GetByUsername;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class AddUserToCommunityWindow : Window
{
    private readonly CommunityItem community;
    private readonly long currentUserId;
    private readonly IMediator mediator;
    private UserResponseDto? foundUser = null;
    private System.Timers.Timer? debounceTimer;

    public AddUserToCommunityWindow(CommunityItem communityItem, long userId)
    {
        InitializeComponent();
        community = communityItem;
        currentUserId = userId;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();

        UsernameTextBox.Focus();
    }

    protected override void OnClosed(EventArgs e)
    {
        debounceTimer?.Stop();
        debounceTimer?.Dispose();
        base.OnClosed(e);
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

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void UsernameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Cancel existing timer
        debounceTimer?.Stop();
        debounceTimer?.Dispose();

        var username = UsernameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username))
        {
            ResetUI();
            return;
        }

        // Create new timer for debouncing (wait 500ms after user stops typing)
        debounceTimer = new System.Timers.Timer(500);
        debounceTimer.Elapsed += async (s, ev) =>
        {
            debounceTimer.Stop();
            await Dispatcher.InvokeAsync(async () => await SearchUserAsync(username));
        };
        debounceTimer.Start();
    }

    private async Task SearchUserAsync(string username)
    {
        try
        {
            var query = new GetByUsernameQuery(username);
            var result = await mediator.Send(query);

            if (result.IsSuccess && result.Value.Any())
            {
                // User found
                foundUser = result.Value.First();

                // Update UI - Green border
                UsernameBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                UsernameBorder.BorderThickness = new Thickness(2);

                // Show status
                StatusTextBlock.Text = "✓ User found";
                StatusTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                StatusTextBlock.Visibility = Visibility.Visible;

                // Show user info
                FoundUsernameTextBlock.Text = foundUser.UserName;
                UserInfoPanel.Visibility = Visibility.Visible;

                // Enable add button
                AddButton.IsEnabled = true;
            }
            else
            {
                // User not found - Red border
                foundUser = null;

                UsernameBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"));
                UsernameBorder.BorderThickness = new Thickness(2);

                StatusTextBlock.Text = "✗ User not found";
                StatusTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"));
                StatusTextBlock.Visibility = Visibility.Visible;

                UserInfoPanel.Visibility = Visibility.Collapsed;
                AddButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Error: {ex.Message}";
            StatusTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B"));
            StatusTextBlock.Visibility = Visibility.Visible;
            AddButton.IsEnabled = false;
        }
    }

    private void ResetUI()
    {
        foundUser = null;
        UsernameBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39"));
        UsernameBorder.BorderThickness = new Thickness(2);
        StatusTextBlock.Visibility = Visibility.Collapsed;
        UserInfoPanel.Visibility = Visibility.Collapsed;
        AddButton.IsEnabled = false;
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (foundUser == null)
        {
            return;
        }

        try
        {
            AddButton.IsEnabled = false;

            var dto = new CreateRequestDto
            {
                CommunityId = community.Id,
                SenderId = currentUserId,
                RecipientId = foundUser.Id,
                Comment = $"Invitation to join {community.Name}",
            };

            var command = new CreateJoinRequestCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show(
                    $"Join request sent to user '{foundUser.UserName}' successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.Close();
            }
            else
            {
                MessageBox.Show(
                    $"Error sending join request: {result.Errors.FirstOrDefault()?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                AddButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            AddButton.IsEnabled = true;
        }
    }
}
