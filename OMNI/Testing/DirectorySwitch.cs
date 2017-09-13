using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMNI.Testing
{
    public class DirectorySwitch
    {
        public static void ListTest()
        {
            var _list = new List<string>();
            var _dir = Directory.GetFiles("\\\\manage2\\server\\Document Center\\Production\\");
            foreach (var s in _dir)
            {
                _list.Add(Path.GetFileNameWithoutExtension(s));
            }
        }
    }
}
