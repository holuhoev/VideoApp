using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Storage;
using System.IO;

namespace VideoApp.Classes
{
    static class WriteReadVideoModel
    {
        static public async Task<VideoModel[]> ReadFilesInLocalFolder()
        {            
            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            // Get the DataFolder folder.
            var dataFolder = await local.GetFolderAsync("DataFolder");
            var files = await dataFolder.GetFilesAsync();
            List<string> membs = new List<string>();
            
            string[] lines = new string[3];
            string str;// считываемая строка
            foreach (StorageFile file in files)
            {
                str = file.Name;             
                lines = str.Split('$');
                lines[1] = lines[1].Replace("[etot_simvol]", "$");
                str = lines[1];
                if (lines[0].Contains("Data"))
                    membs.Add(str);
            }
            int count = membs.Count;
            List<VideoModel> models = new List<VideoModel>();
            
            byte[] imageBytes;

            for (int i=0;i<count;i++)
            {
                var fileData = await dataFolder.GetFileAsync("DataFile$" + membs[i] + "$.txt");
                using (var fileDataStream = await fileData.OpenStreamForReadAsync())
                {
                    using (StreamReader streamReader = new StreamReader(fileDataStream))
                    {
                        str = streamReader.ReadToEnd();
                    }
                }
                lines = str.Split('$');
                string name = lines[0].Replace("[etot_simvol]", "$");
                string dataCreated = lines[1].Replace("[etot_simvol]", "$");

                // Get the filePath.
                StorageFile filePath = await dataFolder.GetFileAsync("PathFile$" + membs[i] + "$.txt");
                IBuffer buffer = await FileIO.ReadBufferAsync(filePath);
                imageBytes = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(buffer);
                
                
                //If this videoFile not exists , localfiles connected with it will be deleted
                     
                if (await VideoFileExists(name))
                {
                    VideoModel model = new VideoModel() { DateCreated = dataCreated, Name = name, HeadPiece = imageBytes };
                    models.Add(model);
                }
                else
                {
                    await filePath.DeleteAsync();
                    await fileData.DeleteAsync();
                }
            }
            return models.ToArray();
        }
        
        static public async Task<int> FileCountInLocalFolder(string folderName)
        {
            var local = ApplicationData.Current.LocalFolder;
            if (local != null)
            {
                var foldersInLocal = await local.GetFoldersAsync();
                if (foldersInLocal.Count > 0)
                {
                    int count;
                    var folder = await local.GetFolderAsync(folderName);
                    var files = await folder.GetFilesAsync();
                    if (folderName == "DataFolder")
                        count = (int)(files.Count / 2);
                    else
                        count = files.Count;
                    return count;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }
       
        static public async Task WriteVideoModelToFile(VideoModel videoData)
        { 
            
        
            string strData = videoData.ToString();

            // Get the text data from the textbox. 
            byte[] fileBytes = System.Text.Encoding.UTF8.GetBytes(strData.ToCharArray());

            // Get the local folder.
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;

            // Create a new folder name DataFolder.
            var dataFolder = await local.CreateFolderAsync("DataFolder",
                CreationCollisionOption.OpenIfExists);

            // Create a new file named DataFile.txt.
            videoData.Name = videoData.Name.Replace("$", "[etot_simvol]");
            var fileData = await dataFolder.CreateFileAsync("DataFile$" + videoData.Name + "$.txt",
            CreationCollisionOption.ReplaceExisting);
            var filePath = await dataFolder.CreateFileAsync("PathFile$" + videoData.Name + "$.txt",
            CreationCollisionOption.ReplaceExisting);
            
            // Write the data from the textbox.
            using (var s = await fileData.OpenStreamForWriteAsync())
            {
                s.Write(fileBytes, 0, fileBytes.Length);
            }
            using (var s = await filePath.OpenStreamForWriteAsync())
            {

                s.Write(videoData.HeadPiece, 0, videoData.HeadPiece.Length);
            }
        }
       
        /// <summary>
        /// метод проверяет, сущестует ли файл по заданному пути
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<bool> VideoFileExists( string name)
        {
            try
            {
                StorageFolder myVideoLibrary = await Windows.Storage.KnownFolders.PicturesLibrary.GetFolderAsync("ImageConvertor");
                StorageFile videoFile = await myVideoLibrary.GetFileAsync(name +".mp4");
            }
            catch { return false; }
            return true;
        }
    }
}
