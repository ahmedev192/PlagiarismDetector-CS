using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PlagiarismValidation
{
    public class FileSimilarityAnalyzer
    {
        public static List<SimilarityEntry> entries;
        public static Dictionary<(int, int), SimilarityEntry> similarityMap;
        public static Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
        // Method to initialize the similarity map
        public static void Initialize(List<SimilarityEntry> entries)
        {
            similarityMap = entries.ToDictionary(entry => (Math.Min(entry.File1Number, entry.File2Number), Math.Max(entry.File1Number, entry.File2Number)));
        }



        public static List<SimilarityEntry> ReadSimilarityEntries(string filePath)
        {
            entries = new List<SimilarityEntry>();

            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0;HDR=YES;IMEX=1;'";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                OleDbCommand command = new OleDbCommand("SELECT * FROM [Sheet1$]", connection);

                using (OleDbDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SimilarityEntry entry = new SimilarityEntry();
                        entry.File1Name = reader[0].ToString();
                        entry.File2Name = reader[1].ToString();
                        entry.File1Number = GetNumberBeforeLastSlash(entry.File1Name);
                        entry.File2Number = GetNumberBeforeLastSlash(entry.File2Name);
                        entry.F1Similarity = ParsePercentage(entry.File1Name); 
                        entry.F2Similarity = ParsePercentage(entry.File2Name); 
                        entry.LinesMatched = int.Parse(reader[2].ToString()); 
                        entries.Add(entry);
                    }
                }
            }
           Initialize(entries);
            return entries;
        }

        // Helper function to parse percentage string to double
        private static double ParsePercentage(string percentage)
        {
            // Extract the numeric part of the percentage string
            int startIndex = percentage.LastIndexOf('(') + 1;
            int length = percentage.LastIndexOf('%') - startIndex;
            string numericPart = percentage.Substring(startIndex, length);

            // Convert the numeric part to a double value
            return double.Parse(numericPart) ;
        }




        static int GetNumberBeforeLastSlash(string input)
        {
            string number = "";
            int index = input.LastIndexOf('/');

            if (index != -1)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    if (char.IsDigit(input[i]))
                    {
                        number = input[i] + number;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return int.Parse( number);
        }



        public static List<Component> FindGroups(List<SimilarityEntry> entries)
        {
            // Initialize a dictionary to store adjacent nodes for each file
           

            // Build the adjacency list from similarity entries
            foreach (var entry in entries)
            {
                // Add file1 to file2's adjacent nodes
                if (!adjacencyList.ContainsKey(entry.File1Number))
                    adjacencyList[entry.File1Number] = new List<int>();
                adjacencyList[entry.File1Number].Add(entry.File2Number);

                // Add file2 to file1's adjacent nodes
                if (!adjacencyList.ContainsKey(entry.File2Number))
                    adjacencyList[entry.File2Number] = new List<int>();
                adjacencyList[entry.File2Number].Add(entry.File1Number);
            }

            // Initialize a list to store connected components
            List<Component> groups = new List<Component>();

            // Initialize a set to keep track of visited nodes
            HashSet<int> visited = new HashSet<int>();

            // Perform depth-first search (DFS) to find connected components
            foreach (var node in adjacencyList.Keys)
            {
                if (!visited.Contains(node))
                {
                    Component component = new Component();
                    component.Vertices = new List<int>() ;
                    DFS(node, adjacencyList, visited, component.Vertices);
                    component.Index = groups.Count + 1; // Set group index
                    component.Count = component.Vertices.Count; // Set group size
                    groups.Add(component);
                }
            }

            // Calculate average similarity for each group
            foreach (var group in groups)
            {
                double totalSimilarity = 0, counter = 0;
                foreach (var file1 in group.Vertices)
                {
                    foreach (var file2 in adjacencyList[file1])
                    {

                        
                        // Find the corresponding similarity entry
                        var similarityEntry = entries.FirstOrDefault(e =>
                            (e.File1Number == file1 && e.File2Number == file2) ||
                            (e.File1Number == file2 && e.File2Number == file1));
                        if (similarityEntry != null)
                        {
                            counter++;
                            totalSimilarity = totalSimilarity+ similarityEntry.F1Similarity+ similarityEntry.F2Similarity;
                        }
                    }
                }
                group.AverageSimilarity = Math.Round(totalSimilarity / (counter * 2), 1);
            }

            return groups;
        }

        private static void DFS(int node, Dictionary<int, List<int>> adjacencyList, HashSet<int> visited, List<int> component)
        {
            visited.Add(node);
            component.Add(node);
            if (adjacencyList.ContainsKey(node))
            {
                foreach (var neighbor in adjacencyList[node])
                {
                    if (!visited.Contains(neighbor))
                    {
                        DFS(neighbor, adjacencyList, visited, component);
                    }
                }
            }
        }







        private static List<Edge> FindMST(Dictionary<int, List<int>> adjacencyList)
        {
            List<Edge> edges = new List<Edge>();

            // Step 1: Create a list of all edges in the graph
            foreach (var vertex in adjacencyList.Keys)
            {
                foreach (var neighbor in adjacencyList[vertex])
                {
                    double weight = CalculateEdgeWeight(vertex, neighbor); // Calculate edge weight (optional)
                    edges.Add(new Edge(vertex, neighbor, weight));
                }
            }

            // Step 2: Sort the edges by their weights in non-decreasing order
            Sort.SortList(edges);

            // Step 3: Apply Kruskal's algorithm to find the MST
            List<Edge> mstEdges = new List<Edge>();
            Dictionary<int, int> componentMap = new Dictionary<int, int>();

            foreach (var vertex in adjacencyList.Keys)
            {
                componentMap[vertex] = vertex; // Each vertex is initially its own component
            }

            foreach (var edge in edges)
            {
                int root1 = FindRoot(edge.Vertex1, componentMap);
                int root2 = FindRoot(edge.Vertex2, componentMap);

                if (root1 != root2)
                {
                    mstEdges.Add(edge);
                    componentMap[root1] = root2; // Union operation by updating component map
                }
            }

            return mstEdges;
        }

        // Helper method to find the root of a component using path compression
        private static int FindRoot(int vertex, Dictionary<int, int> componentMap)
        {
            if (componentMap[vertex] != vertex)
            {
                // Path compression: Update the parent of the current vertex
                componentMap[vertex] = FindRoot(componentMap[vertex], componentMap);
            }
            return componentMap[vertex];
        }


        public static double CalculateEdgeWeight(int vertex1, int vertex2)
        {


            // Get the similarity entry based on vertex pair
            if (similarityMap.TryGetValue((Math.Min(vertex1, vertex2), Math.Max(vertex1, vertex2)), out var similarityEntry))
            {
                // Return the maximum similarity between F1 and F2
                return Math.Max(similarityEntry.F1Similarity, similarityEntry.F2Similarity);
            }
            else
            {
                // If no similarity entry found, return a default value or throw an exception based on your requirement
                return 0; // Default value
                          //throw new KeyNotFoundException("Similarity entry not found for the specified vertices.");
            }
        }



        private static List<int> ExtractMSTVertices(List<Edge> mstEdges)
        {
            HashSet<int> vertices = new HashSet<int>();

            // Add vertices from MST edges to the set
            foreach (var edge in mstEdges)
            {
                vertices.Add(edge.Vertex1);
                vertices.Add(edge.Vertex2);
            }

            return vertices.ToList();
        }












        public static List<Component> RefineGroups(List<Component> groups, List<SimilarityEntry> entries)
        {
            foreach (var group in groups)
            {
                // Step 1: Construct adjacency list for the group
               Dictionary<int , List<int>> adjacencyList = BuildAdjacencyList(group.Vertices, entries);

                // Step 2: Construct MST for the group
                List<Edge> mstEdges = FindMST(adjacencyList);

                // Step 3: Extract vertices from MST
                List<int> mstVertices = ExtractMSTVertices(mstEdges);

                // Update vertices of the group to reflect MST vertices
                group.Vertices = mstVertices;
            }

            return groups;
        }




       
        private static Dictionary<int, List<int>> BuildAdjacencyList(List<int> vertices, List<SimilarityEntry> entries)
        {
            Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();

            // Add vertices and their neighbors based on similarity entries
            foreach (var vertex in vertices)
            {
                adjacencyList[vertex] = new List<int>();
                foreach (var entry in entries)
                {
                    if (entry.File1Number == vertex && vertices.Contains(entry.File2Number))
                    {
                        adjacencyList[vertex].Add(entry.File2Number);
                    }
                    else if (entry.File2Number == vertex && vertices.Contains(entry.File1Number))
                    {
                        adjacencyList[vertex].Add(entry.File1Number);
                    }
                }
            }

            return adjacencyList;
        }



    }
}
