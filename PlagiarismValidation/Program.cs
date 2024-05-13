using ClosedXML.Excel;
using System.Diagnostics;

namespace PlagiarismValidation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[,] testCases = new string[,]
      {
            { "Sample 1", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Sample\\1-Input.xlsx" },
            { "Sample 2", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Sample\\2-Input.xlsx" },
            { "Sample 3", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Sample\\3-Input.xlsx" },
            { "Easy 1", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Easy\\1-Input.xlsx" },
            { "Easy 2", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Easy\\2-Input.xlsx" },
            { "Medium 1", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Medium\\1-Input.xlsx" },
            { "Medium 2", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Medium\\2-Input.xlsx" },
            { "Hard 1", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Hard\\1-Input.xlsx" },
            { "Hard 2", "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Hard\\2-Input.xlsx" }
      };


            for (int i = 0; i < testCases.GetLength(0); i++)
            {
                string testCaseName = testCases[i, 0];
                string filepath = testCases[i, 1];

                Console.WriteLine($"Running Test Case: {testCaseName}");
                FileSimilarityAnalyzer Analyzer = new FileSimilarityAnalyzer(testCaseName, filepath);
              
                Console.WriteLine();
                GlobalVariables.similarityMap = new Dictionary<(int, int), Entry>();
                FileSimilarityAnalyzer.adjacencyList = new Dictionary<int, List<int>>();

            }

            Console.WriteLine("All test cases executed.");
        }

    }








}
