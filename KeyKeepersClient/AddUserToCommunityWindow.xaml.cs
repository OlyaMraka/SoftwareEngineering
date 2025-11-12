using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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
    private readonly long senderId;
    private readonly IMediator mediator;
    private DispatcherTimer searchDebounceTimer;
    private UserResponseDto? selectedUser;
    private List<UserResponseDto> currentSuggestions = new();

    public AddUserToCommunityWindow(CommunityItem communityItem, long currentUserId)
    {
        InitializeComponent();
        community = communityItem;
        senderId = currentUserId;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();

        CommunityNameTextBlock.Text = community.Name;

        // Initialize debounce timer for search
        searchDebounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(300),
        };
        searchDebounceTimer.Tick += SearchDebounceTimer_Tick;
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

    private void UsernameSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Reset debounce timer
        searchDebounceTimer.Stop();
        searchDebounceTimer.Start();

        // Clear selection if text changed
        if (selectedUser != null && UsernameSearchTextBox.Text.Trim() != selectedUser.UserName)
        {
            selectedUser = null;
            UserInfoPanel.Visibility = Visibility.Collapsed;
            AddButton.IsEnabled = false;
        }
    }

    private async void SearchDebounceTimer_Tick(object? sender, EventArgs e)
    {
        searchDebounceTimer.Stop();

        string searchText = UsernameSearchTextBox.Text.Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            SuggestionsPopup.IsOpen = false;
            SearchStatusTextBlock.Visibility = Visibility.Collapsed;
            return;
        }

        try
        {
            SearchStatusTextBlock.Text = "Searching...";
            SearchStatusTextBlock.Visibility = Visibility.Visible;

            var query = new GetByUsernameQuery(searchText);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                currentSuggestions = result.Value.ToList();

                if (currentSuggestions.Any())
                {
                    SuggestionsListBox.ItemsSource = currentSuggestions.Select(u => u.UserName).ToList();
                    SuggestionsPopup.IsOpen = true;
                    SearchStatusTextBlock.Text = $"{currentSuggestions.Count} user(s) found";
                }
                else
                {
                    SuggestionsPopup.IsOpen = false;
                    SearchStatusTextBlock.Text = "No users found";
                }
            }
            else
            {
                SuggestionsPopup.IsOpen = false;
                SearchStatusTextBlock.Text = "Search failed";
            }
        }
        catch (Exception ex)
        {
            SearchStatusTextBlock.Text = $"Error: {ex.Message}";
            SuggestionsPopup.IsOpen = false;
        }
    }

    private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SuggestionsListBox.SelectedItem is string selectedUsername)
        {
            selectedUser = currentSuggestions.FirstOrDefault(u => u.UserName == selectedUsername);

            if (selectedUser != null)
            {
                UsernameSearchTextBox.Text = selectedUser.UserName;
                SelectedUsernameTextBlock.Text = selectedUser.UserName;
                UserInfoPanel.Visibility = Visibility.Visible;
                AddButton.IsEnabled = true;
                SuggestionsPopup.IsOpen = false;
                SearchStatusTextBlock.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void SuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Handle selection on mouse click
        if (SuggestionsListBox.SelectedItem != null)
        {
            SuggestionsListBox_SelectionChanged(sender, null!);
        }
    }

    private void UsernameSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && SuggestionsPopup.IsOpen)
        {
            // Move focus to suggestions list
            if (SuggestionsListBox.Items.Count > 0)
            {
                SuggestionsListBox.SelectedIndex = 0;
                SuggestionsListBox.Focus();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter && selectedUser != null)
        {
            // Add user on Enter key
            AddButton_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            // Close popup on Escape
            SuggestionsPopup.IsOpen = false;
            e.Handled = true;
        }
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        if (selectedUser == null)
        {
            MessageBox.Show(
                "Please select a user from the suggestions.",
                "No User Selected",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            AddButton.IsEnabled = false;

            var dto = new CreateRequestDto
            {
                CommunityId = community.CommunityId,
                RecipientId = selectedUser.Id,
                SenderId = community.CommunityUserId,
            };

            var command = new CreateJoinRequestCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show(
                    $"Invitation sent to {selectedUser.UserName} successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.Close();
            }
            else
            {
                MessageBox.Show(
                    $"Error sending invitation: {result.Errors.FirstOrDefault()?.Message}",
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
