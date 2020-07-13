using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace OneDrive_Cloud_Player.Services.Utilities
{
    class JsonHandler
    {
        /// <summary>
        /// Read a JSON file
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        public static async Task<string> ReadJsonAsync(string filePath)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await storageFolder.GetFileAsync(filePath);
                string fileContent = await FileIO.ReadTextAsync(sampleFile);
                Debug.WriteLine(" + Retrieved JSON from a file.");
                return fileContent;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Writes JSON to a file.
        /// </summary>
        /// <param name="JsonToWrite"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task WriteJsonAsync(string JsonToWrite, string fileName)
        {
            // Write json to a file
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonToWrite);
            Debug.WriteLine(" + Written JSON to a file.");
        }
    }
}
