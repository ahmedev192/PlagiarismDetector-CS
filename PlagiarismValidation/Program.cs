namespace PlagiarismValidation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            string inputFile = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\Test Cases\\sample\\1-Input.xlsx"; // Update with your input file path

            // Read similarity entries from input file
            List<SimilarityEntry> entries = FileSimilarityAnalyzer.ReadSimilarityEntries(inputFile);

            // Find groups of similar files
            List<Component> groups = FileSimilarityAnalyzer.FindGroups(entries);

            // Refine groups by constructing MSTs
            groups = FileSimilarityAnalyzer.RefineGroups(groups, entries);
            Console.WriteLine("Final Groups:");
            foreach (var group in groups)
            {
                Console.WriteLine($"Group Index: {group.Index}");
                Console.WriteLine($"Files: {string.Join(", ", group.Vertices)}");
                Console.WriteLine($"Average Similarity: {group.AverageSimilarity}");
                Console.WriteLine();
            }

            Console.WriteLine("Analysis complete. Output files generated.");

        }


    }
    }