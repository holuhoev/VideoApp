using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoApp.Classes
{
    class VideoModel:IComparable
    {
        public string Name { get; set; }
        public byte[] HeadPiece { get; set; }
        public string DateCreated { get; set; }

        public override string ToString()
        {
            Name= Name.Replace("$", "[etot_simvol]");
            DateCreated =DateCreated.Replace("$", "[etot_simvol]");
            return Name + "$" + DateCreated;
        }
        public int CompareTo(object obj)
        {
            string[] dt = this.DateCreated.Split(' ');
            string[] dt1= dt[0].Split('.'); // день месяц год
            string[] dt2 = dt[1].Split(':');//час минута секунда
            DateTime thisData = new DateTime(int.Parse(dt1[2]), int.Parse(dt1[1]), int.Parse(dt1[0]), int.Parse(dt2[0]), int.Parse(dt2[1]), int.Parse(dt2[2]));

            VideoModel newModel = obj as VideoModel;
            dt = newModel.DateCreated.Split(' ');
            dt1 = dt[0].Split('.'); // день месяц год
            dt2 = dt[1].Split(':');//час минута секунда
            DateTime newData = new DateTime(int.Parse(dt1[2]), int.Parse(dt1[1]), int.Parse(dt1[0]), int.Parse(dt2[0]), int.Parse(dt2[1]), int.Parse(dt2[2]));

            return newData.CompareTo(thisData);
        }
    }
}
