using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class Component
    {
        public int Index { get; set; }
        public List<int> Vertices { get; set; }
        public double AverageSimilarity { get; set; }
        public int Count { get; set; }
    }
}
