using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class Component
    {
        public int IDX { get; set; }

        public List<int> Vertices { get; set; }

        public double AVGSim { get; set; }

        public int VCount { get; set; }

     
    }
}
