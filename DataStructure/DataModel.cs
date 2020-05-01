using System;
using System.Collections.Generic;
using System.Text;

namespace OneDrive_Cloud_Player.DataStructure
{
	class DataModel
	{
		private AppConfiguration settings;
		private CurrentDirectory currentDirectory;
		private Profile profile;

		public AppConfiguration Settings {
			get { return settings; }
			set { settings = value; }
		}

		public CurrentDirectory CurrentDirectory {
			get { return currentDirectory; }
			set { currentDirectory = value; }
		}

		public Profile Profile {
			get { return profile; }
			set { profile = value; }
		}


	}
}
