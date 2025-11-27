using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;

namespace KeyKeepersClient.ViewModels.CommunityViewModel
{
    public class AddCommunityViewModel : INotifyPropertyChanged
    {
        private string? communityName;
        private string? characterCount;
        private Brush? characterCountForeground;
        private bool? isCreateEnabled;

        public AddCommunityViewModel()
        {
            CloseCommand = new RelayCommand(Close);
            CreateCommand = new RelayCommand(Create);
            CancelCommand = new RelayCommand(Cancel);

            CommunityName = string.Empty;

            UpdateCharacterCount();
        }

        public event EventHandler<DialogResultEventArgs>? DialogResultRequested;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? CommunityName
        {
            get => communityName;
            set
            {
                if (communityName != value)
                {
                    communityName = value;
                    OnPropertyChanged();
                    UpdateCharacterCount();
                    UpdateCreateButtonState();
                }
            }
        }

        public string? CharacterCount
        {
            get => characterCount;
            private set
            {
                if (characterCount != value)
                {
                    characterCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush? CharacterCountForeground
        {
            get => characterCountForeground;
            private set
            {
                if (characterCountForeground != value)
                {
                    characterCountForeground = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool? IsCreateEnabled
        {
            get => isCreateEnabled;
            private set
            {
                if (isCreateEnabled != value)
                {
                    isCreateEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CloseCommand { get; }

        public ICommand CreateCommand { get; }

        public ICommand CancelCommand { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateCharacterCount()
        {
            var length = CommunityName?.Length ?? 0;
            CharacterCount = $"{length} / 50 characters";

            CharacterCountForeground = length >= 45
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FDE053"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#666666"));
        }

        private void UpdateCreateButtonState()
        {
            IsCreateEnabled = !string.IsNullOrWhiteSpace(CommunityName);
            ((RelayCommand)CreateCommand).RaiseCanExecuteChanged();
        }

        private bool? CanCreate(object parameter)
        {
            return IsCreateEnabled;
        }

        private void Create(object parameter)
        {
            if (string.IsNullOrWhiteSpace(CommunityName))
            {
                return;
            }

            DialogResultRequested?.Invoke(this, new DialogResultEventArgs(true, CommunityName.Trim()));
        }

        private void Cancel(object parameter)
        {
            DialogResultRequested?.Invoke(this, new DialogResultEventArgs(false));
        }

        private void Close(object parameter)
        {
            DialogResultRequested?.Invoke(this, new DialogResultEventArgs(false));
        }
    }
}
