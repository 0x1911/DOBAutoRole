using System.Net.Http;
using System.Threading.Tasks;

namespace DOBAR.Helper.v5API
{
    public class Licenses
    {
        /// <summary>
        /// return a json object as string\r
        /// </summary>
        /// <param name="ApiKey">handed out by a v5 dev</param>
        /// <param name="AuthKey">the auth key we want to receive the attached license info of</param>
        /// <returns>json object as string</returns>
        public async Task<string> GetLicenseInfo(string ApiKey, string AuthKey)
        {
            //did we get valid data in the first place?
            if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(AuthKey))
                return null;


            var responseString = string.Empty;

            try
            {
                var hc = new HttpClient();
                var answer = await hc.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"http://[2a01:4f8:172:201b::10]/discord.php?apiKey={ApiKey}&authKey={AuthKey}"));
                responseString = await answer.Content.ReadAsStringAsync();
            }
            catch(HttpRequestException crap)
            {
                Logger.Error("[GetLicenseInfo] " + crap.Message.ToString());
            }


            return responseString;
        }
    }
}