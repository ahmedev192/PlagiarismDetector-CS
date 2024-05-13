

using DocumentFormat.OpenXml.EMMA;
using System.Diagnostics;

namespace PlagiarismValidation
{
    public static class GlobalVariables
    {
        // <<f1num , f2num>  Entry >
        public static Dictionary<(int, int), Entry> similarityMap;
    }

    public class FileSimilarityAnalyzer
    {
      
        //Global Variables That We Need To Access Many Times : 
        public List<Entry> entries;

        // <File Number "vertix" , adj list for vertix > 
        public static Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
        public List<Component> groups;
        public List<List<Edge>> spanningTree;
        public static string CaseName = "";

        public FileSimilarityAnalyzer(string caseName , string filePath)
        {
            Stopwatch TotalTime = Stopwatch.StartNew();
            CaseName = caseName;
            entries = ExcelHelper.ReadFile(filePath);
            GlobalVariables.similarityMap = ExcelHelper.InitializeSimDict(entries);
            Stopwatch FindGTimer = Stopwatch.StartNew();
            FindGroups();
            Sort.MGSort(groups, component => component.AVGSim);

            foreach (var component in groups)
            {
                Sort.MGSort(component.Vertices, x => -x);
            }


            FindGTimer.Stop();
            TimeSpan FindGTimerelapsedTime = FindGTimer.Elapsed;
            Console.WriteLine($"Total Time Taken In Find Groups And Generating Stat File : {FindGTimerelapsedTime.Hours:00}:{FindGTimerelapsedTime.Minutes:00}:{FindGTimerelapsedTime.Seconds:00}.{FindGTimerelapsedTime.Milliseconds:000}");

            Stopwatch ExportSTATTimer = Stopwatch.StartNew();
            string STATPath = $"C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\{CaseName}_Stat.xlsx";
            ExcelHelper.ExportStat(groups, STATPath);
            ExportSTATTimer.Stop();
            TimeSpan ExportSTATElapsed = ExportSTATTimer.Elapsed;
            Console.WriteLine($"Total Time Taken In Exporting STAT FILE : {ExportSTATElapsed.Hours:00}:{ExportSTATElapsed.Minutes:00}:{ExportSTATElapsed.Seconds:00}.{ExportSTATElapsed.Milliseconds:000}");



            Stopwatch RefindGTime = Stopwatch.StartNew();
            RefineGroups();
            Func<Edge, double> getKey = edge => edge.MatchLines;
            foreach (var edgesList in spanningTree)
            {
                Sort.MGSort(edgesList, getKey);
            }
            RefindGTime.Stop();
            TimeSpan RefindGTimeelapsedTime = RefindGTime.Elapsed;
            Console.WriteLine($"Total Time Taken In MST & Generating MST FILE CONTENT: {RefindGTimeelapsedTime.Hours:00}:{RefindGTimeelapsedTime.Minutes:00}:{RefindGTimeelapsedTime.Seconds:00}.{RefindGTimeelapsedTime.Milliseconds:000}");


            Stopwatch ExportMSTTimer = Stopwatch.StartNew();
            string SavingPath = $"C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\{CaseName}_MST.xlsx";
            ExcelHelper.WriteMySpanningTreeToExcel(spanningTree, SavingPath);
            ExportSTATTimer.Stop();
            TimeSpan ExportMSTElapsed = ExportSTATTimer.Elapsed;
            Console.WriteLine($"Total Time Taken In Exporting STAT FILE : {ExportMSTElapsed.Hours:00}:{ExportMSTElapsed.Minutes:00}:{ExportMSTElapsed.Seconds:00}.{ExportMSTElapsed.Milliseconds:000}");


            TotalTime.Stop();
            TimeSpan TotalElapsed = TotalTime.Elapsed;
            Console.WriteLine($"Full Project Elapsed Time Including Read All Files From Excel And Write It All : {TotalElapsed.Hours:00}:{TotalElapsed.Minutes:00}:{TotalElapsed.Seconds:00}.{TotalElapsed.Milliseconds:000}");



        }


        





        // Constructing Graph Functions.
        public List<Component> FindGroups()
        {
            //1. This Part , List All Neighbours For Each Veritx 
            foreach (var e in entries)
            {
                if (!adjacencyList.ContainsKey(e.F1Num))
                    adjacencyList[e.F1Num] = new List<int>();
                if (!adjacencyList.ContainsKey(e.F2Num))
                    adjacencyList[e.F2Num] = new List<int>();
                adjacencyList[e.F2Num].Add(e.F1Num);
                adjacencyList[e.F1Num].Add(e.F2Num);

            }

            List<Component> components = new List<Component>();
            HashSet<int> visited = new HashSet<int>();

            foreach (var vertix in adjacencyList.Keys)
            {
                if (!visited.Contains(vertix))
                {
                    Component component = new Component();
                    component.Vertices = new List<int>();
                    DepthSearch(vertix, visited, component.Vertices);
                    component.IDX = components.Count + 1;
                    component.VCount = component.Vertices.Count;
                    components.Add(component);
                }
            }

            foreach (var component in components)
            {
                double totalSimilarity = 0, counter = 0;
                foreach (var file1 in component.Vertices)
                {
                    foreach (var file2 in adjacencyList[file1])
                    {
                        if (GlobalVariables.similarityMap.ContainsKey((file1, file2)))
                        {
                            counter++;
                            totalSimilarity += GlobalVariables.similarityMap[(file1, file2)].F1Sim + GlobalVariables.similarityMap[(file1, file2)].F2Sim;
                        }
                        else if (GlobalVariables.similarityMap.ContainsKey((file2, file1)))
                        {
                            counter++;
                            totalSimilarity += GlobalVariables.similarityMap[(file2, file1)].F1Sim + GlobalVariables.similarityMap[(file2, file1)].F2Sim;
                        }
                    }
                }
                component.AVGSim = Math.Round(totalSimilarity / (counter * 2), 1);
            }
            groups = components;
            return components;
        }


