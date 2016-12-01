using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinBlockSizeChange
{
    /// <summary>
    /// https://bitcoin.org/en/developer-reference#raw-transaction-format
    /// </summary>
    public class BitcoinTransaction
    {
        public UInt32 Version { get; set; }
        public UInt64 TxInCount { get; set; }
        public TxIn[] TxInList { get; set; }
        public UInt64 TxOutCount { get; set; }
        public TxOut[] TxOutList { get; set; }
        public UInt32 LockTime { get; set; }

    }

    /// <summary>
    /// https://bitcoin.org/en/developer-reference#txin
    /// </summary>
    public class TxIn
    {
        public string TxId { get; set; }
        public UInt32 OutIndex { get; set; }

        /// <summary>
        /// ScriptSigLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public int ScriptSigLength { get; set; }
        public string ScriptSig { get; set; }
        public UInt32 Sequence { get; set; }
    }

    /// <summary>
    /// https://bitcoin.org/en/developer-reference#txout
    /// </summary>
    public class TxOut
    {
        public UInt64 Amount { get; set; }

        /// <summary>
        /// PkScriptLength is CompactSize but the max value is 10,000 bytes.
        /// </summary>
        public int PkScriptLength { get; set; }
        public string PkScript { get; set; }
    }
}
