using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class Edge
    {
        public int Vertex1 { get; set; }  // Endpoint 1
        public int Vertex2 { get; set; }  // Endpoint 2
        public double Weight { get; set; }   // Weight or cost associated with the edge

        public Edge(int vertex1, int vertex2, double weight)
        {
            Vertex1 = vertex1;
            Vertex2 = vertex2;
            Weight = weight;
        }

    }
}