        //2. This Function Takes Each Node And It's Adj , It's Rule Is To Collect All Vertices That Have Relation With Each Others Togetger In a Group And Mark The Visited Vertix;
        private void DepthSearch(int startNode, HashSet<int> visited, List<int> component)
        {
            visited.Add(startNode);
            component.Add(startNode);

            if (adjacencyList.ContainsKey(startNode))
            {
                foreach (var neighbor in adjacencyList[startNode])
                {
                    if (!visited.Contains(neighbor))
                    {
                        DepthSearch(neighbor, visited, component);
                    }
                }
            }
        }





        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------




        // Refinning And MST.

        public List<Component> RefineGroups()
        {
            spanningTree = new List<List<Edge>>();

            foreach (var group in groups)
            {

                Dictionary<int, List<int>> adjList = BuildAdjList(group.Vertices);
                List<Edge> mstEdges = FindMST(adjList);
                spanningTree.Add(mstEdges);

                List<int> mstVertices = GetAllVertices(mstEdges);

                group.Vertices = mstVertices;
            }


            return groups;
        }



        private Dictionary<int, List<int>> BuildAdjList(List<int> vertices)
        {
            Dictionary<int, List<int>> adjList = new Dictionary<int, List<int>>();
            foreach (var vertex in vertices)
            {

                if (adjacencyList.TryGetValue(vertex, out List<int> adjacentVertices))
                {
                    adjList[vertex] = adjacentVertices;
                }
            }

            return adjList;
        }




        private List<Edge> FindMST(Dictionary<int, List<int>> adjacencyList)
        {
            List<Edge> edges = new List<Edge>();
            HashSet<string> edgeSet = new HashSet<string>();

            foreach (var vertex in adjacencyList.Keys)
            {
                foreach (var neighbor in adjacencyList[vertex])
                {
                    var result = GetEdgeWeightAndMatchedLines(vertex, neighbor);
                    double weight = result.weight;
                    int matchedlines = result.similarityLines;


                    string edgeKey = $"{Math.Min(vertex, neighbor)}_{Math.Max(vertex, neighbor)}_{weight}_{matchedlines}";

                    if (!edgeSet.Contains(edgeKey))
                    {
                        edgeSet.Add(edgeKey);
                        edges.Add(new Edge(Math.Min(vertex, neighbor), Math.Max(vertex, neighbor), weight, matchedlines));
                    }

                }
            }

            Sort.MGSort(edges, new EdgeCompare());

            List<Edge> mstEdges = new List<Edge>();
            Dictionary<int, int> componentMapping = new Dictionary<int, int>();

            foreach (var vertex in adjacencyList.Keys)
            {
                componentMapping[vertex] = vertex;
            }

            foreach (var edge in edges)
            {
                int root1 = FindingTheRoot(edge.V1, componentMapping);
                int root2 = FindingTheRoot(edge.V2, componentMapping);

                if (root1 != root2)
                {
                 
                        mstEdges.Add(edge);
                        componentMapping[root1] = root2;
                    
                }
            }

            return mstEdges;
        }





        private int FindingTheRoot(int vertex, Dictionary<int, int> componentMap)
        {
            int root = vertex;
            while (componentMap[root] != root)
            {
                root = componentMap[root];
            }
            return root;
        }


        public (double weight, int similarityLines) GetEdgeWeightAndMatchedLines(int vertex1, int vertex2)
        {
            GlobalVariables.similarityMap.TryGetValue((Math.Min(vertex1, vertex2), Math.Max(vertex1, vertex2)), out var similarityEntry);

            double weight = Math.Max(similarityEntry.F1Sim, similarityEntry.F2Sim);
            int similarityLines = similarityEntry.SameLines;

            return (weight, similarityLines);
        }




        private List<int> GetAllVertices(List<Edge> mstEdges)
        {
            HashSet<int> vertices = new HashSet<int>();

            foreach (var edge in mstEdges)
            {
                vertices.Add(edge.V1);
                vertices.Add(edge.V2);
            }

            return vertices.ToList();
        }







    }
}
