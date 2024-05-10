

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

        public FileSimilarityAnalyzer(string filePath)
        {
            entries = ExcelHelper.ReadFile(filePath);
            GlobalVariables.similarityMap = ExcelHelper.InitializeSimDict(entries);
            Stopwatch stopwatch = Stopwatch.StartNew();

            FindGroups();
            Sort.MGSort(groups, component => component.AVGSim);
            RefineGroups();
            Func<Edge, double> getKey = edge => edge.MatchLines;
            foreach (var edgesList in spanningTree)
            {
                Sort.MGSort(edgesList, getKey);
            }
            string STATPath = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\Stat.xlsx";
            ExcelHelper.ExportStat(groups, STATPath);
            string SavingPath = "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\MST.xlsx";
            ExcelHelper.WriteMySpanningTreeToExcel(spanningTree, SavingPath);
            stopwatch.Stop();
            TimeSpan elapsedTime = stopwatch.Elapsed;
            Console.WriteLine($"timeee: {elapsedTime.Hours:00}:{elapsedTime.Minutes:00}:{elapsedTime.Seconds:00}.{elapsedTime.Milliseconds:000}");



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
                    DepthSearch(-1, vertix, visited, component);
                    component.IDX = components.Count + 1;
                    component.VCount = component.Vertices.Count;
                    components.Add(component);
                }
            }

            foreach (var component in components)
            {

                int edgeCount = component.Vertices.Sum(v => adjacencyList[v].Count);

                component.AVGSim = Math.Round(component.AVGSim / (edgeCount * 2), 1);
            }
            groups = components;
            return components;
        }


        //2. This Function Takes Each Node And It's Adj , It's Rule Is To Collect All Vertices That Have Relation With Each Others Togetger In a Group And Mark The Visited Vertix;
        private static void DepthSearch(int parent, int node, HashSet<int> visited, Component component)
        {
            visited.Add(node);
            component.Vertices.Add(node);
            if (adjacencyList.ContainsKey(node))
            {
                foreach (var neighbor in adjacencyList[node])
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (parent != -1 && GlobalVariables.similarityMap.ContainsKey((parent, neighbor)))
                        {
                            component.AVGSim += GlobalVariables.similarityMap[(parent, neighbor)].F1Sim + GlobalVariables.similarityMap[(parent, neighbor)].F2Sim;


                        }
                        DepthSearch(node, neighbor, visited, component);
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

                    // Generate a unique identifier for the edge
                    string edgeKey = $"{Math.Min(vertex, neighbor)}_{Math.Max(vertex, neighbor)}_{weight}_{matchedlines}";

                    // Check if the edge is already in the set
                    if (!edgeSet.Contains(edgeKey))
                    {
                        // If not, add it to the set and to the list of edges
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
                    if (!MakesCycleOrNot(edge, componentMapping))
                    {
                        mstEdges.Add(edge);
                        componentMapping[root1] = root2;
                    }
                }
            }

            return mstEdges;
        }


        private bool MakesCycleOrNot(Edge edge, Dictionary<int, int> componentMap)
        {
            int root1 = FindingTheRoot(edge.V1, componentMap);
            int root2 = FindingTheRoot(edge.V2, componentMap);

            return root1 == root2;
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
