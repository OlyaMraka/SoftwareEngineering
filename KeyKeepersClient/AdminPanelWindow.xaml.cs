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
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception deleting community: {httpEx}");
            MessageBox.Show(
                $"Помилка з'єднання з сервером при видаленні спільноти.\nПеревірте підключення до інтернету.\n\nДеталі: {httpEx.Message}",
                "Помилка з'єднання",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation deleting community: {invEx}");
            MessageBox.Show(
                $"Не вдалося видалити спільноту.\nМожливо, у ній є учасники або дані.\n\nДеталі: {invEx.Message}",
                "Помилка операції",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (UnauthorizedAccessException unauthEx)
        {
            System.Diagnostics.Debug.WriteLine($"Unauthorized deleting community: {unauthEx}");
            MessageBox.Show(
                "У вас немає прав для видалення цієї спільноти.\nТільки власник може видалити спільноту.",
                "Доступ заборонено",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception deleting community: {ex}");
            MessageBox.Show(
                $"Виникла помилка при видаленні спільноти '{community.Name}'.\n\nТип помилки: {ex.GetType().Name}\nПовідомлення: {ex.Message}",
                "Критична помилка",
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
