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

                return await FileIO.ReadTextAsync(sampleFile);

                //return File.ReadAllText(Path);

                //return FileIO.Read
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        public static async Task WriteJsonAsync(string JsonToWrite, string fileName)
        {
            // write json to a file
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonToWrite);
            Debug.WriteLine(" + Written to file.");

            //await FileIO.WriteTextAsync(jsonFile, json);

            //using (var stream = await StorageFileObject.OpenStreamForWriteAsync())
            //{
            //    using (var writer = new StreamWriter(stream))
            //    {
            //        await FileIO.WriteTextAsync(StorageFileObject, JsonToWrite);
            //        //writer.Write();
            //        Debug.WriteLine(" + Written cache to file.");
            //    }
            //}
        }
    }
}
