using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class SimilarityEntry
    {
        public string File1Name { get; set; }
        public string File2Name { get; set; }
        public int File1Number { get; set; }
        public int File2Number {  get; set; }
        public double F1Similarity { get; set; }
        public double F2Similarity { get; set; }
        public int LinesMatched { get; set; }
    }
}
