﻿

namespace PlagiarismValidation
{
    public class Entry
    {
        public int IDX { get; set; }
        public string F1Name { get; set; }

        public string F2Name { get; set; }
        
        public int F1Num { get; set; }
        
        public int F2Num {  get; set; }
        
        public double F1Sim { get; set; }
        
        public double F2Sim { get; set; }
        
        public int SameLines { get; set; }
    }
}
