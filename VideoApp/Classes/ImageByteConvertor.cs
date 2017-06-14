using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VideoApp
{
    class ImageByteConvertor
    {
        // converts image to byte[]
        public  async static Task<byte[]> ImageToByteArray(StorageFile file)
        {
           byte[] fileBytes = null;
            // convert file to byte[]
            if (file != null)
            {
                using (Windows.Storage.Streams.IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
                {
                    fileBytes = new byte[stream.Size];
                    using (DataReader reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(fileBytes);
                    }
                }
            }
            return fileBytes;

        }

        //converts byte[] to BitmapImage
        public async static Task<BitmapImage> ByteArrayToImage(byte[] data)
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms))
                {
                    byte[] bitmap = data;
                    writer.WriteBytes(bitmap);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                    writer.DetachStream();
                }
                ms.Seek(0);
                var image = new BitmapImage();
                image.SetSource(ms);
                return image;
            }
        }


    }
}
