using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace BitcoinBlockSizeChange
{
    public class Api
    {
        public static Block GetBlock()
        {
            Random r = new Random();
            Block bl = new Block();
            int startBlock = 440300;
            bl.BlockHeight = r.Next(startBlock, startBlock + 1000);
            bl.BlockHex = GetRawBlock(bl.BlockHeight).Result;
            return bl;
        }

        private static async Task<string> GetRawBlock(int height)
        {
            using (HttpClient client = new HttpClient())
            {
                string baseUrl = "https://blockexplorer.com/api";

                string blockHash = JObject.Parse(await client.GetStringAsync(baseUrl + "/block-index/" + height))["blockHash"].ToString();

                string result = await client.GetStringAsync(baseUrl + "/rawblock/" + blockHash);

                JObject j = JObject.Parse(result);

                return j["rawblock"].ToString();
            }
        }
    }
}
