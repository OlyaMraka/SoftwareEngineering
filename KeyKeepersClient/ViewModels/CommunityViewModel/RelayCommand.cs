using System;
using System.Windows.Input;

namespace KeyKeepersClient.ViewModels.CommunityViewModel
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> executeAction, Predicate<object> canExecutePredicate = null!)
        {
            execute = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            canExecute = canExecutePredicate;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter!);
        }

        public void Execute(object? parameter)
        {
            execute(parameter!);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
