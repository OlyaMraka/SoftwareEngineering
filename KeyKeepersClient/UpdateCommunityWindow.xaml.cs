using System.Windows;
using System.Windows.Input;
using KeyKeepers.BLL.Commands.Communities.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace KeyKeepersClient;

public partial class UpdateCommunityWindow : Window
{
    private readonly CommunityItem community;
    private readonly IMediator mediator;
    private readonly Action onUpdateSuccess;

    public UpdateCommunityWindow(CommunityItem communityItem, Action onSuccess)
    {
        InitializeComponent();
        community = communityItem;
        onUpdateSuccess = onSuccess;
        mediator = App.ServiceProvider.GetRequiredService<IMediator>();

        // Load current community name
        CommunityNameTextBox.Text = community.Name;
        CommunityNameTextBox.Focus();
        CommunityNameTextBox.SelectAll();
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

    private void CommunityNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        var text = CommunityNameTextBox.Text.Trim();
        CharacterCountTextBlock.Text = $"{text.Length} / 50 characters";

        // Validate and enable/disable save button
        bool isValid = ValidateName(text);
        SaveButton.IsEnabled = isValid && text != community.Name;

        // Change counter color if approaching limit
        if (text.Length >= 45)
        {
            CharacterCountTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FDE053"));
        }
        else
        {
            CharacterCountTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#666666"));
        }
    }

    private bool ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            NameErrorTextBlock.Text = CommunityConstants.CommunityNameRequiredError;
            NameErrorTextBlock.Visibility = Visibility.Visible;
            return false;
        }

        if (name.Length < CommunityConstants.MinNameLenght)
        {
            NameErrorTextBlock.Text = CommunityConstants.MinNameLenghtError;
            NameErrorTextBlock.Visibility = Visibility.Visible;
            return false;
        }

        if (name.Length > CommunityConstants.MaxNameLenght)
        {
            NameErrorTextBlock.Text = CommunityConstants.MaxNameLenghtError;
            NameErrorTextBlock.Visibility = Visibility.Visible;
            return false;
        }

        NameErrorTextBlock.Visibility = Visibility.Collapsed;
        return true;
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var newName = CommunityNameTextBox.Text.Trim();

        if (!ValidateName(newName))
        {
            return;
        }

        try
        {
            SaveButton.IsEnabled = false;

            var dto = new UpdateCommunityRequestDto
            {
                CommunityId = community.Id,
                Name = newName,
            };

            var command = new UpdateCommunityCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                MessageBox.Show(
                    $"Community '{newName}' has been successfully updated!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Update the community item
                community.Name = newName;

                // Call success callback to refresh UI
                onUpdateSuccess?.Invoke();

                this.Close();
            }
            else
            {
                MessageBox.Show(
                    $"Error updating community: {result.Errors.FirstOrDefault()?.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                SaveButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            SaveButton.IsEnabled = true;
        }
    }
}
