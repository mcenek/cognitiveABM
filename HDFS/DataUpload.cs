using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace HDFS{

    public class Upload {

        public static void UploadToHDFS()
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
            uploader.UploadFileToHDFS(csvFileABS + "/output/landscape_exportInfo.csv", hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_2_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "moatGauss_exportInfo.csv");
            uploader.UploadFileToHDFS(csvFileABS + "/output/moatGauss_exportInfo.csv", hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_3_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "grid_exportInfo.csv");
            uploader.UploadFileToHDFS(csvFileABS + "/output/grid_exportInfo.csv", hdfsDir, hdfsFileName, Agent_data);
            // TERRAIN DATA -----------------------------------
            hdfsDir = "/terrains";
            string terrainData = "terrain";
            int terrain_data = (int)jsonData[terrainData];
            terrain_data++;
            jsonData[terrainData] = terrain_data;

            hdfsFileName = "terrain";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape.csv");
            uploader.UploadFileToHDFS(csvFileABS + "/layers/landscape.csv" , hdfsDir, hdfsFileName, terrain_data);

            // REWARD DATA --------------------------------------
            hdfsDir = "/rewards";
            string rewardData = "reward";
            int reward_data = (int)jsonData[rewardData];
            reward_data++;
            jsonData[rewardData] = reward_data;
            
            hdfsFileName = "reward";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape_reward.csv");
            uploader.UploadFileToHDFS(csvFileABS + "/layers/landscape_reward.csv" , hdfsDir, hdfsFileName, reward_data);
            
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
               _hdfsBaseUrl = "http://localhost:9000";
            }
        public async void UploadCsvToHdfs(string csvFilePath, string hdfsDirectory, string hdfsFileName, int fileId){
            try{
                using (var client = new HttpClient()) {
                    client.Timeout = TimeSpan.FromMinutes(5);
                    var url = $"{_hdfsBaseUrl}/webhdfs/v1{hdfsDirectory}/{hdfsFileName}_{fileId}?op=CREATE&overwrite=true";
                    Console.WriteLine("ONE");
                    var request = new HttpRequestMessage(HttpMethod.Put, url);
                    Console.WriteLine("TWO");
                    client.DefaultRequestHeaders.Add("Expect", "100-continue");
                    using (var fileStream = File.OpenRead(csvFilePath))
                    {
                        Console.WriteLine("THREE");
                        request.Content = new StreamContent(fileStream);
                        Console.WriteLine("FOUR");
                        try {
                        var response = await client.SendAsync(request);
                        Console.WriteLine("FIVE");
                        Console.WriteLine($"Response status code: {response.StatusCode}");
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine($"Successfully uploaded {csvFilePath} to {hdfsDirectory}/{hdfsFileName}_{fileId} in HDFS.");
                        } catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending request: {ex.ToString()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading {csvFilePath} to HDFS: {ex.ToString()}");
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