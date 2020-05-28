using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.Login
{
    class VideoPlayerModel : ICommand
    {
        Action<object> executeMethod;
        Func<object, bool> canexecuteMethod;

        public VideoPlayerModel(Action<object> executeMethod, Func<object, bool> canexecuteMethod)
        {
            this.executeMethod = executeMethod;
            this.canexecuteMethod = canexecuteMethod;
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
