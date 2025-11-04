using System;
using System.Windows;
using System.Windows.Input;

namespace KeyKeepersClient
{
    /// <summary>
    /// Interaction logic for AddCommunityWindow.xaml.
    /// </summary>
    public partial class AddCommunityWindow : Window
    {
        public AddCommunityWindow()
        {
            InitializeComponent();
            CommunityNameTextBox.Focus();
            CommunityName = string.Empty;
        }

        public string CommunityName { get; private set; }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CommunityName = CommunityNameTextBox.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void CommunityNameTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var text = CommunityNameTextBox.Text;
            CharacterCountTextBlock.Text = $"{text.Length} / 50 characters";

            // Enable Create button only if there's text
            CreateButton.IsEnabled = !string.IsNullOrWhiteSpace(text);

            // Change counter color if approaching limit
            if (text.Length >= 45)
            {
                CharacterCountTextBlock.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FDE053"));
            }
            else
            {
                CharacterCountTextBlock.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#666666"));
            }
        }
    }
}
