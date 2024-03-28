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
            string hdfsDir = "/user/hadoop/agent_data";

            // JSON ID Data ==================================
            string jsonFilePath = Path.Combine(baseDirectory, "..", "..","..","..","..", "HDFS", "ids.json");
            string jsonString = File.ReadAllText(jsonFilePath);
            JObject jsonData = JObject.Parse(jsonString);
            string fileType = "Agent_data";
            int Agent_data = (int)jsonData[fileType];
            Agent_data++;
            jsonData[fileType] = Agent_data;

            // connect to hdfs ================================
            HdfsUploader uploader = new HdfsUploader(); 

            // upload =========================================
            string hdfsFileName = "Generation_1_data";
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "landscape_exportInfo.csv");
            Console.WriteLine("CSV FILE1 >>>>>>> "+csvFile);
            //uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_2_data";
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "moatGauss_exportInfo.csv");
            Console.WriteLine("CSV FILE2 >>>>>>> "+csvFile);
            //uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

            hdfsFileName = "Generation_3_data";
            Console.WriteLine("CSV FILE3 >>>>>>> "+csvFile);
            csvFile = Path.Combine(baseDirectory, "..", "..", "..", "output", "grid_exportInfo.csv");
            //uploader.UploadCsvToHdfs(csvFile, hdfsDir, hdfsFileName, Agent_data);

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
        public async void UploadCsvToHdfs(string csvFilePath, string hdfsDirectory, string hdfsFileName, int fileId)
        {
            try {
                using (var client = new HttpClient()) {

                    var url = $"{this._hdfsBaseUrl}/webhdfs/v1{hdfsDirectory}/{hdfsFileName}_{fileId}?op=CREATE&overwrite=true";
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