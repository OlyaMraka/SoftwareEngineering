using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using KeyKeepers.BLL.Commands.JoinRequests.Accept;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.BLL.Queries.JoinRequests.GetByRecipientId;
using KeyKeepers.DAL.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class InvitationsWindow : Window
{
    private readonly long userId;
    private readonly IMediator mediator;
    private readonly Action onInvitationHandled;
    private List<JoinRequestResponseDto> invitations = new();

    public InvitationsWindow(long currentUserId, Action onHandled)
    {
        InitializeComponent();
        userId = currentUserId;
        onInvitationHandled = onHandled;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();

        Loaded += InvitationsWindow_Loaded;
    }

    private async void InvitationsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadInvitationsAsync();
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

    private async Task LoadInvitationsAsync()
    {
        try
        {
            var query = new GetByRecipientIdQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsSuccess)
            {
                invitations = result.Value.Where(x => x.Status == RequestStatus.Pending).ToList();

                InvitationsPanel.Children.Clear();

                if (!invitations.Any())
                {
                    NoInvitationsText.Visibility = Visibility.Visible;
                    AcceptAllButton.IsEnabled = false;
                    DeclineAllButton.IsEnabled = false;
                }
                else
                {
                    NoInvitationsText.Visibility = Visibility.Collapsed;
                    AcceptAllButton.IsEnabled = true;
                    DeclineAllButton.IsEnabled = true;

                    foreach (var invitation in invitations)
                    {
                        var card = CreateInvitationCard(invitation);
                        InvitationsPanel.Children.Add(card);
                    }
                }
            }
            else
            {
                MessageBox.Show(
                    $"Error loading invitations: {result.Errors.FirstOrDefault()?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private Border CreateInvitationCard(JoinRequestResponseDto invitation)
    {
        var card = new Border
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E121B")),
            BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222A39")),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 15),
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // Left side - Invitation info
        var infoStack = new StackPanel();

        var communityText = new TextBlock
        {
            Text = invitation.Community.Name,
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 0, 5),
        };

        var fromText = new TextBlock
        {
            Text = $"From: {invitation.SenderUsername}",
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 14,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999")),
            Margin = new Thickness(0, 0, 0, 5),
        };

        var dateText = new TextBlock
        {
            Text = $"Received: {invitation.CreatedAt:MMM dd, yyyy HH:mm}",
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 12,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666")),
        };

        infoStack.Children.Add(communityText);
        infoStack.Children.Add(fromText);
        infoStack.Children.Add(dateText);

        // Accept Button
        var acceptButton = new Button
        {
            Content = "Accept",
            Width = 120,
            Height = 40,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B5E40")),
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Tag = invitation,
        };
        acceptButton.Click += AcceptButton_Click;

        var acceptTemplate = new ControlTemplate(typeof(Button));
        var acceptBorder = new FrameworkElementFactory(typeof(Border));
        acceptBorder.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
        acceptBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        var acceptPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        acceptPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        acceptPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        acceptBorder.AppendChild(acceptPresenter);
        acceptTemplate.VisualTree = acceptBorder;
        acceptButton.Template = acceptTemplate;

        // Decline Button
        var declineButton = new Button
        {
            Content = "Decline",
            Width = 120,
            Height = 40,
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A1010")),
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6B6B")),
            FontFamily = new FontFamily("Bahnschrift"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            BorderThickness = new Thickness(0),
            Cursor = Cursors.Hand,
            Tag = invitation,
        };
        declineButton.Click += DeclineButton_Click;

        var declineTemplate = new ControlTemplate(typeof(Button));
        var declineBorder = new FrameworkElementFactory(typeof(Border));
        declineBorder.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
        declineBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
        var declinePresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        declinePresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        declinePresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        declineBorder.AppendChild(declinePresenter);
        declineTemplate.VisualTree = declineBorder;
        declineButton.Template = declineTemplate;

        Grid.SetColumn(infoStack, 0);
        Grid.SetColumn(acceptButton, 1);
        Grid.SetColumn(declineButton, 3);

        grid.Children.Add(infoStack);
        grid.Children.Add(acceptButton);
        grid.Children.Add(declineButton);

        card.Child = grid;
        return card;
    }

    private async void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is JoinRequestResponseDto invitation)
        {
            await HandleInvitationAsync(invitation.Id, RequestStatus.Accepted);
        }
    }

    private async void DeclineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is JoinRequestResponseDto invitation)
        {
            await HandleInvitationAsync(invitation.Id, RequestStatus.Declined);
        }
    }

    private async void AcceptAllButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to accept all invitations?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await HandleAllInvitationsAsync(RequestStatus.Accepted);
        }
    }

    private async void DeclineAllButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to decline all invitations?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await HandleAllInvitationsAsync(RequestStatus.Declined);
        }
    }

    private async Task HandleInvitationAsync(long invitationId, RequestStatus status)
    {
        try
        {
            var dto = new AcceptOrDeclineDto
            {
                Id = invitationId,
                Status = status,
            };

            var command = new AcceptOrDeclineCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                var actionText = status == RequestStatus.Accepted ? "accepted" : "declined";
                MessageBox.Show(
                    $"Invitation {actionText} successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Reload invitations
                await LoadInvitationsAsync();

                // Notify parent window
                onInvitationHandled?.Invoke();
            }
            else
            {
                MessageBox.Show(
                    $"Error handling invitation: {result.Errors.FirstOrDefault()?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task HandleAllInvitationsAsync(RequestStatus status)
    {
        try
        {
            var tasks = invitations.Select(inv =>
            {
                var dto = new AcceptOrDeclineDto
                {
                    Id = inv.Id,
                    Status = status,
                };
                var command = new AcceptOrDeclineCommand(dto);
                return mediator.Send(command);
            });

            var results = await Task.WhenAll(tasks);

            var failedCount = results.Count(r => !r.IsSuccess);

            if (failedCount == 0)
            {
                var actionText = status == RequestStatus.Accepted ? "accepted" : "declined";
                MessageBox.Show(
                    $"All invitations {actionText} successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    $"{failedCount} invitation(s) failed to process. Please try again.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            // Reload invitations
            await LoadInvitationsAsync();

            // Notify parent window
            onInvitationHandled?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
