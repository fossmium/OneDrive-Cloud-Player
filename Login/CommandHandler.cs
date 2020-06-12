using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.Login
{
    class CommandHandler : ICommand
    {
        readonly Action<object> executeMethod;
        readonly Func<object, bool> canExecuteMethod;

        public CommandHandler(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            executeMethod(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
