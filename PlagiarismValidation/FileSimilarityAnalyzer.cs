

namespace PlagiarismValidation
{
    public class FileSimilarityAnalyzer
    {
        //Global Variables That We Need To Access Many Times : 
        public  List<Entry> entries;
        // <<f1num , f2num>  Entry >
        public  Dictionary<(int, int), Entry> similarityMap;
        // <File Number "vertix" , adj list for vertix > 
        public  Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
        public  List<Component> groups;
        public  List<List<Edge>> spanningTree;

        public FileSimilarityAnalyzer(string filePath)
        {
            entries = ExcelHelper.ReadFile(filePath);
            similarityMap = ExcelHelper.InitializeSimDict(entries);
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
            ExcelHelper.WriteMySpanningTreeToExcel(spanningTree, similarityMap, SavingPath );



        }




        // Constructing Graph Functions.
        public  List<Component> FindGroups()
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
                    DepthSearch(vertix, adjacencyList, visited, component.Vertices);
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
                        if (similarityMap.ContainsKey((file1, file2)))
                        {
                            counter++;
                            totalSimilarity += similarityMap[(file1, file2)].F1Sim + similarityMap[(file1, file2)].F2Sim;
                        }
                        else if (similarityMap.ContainsKey((file2, file1)))
                        {
                            counter++;
                            totalSimilarity += similarityMap[(file2, file1)].F1Sim + similarityMap[(file2, file1)].F2Sim;
                        }
                    }
                }
                component.AVGSim = Math.Round(totalSimilarity / (counter * 2), 1);
            }
            groups = components;
            return components;
        }


        //2. This Function Takes Each Node And It's Adj , It's Rule Is To Collect All Vertices That Have Relation With Each Others Togetger In a Group And Mark The Visited Vertix;
        private  void DepthSearch(int startNode, Dictionary<int, List<int>> adjacencyList, HashSet<int> visited, List<int> component)
        {
            Stack<int> stack = new Stack<int>();
            stack.Push(startNode);

            while (stack.Count > 0)
            {
                int node = stack.Pop();
                if (!visited.Contains(node))
                {
                    visited.Add(node);
                    component.Add(node);

                    if (adjacencyList.ContainsKey(node))
                    {
                        foreach (var neighbor in adjacencyList[node])
                        {
                            if (!visited.Contains(neighbor))
                            {
                                stack.Push(neighbor);
                            }
                        }
                    }
                }
            }
        }



        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------




        // Refinning And MST.

        public  List<Component> RefineGroups()
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



        private  Dictionary<int, List<int>> BuildAdjList(List<int> vertices)
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





        private  List<Edge> FindMST(Dictionary<int, List<int>> adjacencyList)
        {
            List<Edge> edges = new List<Edge>();

            foreach (var vertex in adjacencyList.Keys)
            {
                foreach (var neighbor in adjacencyList[vertex])
                {
                    var result = GetEdgeWeightAndMatchedLines(vertex, neighbor);
                    double weight = result.weight;
                    int matchedlines = result.similarityLines;
                    edges.Add(new Edge(Math.Min(vertex, neighbor),Math.Max(vertex, neighbor), weight, matchedlines));
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

                    if (!MakesCycleOrNot(edge, mstEdges, componentMapping))
                    {
                        mstEdges.Add(edge);
                        componentMapping[root1] = root2; 
                    }
                }
            }

            return mstEdges;
        }

        private  bool MakesCycleOrNot(Edge edge, List<Edge> mstEdges, Dictionary<int, int> componentMap)
        {
            int root1 = FindingTheRoot(edge.V1, componentMap);
            int root2 = FindingTheRoot(edge.V2, componentMap);

            return root1 == root2;
        }


  
        private  int FindingTheRoot(int vertex, Dictionary<int, int> componentMap)
        {
            if (componentMap[vertex] != vertex)
            {

                componentMap[vertex] = FindingTheRoot(componentMap[vertex], componentMap);
            }
            return componentMap[vertex];
        }


        public  (double weight, int similarityLines) GetEdgeWeightAndMatchedLines(int vertex1, int vertex2)
        {
            similarityMap.TryGetValue((Math.Min(vertex1, vertex2), Math.Max(vertex1, vertex2)), out var similarityEntry);

            double weight = Math.Max(similarityEntry.F1Sim, similarityEntry.F2Sim);
            int similarityLines = similarityEntry.SameLines;

            return (weight, similarityLines);
        }




        private  List<int> GetAllVertices(List<Edge> mstEdges)
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
