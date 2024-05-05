using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class Sort
    {


        public static void SortList(List<Edge> edges)
        {
            MergeSort(edges, 0, edges.Count - 1);
        }

        private static void MergeSort(List<Edge> edges, int start, int end)
        {
            if (start < end)
            {
                int mid = (start + end) / 2;
                MergeSort(edges, start, mid);
                MergeSort(edges, mid + 1, end);
                Merge(edges, start, mid, end);
            }
        }


        private static void Merge(List<Edge> edges, int start, int mid, int end)
        {
            int n1 = mid - start + 1;
            int n2 = end - mid;
            List<Edge> leftArray = new List<Edge>(n1 + 1); // Adding 1 extra for infinity
            List<Edge> rightArray = new List<Edge>(n2 + 1); // Adding 1 extra for infinity

            for (int i = 0; i < n1; i++)
                leftArray.Add(edges[start + i]);
            leftArray.Add(new Edge(0, 0, double.PositiveInfinity)); // Infinity

            for (int j = 0; j < n2; j++)
                rightArray.Add(edges[mid + 1 + j]);
            rightArray.Add(new Edge(0, 0, double.PositiveInfinity)); // Infinity

            int leftIndex = 0, rightIndex = 0;

            for (int i = start; i <= end; i++)
            {
                if (leftArray[leftIndex].Weight <= rightArray[rightIndex].Weight)
                {
                    edges[i] = leftArray[leftIndex++];
                }
                else
                {
                    edges[i] = rightArray[rightIndex++];
                }
            }
        }


    }
}
