using System;
using System.Windows.Input;

public class RelayCommand<T> : ICommand
{
    private readonly Predicate<T> _canExecute;
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute((T)parameter!);
    }

    public void Execute(object? parameter)
    {
        _execute((T)parameter!);
    }

    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    //public void RaiseCanExecuteChanged()
    //{
    //    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    //}
}