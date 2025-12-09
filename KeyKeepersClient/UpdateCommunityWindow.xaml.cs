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

        bool isValid = ValidateName(text);
        SaveButton.IsEnabled = isValid && text != community.Name;

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
                CommunityId = community.CommunityId,
                Name = newName,
            };

            var command = new UpdateCommunityCommand(dto);
            var result = await mediator.Send(command);

            if (result.IsSuccess)
            {
                var msg = new MessageWindow($"Community '{newName}' has been successfully updated!");
                msg.Owner = this;
                msg.ShowDialog();

                community.Name = newName;

                onUpdateSuccess?.Invoke();

                this.Close();
            }
            else
            {
                var errorWindow = new ErrorWindow($"Error updating community: {result.Errors.FirstOrDefault()?.Message}");
                errorWindow.Owner = this;
                errorWindow.ShowDialog();
                SaveButton.IsEnabled = true;
            }
        }
        catch (System.Net.Http.HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"HTTP Exception updating community: {httpEx}");
            var errorWindow = new ErrorWindow($"Server connection error when updating community.\nCheck your internet connection.\n\nDetails: {httpEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            SaveButton.IsEnabled = true;
        }
        catch (InvalidOperationException invEx)
        {
            System.Diagnostics.Debug.WriteLine($"Invalid operation updating community: {invEx}");
            var errorWindow = new ErrorWindow($"Invalid operation when updating community.\nCommunity with this name may already exist.\n\nDetails: {invEx.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            SaveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected exception updating community: {ex}");
            var errorWindow = new ErrorWindow($"An unexpected error occurred when updating community '{newName}'.\n\nError type: {ex.GetType().Name}\nMessage: {ex.Message}");
            errorWindow.Owner = this;
            errorWindow.ShowDialog();
            SaveButton.IsEnabled = true;
        }
    }
}
