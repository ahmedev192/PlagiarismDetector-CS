using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class Edge
    {
        public int V1 { get; set; }  
        public int V2 { get; set; }  
        public double EdgeWeight { get; set; }   
        public int MatchLines {  get; set; }

        public Edge(int v1, int v2, double w, int matchedLines)
        {
            V1 = v1;
            V2 = v2;
            EdgeWeight = w;
            MatchLines = matchedLines;
        }



    }

    public class EdgeComparer : IComparer<Edge>
    {
        public int Compare(Edge x, Edge y)
        {
            // First, compare by EdgeWeight
            int weightComparison = x.EdgeWeight.CompareTo(y.EdgeWeight);

            // If weights are equal, compare by the greatest number of matching lines
            if (weightComparison == 0)
            {
                return y.MatchLines.CompareTo(x.MatchLines);
            }

            return weightComparison;
        }
    }

   

}
