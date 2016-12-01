using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BitcoinBlockSizeChange
{
    public class Statistics
    {
        public int BlockHeight { get; set; }
        public UInt64 TxCount { get; set; }
        public double AverageTxSize { get; set; }
        public int CleanUpSize { get; set; }
        public double CleanUpPercent { get; set; }
        public int AdditionalTx { get; set; }

        private const string MyFile = @"C:\report.txt";

        public string WriteStats()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("Block Height:\t" + BlockHeight);
            result.AppendLine("Tx Count:\t" + TxCount);
            result.AppendLine("Average TxSize:\t" + (int)AverageTxSize + " (bytes)");
            result.AppendLine("Clean Up Size:\t" + CleanUpSize + " (bytes)");
            result.AppendLine(string.Format("Clean Up %:\t{0:#.##}%", CleanUpPercent));
            result.AppendLine("Additional Tx:\t" + AdditionalTx);
            result.AppendLine("====================================");

            File.AppendAllText(MyFile, result.ToString());

            return result.ToString();
        }
    }
}
