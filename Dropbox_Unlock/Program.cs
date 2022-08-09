using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dropbox.Api;
using Newtonsoft.Json;

namespace DropBox_Unlock
{
    internal class Program
    {
        public class Token
        {
            public string access_token { get; set; }
        }

        static async Task Main(string[] args)
        {
            //------------------------------------------------------

            var Documents_to_be_Unlocked =
                "{\"entries\":" +
                    "[" +
                        "{\"path\":\"/documents/db/main.db\"}," +
                        "{\"path\":\"/documents/db/accounts.db\"}," +
                        "{\"path\":\"/documents/db/contacts.db\"}" +


                    "]" +
                "}";

            //------------------------------------------------------


            //-----------------------------------------------------------
            //--------------  ACCESS TOKEN API  -------------------------
            //-----------------------------------------------------------

            var url = "https://api.dropbox.com/oauth2/token";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Headers["Authorization"] = "Basic ZGVzazNiem52bGpkcnMwOjhiOGZuOGRwbjdpemxmOQ==";
            var data = "grant_type=refresh_token&refresh_token=XivXcSbjU28AAAAAAAAAAR-dXtEx7EhZB4yy4OEOCrvEbQ-E1AEhrFK6dnCFmt81";
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }
            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();

                Token t1 = System.Text.Json.JsonSerializer.Deserialize<Token>(result);

                //Uncomment the below line if Access_Token needs to be viewed in terminal
                //Console.WriteLine("Access Token: {0}", t1.access_token);

                //-----------------------------------------------------------
                //--------------  FILE UNLOCKING API  -------------------------
                //-----------------------------------------------------------

                url = "https://api.dropboxapi.com/2/files/unlock_file_batch";
                httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Method = "POST";
                httpRequest.Headers["Authorization"] = "Bearer "+t1.access_token;
                httpRequest.ContentType = "application/json";
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    streamWriter.Write(Documents_to_be_Unlocked);
                }
                httpResponse = (HttpWebResponse)httpRequest.GetResponse();

                using (var streamReader1 = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader1.ReadToEnd();

                    int Num_Files = Regex.Matches(result, "success").Count;
                    //Console.WriteLine(Num_Files);

                    string matchString = "path_lower";

                    List<int> unlocked_paths_index = new List<int>();

                    matchString = Regex.Escape(matchString);

                    foreach (Match match in Regex.Matches(result, matchString))
                    {
                        unlocked_paths_index.Add(match.Index);
                    }

                    //for (int i = 0; i < Num_Files; i++)
                    //{
                    //    Console.WriteLine(locked_paths_index[i]);
                    //}

                    List<string> unlocked_paths = new List<string>();

                    for (int i = 0; i < Num_Files; i++)
                    {
                        string temp = "";
                        bool check = true;
                        for (int j = unlocked_paths_index[i]; result[j] != ','; j++)
                        {
                            if (check == true)
                            {
                                j += 13;
                                check = false;
                            }
                            else
                            {
                                temp += result[j];
                            }
                        }
                        string final_path = temp.Remove(temp.Length - 1);
                        unlocked_paths.Add(final_path);
                    }

                    Console.WriteLine("Paths Unlocked Successfully:");
                    Console.WriteLine("-------------------------");
                    for (int i = 0; i < Num_Files; i++)
                    {
                        Console.WriteLine(unlocked_paths[i]);
                    }
                }

                //Console.WriteLine(httpResponse.StatusCode);

                //------------------------------------------------------------
            }

            Console.ReadKey();

        }

    }

}
