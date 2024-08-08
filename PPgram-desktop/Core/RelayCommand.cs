﻿using System.Windows.Input;

namespace PPgram_desktop.Core;

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);

        _execute = execute;
        _canExecute = canExecute;
    }
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }
    public event EventHandler? CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    public void Execute(object parameter)
    {
        _execute(parameter ?? "<N/A>");
    }
}
