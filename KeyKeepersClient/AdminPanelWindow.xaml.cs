using System.Windows;
using System.Windows.Input;
using KeyKeepers.BLL.Commands.Communities.Delete;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class AdminPanelWindow : Window
{
    private readonly CommunityItem community;
    private readonly long userId;
    private readonly IMediator mediator;
    private readonly Action onCommunityDeleted;
    private readonly Action onCommunityUpdated;

    public AdminPanelWindow(CommunityItem communityItem, long currentUserId, Action onDeleted, Action onUpdated)
    {
        InitializeComponent();
        community = communityItem;
        userId = currentUserId;
        onCommunityDeleted = onDeleted;
        onCommunityUpdated = onUpdated;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();
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

    private void UpdateCommunityButton_Click(object sender, RoutedEventArgs e)
    {
        var updateWindow = new UpdateCommunityWindow(community, () =>
        {
            onCommunityUpdated?.Invoke();
        })
        {
            Owner = this,
        };

        updateWindow.ShowDialog();
    }

    private async void DeleteCommunityButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to delete community \"{community.Name}\"?\nThis action cannot be undone.",
            "Delete Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var command = new DeleteCommunityCommand(community.CommunityId);
            var deleteResult = await mediator.Send(command);

            if (deleteResult.IsSuccess)
            {
                var msg = new MessageWindow($"Community \"{community.Name}\" has been successfully deleted.");
                msg.Owner = this;
                msg.ShowDialog();

                onCommunityDeleted?.Invoke();
                Close();
            }
            else
            {
                var errorWindow = new ErrorWindow($"Error deleting community: {string.Join(", ", deleteResult.Errors)}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception deleting community: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when deleting community.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation deleting community: {invEx}");
            var errorWindow = new ErrorWindow($"Failed to delete community.\nIt may contain members or data.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (UnauthorizedAccessException unauthEx)
        {
            System.Diagnostics.Debug.WriteLine($"Unauthorized deleting community: {unauthEx}");
            var errorWindow = new ErrorWindow($"You do not have permission to delete this community.\nOnly the owner can delete a community.");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception deleting community: {ex}");
            var errorWindow = new ErrorWindow($"An error occurred when deleting community '{community.Name}'.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
        }
    }

    private void AddUserButton_Click(object sender, RoutedEventArgs e)
    {
        var addUserWindow = new AddUserToCommunityWindow(community, userId)
        {
            Owner = this,
        };

        addUserWindow.ShowDialog();
    }
}
