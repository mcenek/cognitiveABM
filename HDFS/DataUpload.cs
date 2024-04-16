using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDFS{

    public class Upload {

        public static async void UploadToHDFS()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // JSON ID Data ==================================
            string jsonFilePath = Path.Combine(baseDirectory, "..", "..","..","..","..", "HDFS", "ids.json");
            string jsonString = File.ReadAllText(jsonFilePath);
            JObject jsonData = JObject.Parse(jsonString);

            // connect to hdfs ================================
            HdfsUploader uploader = new HdfsUploader(); 

            // upload =========================================
            // AGENT DATA ------------------------------------
            string hdfsDir = "/agent_data";
            string agentData = "Agent_data";
            int Agent_data = (int)jsonData[agentData];
            Agent_data++;
            jsonData[agentData] = Agent_data;
            
            string csvFileABS = "/home/matttran7/Documents/cognitiveABM/Examples/HillClimberABMExample";

            string hdfsFileName = "Generation_1_data";
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "landscape_exportInfo.csv");
            await uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_2_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "moatGauss_exportInfo.csv");
            await uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_3_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "grid_exportInfo.csv");
            await uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);
            // TERRAIN DATA -----------------------------------
            hdfsDir = "/terrains";
            string terrainData = "terrain";
            int terrain_data = (int)jsonData[terrainData];
            terrain_data++;
            jsonData[terrainData] = terrain_data;

            hdfsFileName = "terrain";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape.csv");
            await uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, terrain_data);

            // REWARD DATA --------------------------------------
            hdfsDir = "/rewards";
            string rewardData = "reward";
            int reward_data = (int)jsonData[rewardData];
            reward_data++;
            jsonData[rewardData] = reward_data;
            
            hdfsFileName = "reward";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape_reward.csv");
            await uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, reward_data);
            
            // write back to json ===============================
            string updatedJsonString = jsonData.ToString();
            File.WriteAllText(jsonFilePath, updatedJsonString);
            }
        } // class HDFS

    public class HdfsUploader
    {
        private readonly string _hdfsBaseUrl;

        public HdfsUploader()
        {
               _hdfsBaseUrl = "http://-----";
            }
        public async Task UploadCsvToHdfs(string csvFilePath, string hdfsDirectory, string hdfsFileName, int fileId){
            string hdfsUrl = "http://-----/webhdfs/v1";
            string fileUrl = $"{hdfsUrl}{hdfsDirectory}/{hdfsFileName}_{fileId}?op=CREATE&overwrite=true";
            Console.WriteLine(fileUrl);

            try
            {
                // read CSV content
                string csvContent = File.ReadAllText(csvFilePath);

                // Upload content
                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(csvContent, Encoding.UTF8, "application/octet-stream");
                    // ! Keep in mind, if you make "response = await client" w/out .Result (Simple: dont use await), it will skip over
                    HttpResponseMessage response = client.PutAsync(fileUrl, content).Result;
                    Console.WriteLine($"Response Status Code: {response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Response Content: {responseContent}");
                        throw new Exception($"Failed to upload file to HDFS. Status code: {response.StatusCode}, Response: {responseContent}");
                    }
                }
                Console.WriteLine("CSV file added successfully to HDFS.");
            }
            catch (HttpRequestException ex){
                Console.WriteLine($"An HTTP request error occurred: {ex.Message}");
            }
            catch (Exception ex){
                Console.WriteLine($"An error occurred: {ex.ToString()}");
            }
        }
//=============================================================================================//
//                                          Write directly from cmd
//=============================================================================================//

public void UploadFileToHDFS(string csvFilePath, string hdfsDirectory, string hdfsFileName, int fileId){
    try
    {
        string command = $"dfs -put {csvFilePath} {hdfsDirectory}/{hdfsFileName}_{fileId}";
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "hdfs",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process { StartInfo = startInfo };
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        Console.WriteLine($"Upload success for {hdfsFileName}_{fileId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error uploading {csvFilePath} to HDFS: {ex.ToString()}");
    }
}

    } // class hdfsuploader
} // name space