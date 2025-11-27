using System.Windows;
using KeyKeepersClient.ViewModels.CommunityViewModel;

namespace KeyKeepersClient
{
    public partial class AddCommunityWindow : Window
    {
        public AddCommunityWindow()
        {
            InitializeComponent();

            if (DataContext is AddCommunityViewModel vm)
            {
                vm.DialogResultRequested += Vm_DialogResultRequested;
            }
        }

        public string? CommunityName { get; private set; }

        private void Vm_DialogResultRequested(object? sender, DialogResultEventArgs e)
        {
            DialogResult = e.DialogResult;
            if (e.DialogResult)
            {
                this.CommunityName = e.CommunityName;
            }

            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
