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
            FileSimilarityAnalyzer f = new FileSimilarityAnalyzer(inputFile2);
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            Console.WriteLine($"timeee: {elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}");


        }


    }
    }