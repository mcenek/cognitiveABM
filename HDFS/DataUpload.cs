using System;
using System.IO;
using System.Net.Http;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;

namespace HDFS{

    public class upload {

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
            string hdfsDir = "/user/hadoop/agent_data";
            string agentData = "Agent_data";
            int Agent_data = (int)jsonData[agentData];
            Agent_data++;
            jsonData[agentData] = Agent_data;
            
            string hdfsFileName = "Generation_1_data";
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "landscape_exportInfo.csv");
            uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_2_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "moatGauss_exportInfo.csv");
            uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_3_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "grid_exportInfo.csv");
            uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);
            // TERRAIN DATA -----------------------------------
            string hdfsDir = "/user/hadoop/terrains";
            string terrainData = "terrain";
            int terrain_data = (int)jsonData[terrainData];
            terrain_data++;
            jsonData[terrainData] = terrain_data;

            hdfsFileName = "terrain";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape.csv");
            uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, terrain_data);

            // REWARD DATA --------------------------------------
            string hdfsDir = "/user/hadoop/rewards";
            string rewardData = "reward";
            int reward_data = (int)jsonData[rewardData];
            reward_data++;
            jsonData[rewardData] = reward_data;
            
            hdfsFileName = "reward";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape_reward.csv");
            uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, terrain_data);
            
            // write back to json ===============================
            string updatedJsonString = jsonData.ToString();
            File.WriteAllText(jsonFilePath, updatedJsonString);
            }
        } // class HDFS
p
    public class HdfsUploader
    {
        private readonly string _hdfsBaseUrl;

        public HdfsUploader()
        {
               _hdfsBaseUrl = "http://localhost:9000";
            }
        public async void UploadCsvToHdfs(string csvFilePath, string hdfsDirectory, string hdfsFileName, int fileId)
        {
            try {
                using (var client = new HttpClient()) {

                    var url = $"{this._hdfsBaseUrl}/webhdfs/v1{hdfsDirectory}/{hdfsFileName}_ID{fileId}?op=CREATE&overwrite=true";
                    var request = new HttpRequestMessage(HttpMethod.Put, url);

                    using (var fileStream = File.OpenRead(csvFilePath))
                    {
                        request.Content = new StreamContent(fileStream);
                        var response = await client.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        Console.WriteLine($"Successfully uploaded {csvFilePath} to {hdfsDirectory}/{hdfsFileName}_{fileId} in HDFS.");
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error uploading {csvFilePath} to HDFS: {ex.Message}");
            }
        }
    } // class hdfsuploader

} // name space