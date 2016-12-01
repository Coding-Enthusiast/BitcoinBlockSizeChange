using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace BitcoinBlockSizeChange
{
    class Program
    {
        static void Main(string[] args)
        {
            Block rawBlock = Api.GetBlock();
            Statistics stat = new Statistics();
            Serializer.ParseAndDeserialize(rawBlock, ref stat);
            Console.WriteLine(stat.WriteStats());

            Console.ReadLine();
        }
    }
}
