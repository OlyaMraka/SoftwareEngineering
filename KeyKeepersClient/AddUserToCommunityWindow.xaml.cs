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
        searchDebounceTimer.Stop();
        searchDebounceTimer.Start();

        string searchText = UsernameSearchTextBox.Text.Trim();
        AddButton.IsEnabled = !string.IsNullOrEmpty(searchText);

        if (selectedUser != null && searchText != selectedUser.UserName)
        {
            selectedUser = null;
            UserInfoPanel.Visibility = Visibility.Collapsed;
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
        if (SuggestionsListBox.SelectedItem != null)
        {
            SuggestionsListBox_SelectionChanged(sender, null!);
        }
    }

    private void UsernameSearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Down && SuggestionsPopup.IsOpen)
        {
            if (SuggestionsListBox.Items.Count > 0)
            {
                SuggestionsListBox.SelectedIndex = 0;
                SuggestionsListBox.Focus();
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Enter)
        {
            if (!string.IsNullOrEmpty(UsernameSearchTextBox.Text.Trim()))
            {
                AddButton_Click(sender, e);
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape)
        {
            SuggestionsPopup.IsOpen = false;
            e.Handled = true;
        }
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        string searchText = UsernameSearchTextBox.Text.Trim();

        if (string.IsNullOrEmpty(searchText))
        {
            var errorWindow = new ErrorWindow($"Please enter a username.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            return;
        }

        try
        {
            AddButton.IsEnabled = false;

            if (selectedUser == null || selectedUser.UserName != searchText)
            {
                var query = new GetByUsernameQuery(searchText);
                var result = await mediator.Send(query);

                if (result.IsSuccess && result.Value.Any())
                {
                    selectedUser = result.Value.FirstOrDefault(u => u.UserName.Equals(searchText, StringComparison.OrdinalIgnoreCase));

                    if (selectedUser == null)
                    {
                        var errorWindow = new ErrorWindow($"User '{searchText}' not found.");
                        errorWindow.Owner = this;
                        errorWindow.ShowDialog();
                        AddButton.IsEnabled = true;
                        return;
                    }
                }
                else
                {
                    var errorWindow = new ErrorWindow($"User '{searchText}' not found.");
                    errorWindow.Owner = this;
                    errorWindow.ShowDialog();
                    AddButton.IsEnabled = true;
                    return;
                }
            }

            var dto = new CreateRequestDto
            {
                CommunityId = community.CommunityId,
                RecipientId = selectedUser.Id,
                SenderId = community.CommunityUserId,
            };

            var command = new CreateJoinRequestCommand(dto);
            var createResult = await mediator.Send(command);

            if (createResult.IsSuccess)
            {
                var msg = new MessageWindow($"Invitation sent to {selectedUser.UserName} successfully!");
                msg.Owner = this;
                msg.ShowDialog();

                this.Close();
            }
            else
            {
                var errorWindow = new ErrorWindow($"Error sending invitation: {createResult.Errors.FirstOrDefault()?.Message}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                AddButton.IsEnabled = true;
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception adding user to community: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when sending invitation.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            AddButton.IsEnabled = true;
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation adding user: {invEx}");
            var errorWindow = new ErrorWindow($"Failed to send invitation.\nUser may already be in the community or already have an invitation.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            AddButton.IsEnabled = true;
        }
        catch (UnauthorizedAccessException unauthEx)
        {
            System.Diagnostics.Debug.WriteLine($"Unauthorized adding user: {unauthEx}");
            var errorWindow = new ErrorWindow($"You do not have permission to invite users to this community.\nOnly the owner or administrator can add users.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            AddButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"An error occurred when sending invitation.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            var errorWindow = new ErrorWindow($"An error occurred when sending invitation.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            AddButton.IsEnabled = true;
        }
    }
}
