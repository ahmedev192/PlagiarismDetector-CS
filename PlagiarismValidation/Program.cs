using System.Diagnostics;

namespace PlagiarismValidation
{
    internal class Program
    {
        static void Main(string[] args)
        {
             string inputFile = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Sample\\1-Input.xlsx"; 
             string inputFile2 = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Hard\\3-Input.xlsx"; 
             string inputFile3 = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\Complete\\Easy\\1-Input.xlsx"; 
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Read similarity entries from input file
            List<Entry> entries = FileSimilarityAnalyzer.ReadFile(inputFile2);

            // Find groups of similar files
            List<Component> groups = FileSimilarityAnalyzer.FindGroups();

            // Refine groups by constructing MSTs
            groups = FileSimilarityAnalyzer.RefineGroups();
            Console.WriteLine("Final Groups:");
            //foreach (var group in groups)
            //{
            //    Console.WriteLine($"Group Index: {group.IDX}");
            //    Console.WriteLine($"Files: {string.Join(", ", group.Vertices)}");
            //    Console.WriteLine($"Average Similarity: {group.AVGSim}");
            //    Console.WriteLine();
            //}
           
            string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            string resultsFolderPath = Path.Combine(projectDirectory, "Results");
            string fileName = "sorted_groups.xlsx";
            string filePath = Path.Combine(resultsFolderPath, fileName);

            // Call the function to sort and export groups to Excel
            ExcelHelper.ExportToExcel(groups, filePath);


            Console.WriteLine("Analysis complete. Output files generated.");
            // Stop the timer
            stopwatch.Stop();

            // Get the elapsed time
            TimeSpan elapsedTime = stopwatch.Elapsed;

            // Print the elapsed time
            Console.WriteLine($"Total run time: {elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}");


        }


    }
    }