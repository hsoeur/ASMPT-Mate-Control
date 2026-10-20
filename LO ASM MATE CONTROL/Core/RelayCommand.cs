using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LO_ASM_MATE_CONTROL.Core
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Predicate<object> canExecute, Action<object> execeute)
        {
            _canExecute = canExecute;
            _execute = execeute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }

        }

        public bool CanExecute(object parameter)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _canExecute(parameter);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public void Execute(object parameter)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            _execute(parameter);
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
