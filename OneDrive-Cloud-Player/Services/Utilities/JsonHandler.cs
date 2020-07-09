using System.IO;

namespace OneDrive_Cloud_Player.Services.Utilities
{
    class JsonHandler
	{
		/// <summary>
		/// Read a JSON file
		/// </summary>
		/// <param name="Path"></param>
		/// <returns></returns>
		public static string ReadJson(string Path)
		{
			try
			{
				return File.ReadAllText(Path);
			}
			catch(FileNotFoundException)
			{
				return null;
			}
		}

		public static void WriteJson(string JsonToWrite, string Path)
		{
			File.WriteAllText(Path, JsonToWrite);
		}
	}
}
