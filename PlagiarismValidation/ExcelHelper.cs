using ClosedXML.Excel;
using System.Data;
using System.Data.OleDb;


namespace PlagiarismValidation
{
    public class ExcelHelper
    {

        //Reading Files And Related Functions : 
        //--------------------------------------------------
        public static List<Entry> ReadFile(string filePath)
        {
             List<Entry> entries = new List<Entry>();
            string connectionSTR = $" Provider=Microsoft.ACE.OLEDB.12.0;Data Source={filePath};Extended Properties='Excel 12.0;HDR=YES;IMEX=1;'";

            using (OleDbConnection conn = new OleDbConnection(connectionSTR))
            {
                conn.Open();
                DataTable tbls = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string firstName = tbls.Rows[0]["TABLE_NAME"].ToString();

                string q = $"SELECT * FROM [{firstName}]";

                OleDbCommand command = new OleDbCommand(q, conn);

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
        public static Dictionary<(int, int), Entry> InitializeSimDict(List<Entry> entries)
        {

              Dictionary<(int, int), Entry> similarityMap = new Dictionary<(int, int), Entry>();
            foreach (var entry in entries)
            {
                int smallerNum = Math.Min(entry.F1Num, entry.F2Num);
                int largerNum = Math.Max(entry.F1Num, entry.F2Num);

                var key = (smallerNum, largerNum);

                similarityMap[key] = entry;
            }
            return similarityMap;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------







        public static void ExportStat(List<Component> groups, string filePath)
        {




            foreach (var component in groups)
            {

                Sort.MGSort(component.Vertices, x => -x);
            }
            using (var workBK = new XLWorkbook())
            {

                var ws = workBK.Worksheets.Add("Components");
                ws.Cell(1, 1).Value = "Component Index";
                ws.Cell(1, 2).Value = "Vertices";
                ws.Cell(1, 3).Value = "Average Similarity";
                ws.Cell(1, 4).Value = "Component Count";

                int row = 2;
                foreach (var component in groups)
                {
                    ws.Cell(row, 1).Value = row - 1;
                    ws.Cell(row, 2).Value = string.Join(", ", component.Vertices);
                    ws.Cell(row, 3).Value = component.AVGSim;
                    ws.Cell(row, 4).Value = component.VCount;
                    row++;
                }

                workBK.SaveAs(filePath);
            }
        }



        public static void WriteMySpanningTreeToExcel(List<List<Edge>> spanningTree, Dictionary<(int, int), Entry> similarityMap, string filePath)
        {
            using (var workBK = new XLWorkbook())
            {
                var sheet = workBK.Worksheets.Add("SpanningTree");

                sheet.Cell(1, 1).Value = "File 1";
                sheet.Cell(1, 2).Value = "File 2";
                sheet.Cell(1, 3).Value = "Line Matches";

                int roww = 2;
                foreach (var edgeList in spanningTree)
                {
                    foreach (var edge in edgeList)
                    {
                        var entry = similarityMap[(edge.V1, edge.V2)];

                        sheet.Cell(roww, 1).Value = entry.F1Name;
                        sheet.Cell(roww, 2).Value = entry.F2Name;
                        sheet.Cell(roww, 3).Value = entry.SameLines;
                        roww++;
                    }
                }

                workBK.SaveAs(filePath);
            }
        }


    }
}