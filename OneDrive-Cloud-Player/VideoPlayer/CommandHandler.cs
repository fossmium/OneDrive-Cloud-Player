using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OneDrive_Cloud_Player.VideoPlayer
{
	class CommandHandler : ICommand
	{
		public event EventHandler CanExecuteChanged;

		private Action _action;

		public CommandHandler(Action action)
		{
			this._action = action;
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}

		public void Execute(object parameter)
		{
			this._action();
		}
	}
}
