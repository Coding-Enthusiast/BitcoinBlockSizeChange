using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinBlockSizeChange
{
    public class Serializer
    {
        private static double AverageSize = 0;

        public static void ParseAndDeserialize(Block block, ref Statistics stat)
        {
            // 4 + 32 + 32 + 4 + 4 + 4
            int blockHeadSize = (4 + 32 + 32 + 4 + 4 + 4) * 2;
            int index = blockHeadSize;
            UInt64 txn_count = ReadCompactSize(block.BlockHex, ref index);

            List<BitcoinTransaction> txList = Deserialize(block.BlockHex, ref index, txn_count);

            StringBuilder newHash = new StringBuilder();

            newHash.Append(block.BlockHex.Substring(0, blockHeadSize));
            foreach (var item in txList)
            {
                newHash.Append(CreateNewRawTx(item));
            }

            byte[] ba1 = HexToByteArray(block.BlockHex);
            byte[] ba2 = HexToByteArray(newHash.ToString());
            var diff = ba1.Length - ba2.Length;

            stat.BlockHeight = block.BlockHeight;
            stat.TxCount = txn_count;
            stat.AverageTxSize = AverageSize;
            stat.CleanUpSize = diff;
            stat.CleanUpPercent = ((double)diff / ba1.Length) * 100;
            stat.AdditionalTx = (int)(diff / AverageSize);
        }

        static List<BitcoinTransaction> Deserialize(string raw, ref int index, UInt64 loopCount)
        {
            List<BitcoinTransaction> result = new List<BitcoinTransaction>();
            List<int> txSize = new List<int>();
            for (UInt64 j = 0; j < loopCount; j++)
            {
                BitcoinTransaction btx = new BitcoinTransaction();
                int size = index;

                // 1) 4 byte - version
                string version = raw.Substring(index, 8);
                btx.Version = (UInt32)HexToUInt(version);
                index += 8;

                // 2) ? byte - tx_in count (CompactSize uint)
                btx.TxInCount = ReadCompactSize(raw, ref index);

                // Initialize the array
                btx.TxInList = new TxIn[btx.TxInCount];
                for (UInt64 i = 0; i < btx.TxInCount; i++)
                {
                    TxIn temp = new TxIn();
                    // 3) 32 byte - TX hash (reverse)
                    temp.TxId = raw.Substring(index, 64);
                    temp.TxId = ReverseTx(temp.TxId);
                    index += 64;

                    // 4) 4 byte - output Index
                    string outIndex = raw.Substring(index, 8);
                    temp.OutIndex = (UInt32)HexToUInt(outIndex);
                    index += 8;

                    // 5) ? byte - scriptSig length (CompactSize uint) (Maximum value is 10,000 bytes)
                    string scriptSigLength = raw.Substring(index, 2);
                    temp.ScriptSigLength = (int)ReadCompactSize(raw, ref index);

                    // 6) ? byte - scriptSig or a placeholder for unsigned (can be empty too)
                    temp.ScriptSig = raw.Substring(index, temp.ScriptSigLength * 2);
                    index += temp.ScriptSigLength * 2;

                    //7) 4 byte - sequence - max is 0xffffffff - can change for RBF transactions
                    string sequence = raw.Substring(index, 8);
                    temp.Sequence = (UInt32)HexToUInt(sequence);
                    index += 8;

                    btx.TxInList[i] = temp;
                }

                //8) ? byte - tx_out count (compactSize uint)
                btx.TxOutCount = ReadCompactSize(raw, ref index);

                // Initialize the array
                btx.TxOutList = new TxOut[btx.TxOutCount];
                for (UInt64 i = 0; i < btx.TxOutCount; i++)
                {
                    TxOut temp = new TxOut();

                    //9) 8 byte - amout to transfer
                    string amount = raw.Substring(index, 16);
                    temp.Amount = HexToUInt(amount);
                    index += 16;

                    //10) ? byte - pk_script length (compactSize uint)
                    string pkScriptLength = raw.Substring(index, 2);
                    temp.PkScriptLength = (Int32)HexToUInt(pkScriptLength);
                    index += 2;

                    //11) ? byte - pk_script 
                    temp.PkScript = raw.Substring(index, temp.PkScriptLength * 2);
                    index += temp.PkScriptLength * 2;

                    btx.TxOutList[i] = temp;
                }

                //12) 4 byte - lock time
                string lockTime = raw.Substring(index, 8);
                btx.LockTime = (UInt32)HexToUInt(lockTime);
                index += 8;

                result.Add(btx);

                txSize.Add((index - size) / 2);
            }
            AverageSize = txSize.Average();
            return result;
        }

        public static string CreateNewRawTx(BitcoinTransaction btx)
        {
            StringBuilder rawTx = new StringBuilder();

            // 1) 4 byte - version 
            //    *Now CompactSize
            UInt32 version = btx.Version;
            rawTx.Append(MakeCompactSize(version));

            // 2) ? byte - tx_in count (compactSize uint)
            rawTx.Append(MakeCompactSize(btx.TxInCount));

            for (UInt64 i = 0; i < btx.TxInCount; i++)
            {
                // 3) 32 byte - TX hash (reverse)
                string txToSend = btx.TxInList[i].TxId;
                rawTx.Append(ReverseTx(txToSend));

                // 4) 4 byte - output Index
                //    *Now CompactSize
                rawTx.Append(MakeCompactSize(btx.TxInList[i].OutIndex));

                // 5) ? byte - scriptSig length (compactSize uint) (Maximum value is 10,000 bytes)
                rawTx.Append(MakeCompactSize((UInt64)btx.TxInList[i].ScriptSigLength));

                // 6) ? byte - scriptSig which is filled with scriptPubkey temporarily (20 is half the length of address hash160)
                rawTx.Append(btx.TxInList[i].ScriptSig);

                //7) 4 byte - sequence - max is 0xffffffff - can change for RBF transactions
                UInt32 sequence = UInt32.MaxValue;
                rawTx.Append(IntToHex(sequence, 4));
            }

            //8) ? byte - tx_out count (compactSize uint)
            rawTx.Append(MakeCompactSize(btx.TxOutCount));

            for (UInt64 i = 0; i < btx.TxOutCount; i++)
            {
                //9) 8 byte - amout to transfer
                rawTx.Append(MakeCompactSize(btx.TxOutList[i].Amount));

                //10) ? byte - pk_script length (compactSize uint)
                rawTx.Append(MakeCompactSize((UInt64)btx.TxOutList[i].PkScriptLength));

                //11) ? byte - pk_script 
                rawTx.Append(btx.TxOutList[i].PkScript);
            }

            //12) 4 byte - lock time
            //    *Now CompactSize
            rawTx.Append(MakeCompactSize(btx.LockTime));

            return rawTx.ToString();
        }


        private static string ReverseTx(string hex)
        {
            // Convert to byte[] and reverse
            byte[] ba = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                ba[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            // Reverse and Convert back to hex.
            Array.Reverse(ba);
            string result = BitConverter.ToString(ba);
            return result.Replace("-", "").ToLower();
        }
        public static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static UInt64 HexToUInt(string hex)
        {
            byte[] ba = HexToByteArray(hex);
            if (!BitConverter.IsLittleEndian)
            {
                ba = ba.Reverse().ToArray();
            }
            // Prevent out of range exception by adding zeros.
            if (ba.Length < 8)
            {
                ba = ba.Concat(new byte[8 - ba.Length]).ToArray();
            }

            return BitConverter.ToUInt64(ba, 0);
        }
        public static string IntToHex(UInt64 iValue, int bLength)
        {
            byte[] b = BitConverter.GetBytes(iValue);
            // Prevent out of range exception by adding zeros.
            if (b.Length < bLength)
            {
                b = b.Concat(new byte[bLength - b.Length]).ToArray();
            }
            // Make sure byte array is BigEndian
            if (!BitConverter.IsLittleEndian)
            {
                b = b.Reverse().ToArray();
            }
            StringBuilder hex = new StringBuilder();
            for (int i = 0; i < bLength; i++)
            {
                hex.Append(b[i].ToString("x2"));
            }
            return hex.ToString();
        }
        public static UInt64 ReadCompactSize(string hex, ref int i)
        {
            string firstByte = hex.Substring(i, 2);
            string remainingBytes;
            byte firstInt = (byte)HexToUInt(firstByte);
            UInt64 result = 0;
            if (firstInt <= 252) // UInt8 (1 byte)
            {
                result = HexToUInt(firstByte);
                i = i + 2;
            }
            else if (firstInt == 253) // 0xfd followed by the number as UInt16 (2 byte)
            {
                remainingBytes = hex.Substring(i + 2, 4);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 4;
            }
            else if (firstInt == 254) // 0xfe followed by the number as UInt32 (4 byte)
            {
                remainingBytes = hex.Substring(i + 2, 8);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 8;
            }
            else if (firstInt == 255) // 0xff followed by the number as UInt64 (8 byte)
            {
                remainingBytes = hex.Substring(i + 2, 16);
                result = HexToUInt(remainingBytes);
                i = i + 2 + 16;
            }

            return result;
        }
        public static string MakeCompactSize(UInt64 num)
        {
            string result = string.Empty;
            if (num <= 252) // 1 Byte
            {
                result = IntToHex(num, 1);
            }
            else if (num <= 0xffff) // 1 + 2 Byte
            {
                result = "fd" + IntToHex(num, 2);
            }
            else if (num <= 0xffffffff) // 1 + 4 Byte
            {
                result = "fe" + IntToHex(num, 4);
            }
            else // < 0xffffffffffffffff // 1 + 8 Byte
            {
                result = "ff" + IntToHex(num, 8);
            }

            return result;
        }
    }
}
