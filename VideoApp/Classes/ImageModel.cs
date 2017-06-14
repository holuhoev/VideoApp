using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
namespace VideoApp.Classes
{
    class ImageModel:IComparable
    {
        public string DateCreated { get; set; }
        public StorageFile File { get; set; }
        public ImageModel(StorageFile file, string dateCreated)
        {
            File = file;
            DateCreated = dateCreated;
        }

        public int CompareTo(object obj)
        {
            string[] dt = this.DateCreated.Split(' ');
            string[] dt1 = dt[0].Split('.'); // день месяц год
            string[] dt2 = dt[1].Split(':');//час минута секунда
            DateTime thisData = new DateTime(int.Parse(dt1[2]), int.Parse(dt1[1]), int.Parse(dt1[0]), int.Parse(dt2[0]), int.Parse(dt2[1]), int.Parse(dt2[2]));

            ImageModel newModel = obj as ImageModel;
            dt = newModel.DateCreated.Split(' ');
            dt1 = dt[0].Split('.'); // день месяц год
            dt2 = dt[1].Split(':');//час минута секунда
            DateTime newData = new DateTime(int.Parse(dt1[2]), int.Parse(dt1[1]), int.Parse(dt1[0]), int.Parse(dt2[0]), int.Parse(dt2[1]), int.Parse(dt2[2]));

            return newData.CompareTo(thisData);
        }

    }
}
