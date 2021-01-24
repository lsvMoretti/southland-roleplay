using System.IO;
using System.Text;
using AltV.Net;

namespace Server.Extensions
{
    public class Writer : TextWriter
    {
        public override void Write(string value)
        {
            Alt.Log(value);
            return;
        }

        public override Encoding Encoding => Encoding.ASCII;
    }
}