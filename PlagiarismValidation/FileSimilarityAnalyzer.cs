﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace PlagiarismValidation
{
    public class FileSimilarityAnalyzer
    {
        //Global Variables That We Need To Access Many Times : 
        public static List<Entry> entries;
        // <<f1num , f2num>  Entry >
        public static Dictionary<(int, int), Entry> similarityMap;
        // <File Number "vertix" , adj list for vertix > 
        public static Dictionary<int, List<int>> adjacencyList = new Dictionary<int, List<int>>();
        public static List<Component> groups;
        public static List<List<Edge>> spanningTree;
       




        //Reading Files And Related Functions : 
        //--------------------------------------------------
        public static List<Entry> ReadFile(string filePath)
        {
            entries = new List<Entry>();
            string connection = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0;HDR=YES;IMEX=1;'";

            using (OleDbConnection conn = new OleDbConnection(connection))
            {
                conn.Open();
                OleDbCommand command = new OleDbCommand("select * from [Sheet1$]", conn);

                using (OleDbDataReader r = command.ExecuteReader())
                {

                    while (r.Read())
                    {
                        Entry entry = new Entry();
                        entry.F1Name = r[0].ToString();
                        entry.F2Name = r[1].ToString();
                        entry.F1Num = GetFileNum(entry.F1Name);
                        entry.F2Num = GetFileNum(entry.F2Name);
                        entry.F1Sim = TrimPerFromFIleName(entry.F1Name);
                        entry.F2Sim = TrimPerFromFIleName(entry.F2Name);
                        entry.SameLines = int.Parse(r[2].ToString());
                        entries.Add(entry);
                    }
                }
            }
            Initialize(entries);
            return entries;
        }


        static int GetFileNum(string input)
        {
            string num = "";
            int IDX = input.LastIndexOf('/');

            if (IDX != -1)
            {
                for (int i = IDX - 1; i >= 0; i--)
                {
                    if (char.IsDigit(input[i]))
                    {
                        num = input[i] + num;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return int.Parse(num);
        }

        private static double TrimPerFromFIleName(string fileName)
        {
            int firstDigIDX = fileName.LastIndexOf('(') + 1;
            int len = fileName.LastIndexOf('%') - firstDigIDX;
            string sim = fileName.Substring(firstDigIDX, len);
            return double.Parse(sim);
        }


        //Consider this scenario: you have an entry with F1Num = 3 and F2Num = 7.
        //If you simply use (entry.F1Num, entry.F2Num) as the key, it's possible that in another entry you have F1Num = 7 and F2Num = 3,
        //which would result in a different key (7, 3). This could lead to inconsistencies when accessing or updating the dictionary
        //And Also Our Graph Is Undirected So The Same Edge Shouldn't Appear Twice .
        public static void Initialize(List<Entry> entries)
        {
            similarityMap = new Dictionary<(int, int), Entry>();
            foreach (var entry in entries)
            {
                int smallerNum = Math.Min(entry.F1Num, entry.F2Num);
                int largerNum = Math.Max(entry.F1Num, entry.F2Num);

                var key = (smallerNum, largerNum);

                similarityMap[key] = entry;
            }
        }



        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------


        // Constructing Graph Functions.
        public static List<Component> FindGroups()
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
        private static void DepthSearch(int startNode, Dictionary<int, List<int>> adjacencyList, HashSet<int> visited, List<int> component)
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

        public static List<Component> RefineGroups()
        {
           spanningTree = new List<List<Edge>>();
            Sort.MergeSort(groups, component => component.AVGSim);

            foreach (var group in groups)
            {
       
                Dictionary<int, List<int>> adjList = BuildAdjList(group.Vertices);
                List<Edge> mstEdges = FindMST(adjList);
                spanningTree.Add(mstEdges);

                List<int> mstVertices = GetAllVertices(mstEdges);

                group.Vertices = mstVertices;
            }
            // Assuming you have a List<List<Edge>> spanningTree

            // Define a custom key function to get the negative of the line matches for each edge
            Func<Edge, double> getKey = edge => edge.MatchLines;

            // Iterate through each list of edges in spanningTree and sort them based on line matches in descending order
            foreach (var edgesList in spanningTree)
            {
                Sort.MergeSort(edgesList, getKey);
            }


            // Assuming you have a List<List<Edge>> spanningTree and a Dictionary<(int, int), Entry> similarityMap, and a file path to save the Excel file
            string filePath = "path_to_your_excel_file.xlsx";
            ExcelHelper.WriteSpanningTreeToExcel(spanningTree, similarityMap, "C:\\Users\\ahmed\\OneDrive\\Desktop\\Algo_Project\\PlagiarismValidation\\PlagiarismValidation\\Results\\MST.xlsx");

            return groups;
        }






        private static Dictionary<int, List<int>> BuildAdjList(List<int> vertices)
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







        private static List<Edge> FindMST(Dictionary<int, List<int>> adjacencyList)
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
            Sort.MergeSort(edges, new EdgeComparer());


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

        private static bool MakesCycleOrNot(Edge edge, List<Edge> mstEdges, Dictionary<int, int> componentMap)
        {
            int root1 = FindingTheRoot(edge.V1, componentMap);
            int root2 = FindingTheRoot(edge.V2, componentMap);

            return root1 == root2;
        }


  
        private static int FindingTheRoot(int vertex, Dictionary<int, int> componentMap)
        {
            if (componentMap[vertex] != vertex)
            {

                componentMap[vertex] = FindingTheRoot(componentMap[vertex], componentMap);
            }
            return componentMap[vertex];
        }


        public static (double weight, int similarityLines) GetEdgeWeightAndMatchedLines(int vertex1, int vertex2)
        {
            similarityMap.TryGetValue((Math.Min(vertex1, vertex2), Math.Max(vertex1, vertex2)), out var similarityEntry);

            double weight = Math.Max(similarityEntry.F1Sim, similarityEntry.F2Sim);
            int similarityLines = similarityEntry.SameLines;

            return (weight, similarityLines);
        }




        private static List<int> GetAllVertices(List<Edge> mstEdges)
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
