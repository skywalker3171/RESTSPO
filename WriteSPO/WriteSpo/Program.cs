using System;
using System.Net;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WriteSpo
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateClientContextUsingClientIDandClientSecret();
        }

        public static void CreateClientContextUsingClientIDandClientSecret()
        {
            Uri webUri = new Uri("https://uolinc.sharepoint.com/sites/spdevelopersite");

            var SharePointPrincipalId = "00000003-0000-0ff1-ce00-000000000000";
            var token = TokenHelper.GetAppOnlyAccessToken(SharePointPrincipalId, webUri.Authority, null).AccessToken;
            //var ctx = TokenHelper.GetClientContextWithAccessToken(webUri.ToString(), token);

            Uri uri = new Uri("https://uolinc.sharepoint.com/sites/spdevelopersite/");
            String formDigest = "";
            String stringData = @"{ '__metadata': { 'type': 'SP.Data.DummyListListItem' }, 'Title': 'Dummy App3'}";
            using (var client = new HttpClient())
            {
                var content = new StringContent(stringData, Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                using (HttpResponseMessage response = client.PostAsync("https://uolinc.sharepoint.com/sites/spdevelopersite/_api/contextinfo", content).Result)
                {
                    String res = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                        Console.WriteLine("ERROR: SharePoint ListItem Creation Failed!");
                    else
                    {
                        JObject obj = JObject.Parse(res);
                        formDigest = obj["d"]["GetContextWebInformation"]["FormDigestValue"].ToString();
                        Console.WriteLine("FormDigestValue: " + formDigest);
                    }
                }
            }

            using (var client = new HttpClient())
            {
                HttpContent content2 = new StringContent(stringData, Encoding.UTF8, "application/json");
                content2.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Add("Accept", "application/json;odata=verbose");
                client.DefaultRequestHeaders.Add("x-requestdigest", formDigest);
                var result = client.PostAsync("https://uolinc.sharepoint.com/sites/spdevelopersite/_api/web/lists/getbytitle('DummyList')/items", content2).Result;
                string resultContent = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultContent);
            }
        }
    }
}
