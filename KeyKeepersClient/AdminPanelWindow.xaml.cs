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
                MessageBox.Show(
                    $"Community \"{community.Name}\" has been successfully deleted.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                onCommunityDeleted?.Invoke();
                Close();
            }
            else
            {
                MessageBox.Show(
                    $"Error deleting community: {string.Join(", ", deleteResult.Errors)}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while deleting community: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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
