using OneDrive_Cloud_Player.DataStructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneDrive_Cloud_Player
{
	class ViewModel
	{

		public ViewModel()
        {
			DataModel = new DataModel();
        }

		private DataModel dataModel;

		public DataModel DataModel {
			get { return dataModel; }
			set { dataModel = value; }
		}

    }
}
