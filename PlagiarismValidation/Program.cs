using ClosedXML.Excel;
using System.Diagnostics;

namespace PlagiarismValidation
{
    internal class Program
    {
        static void Main(string[] args)
        {
             string inputFile = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Easy\\1-Input.xlsx"; 
             string inputFile2 = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Hard\\1-Input.xlsx"; 
             string inputFile3 = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Medium\\1-Input.xlsx"; 
            
            FileSimilarityAnalyzer f = new FileSimilarityAnalyzer(inputFile);
          



            // Define dictionaries to store data from both files
            Dictionary<string, string> file1Data = new Dictionary<string, string>();
            Dictionary<string, string> file2Data = new Dictionary<string, string>();

            // Read data from File 1
            ReadDataFromFile("C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Easy\\1-mst_file.xlsx", file1Data);

            // Read data from File 2
            ReadDataFromFile("C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\MST.xlsx", file2Data);

            // Find columns exist in only the first file
            int counter = 0;
            foreach (var key in file1Data.Keys)
            {
                if (!file2Data.ContainsKey(key) )
                {
                    counter++;
                    Console.WriteLine($"Column '{key}' exists in File 1 but not in File 2");
                }
            }

            Console.WriteLine($"Total columns in File 1 not present in File 2: {counter}");


        }






        static void ReadDataFromFile(string filePath, Dictionary<string, string> data)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1); // Assuming data is in the first worksheet
                var rows = worksheet.RowsUsed();

                foreach (var row in rows)
                {
                    string key = row.Cell(1).Value.ToString();
                    string value = row.Cell(2).Value.ToString();
                    data.Add(key + value, value);
                }
            }
        }

    }
    }