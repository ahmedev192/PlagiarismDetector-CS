using ClosedXML.Excel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace PlagiarismValidation
{
    public class ExcelHelper
    {

        //Reading Files And Related Functions : 
        //--------------------------------------------------
        public static List<Entry> ReadFile(string filePath)
        {
            Stopwatch ReadTime = Stopwatch.StartNew();

            List<Entry> entries = new List<Entry>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.First();

                int rowCnt = worksheet.Dimension.Rows;
                int colCnt = worksheet.Dimension.Columns;

                for (int row = 2; row <= rowCnt; row++) 
                {
                    Entry entry = new Entry();
                    entry.IDX = row - 1;
                    entry.F1Name = worksheet.Cells[row, 1].GetValue<string>();
                    entry.F2Name = worksheet.Cells[row, 2].GetValue<string>();
                    entry.F1Num = GetFileNum(entry.F1Name);
                    entry.F2Num = GetFileNum(entry.F2Name);
                    entry.F1Sim = TrimPerFromFIleName(entry.F1Name);
                    entry.F2Sim = TrimPerFromFIleName(entry.F2Name);
                    entry.SameLines = worksheet.Cells[row, 3].GetValue<int>();
                    entries.Add(entry);
                }
            }
            ReadTime.Stop();
            TimeSpan ElapsedRead = ReadTime.Elapsed;
            Console.WriteLine($"Total Time Taken In Read Data From Input File : {ElapsedRead.Hours:00}:{ElapsedRead.Minutes:00}:{ElapsedRead.Seconds:00}.{ElapsedRead.Milliseconds:000}");

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
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var pack = new ExcelPackage())
        {
            var ws = pack.Workbook.Worksheets.Add("Components");

            ws.Cells[1, 1].Value = "Component Index";
            ws.Cells[1, 2].Value = "Vertices";
            ws.Cells[1, 3].Value = "Average Similarity";
            ws.Cells[1, 4].Value = "Component Count";

            int row = 2;
            foreach (var component in groups)
            {
                ws.Cells[row, 1].Value = row - 1;
                ws.Cells[row, 2].Value = string.Join(", ", component.Vertices);
                ws.Cells[row, 3].Value = component.AVGSim;
                ws.Cells[row, 4].Value = component.VCount;
                    ColWidth(ws, row, component);


                    row++;
            }

            pack.SaveAs(new FileInfo(filePath));
        }

           }





        private static void ColWidth(ExcelWorksheet ws, int rowCount, Component lastComponent)
        {
            for (int col = 1; col <= 4; col++)
            {
                int maxLen = 0;
                for (int row = 1; row <= rowCount; row++)
                {
                    var len = ws.Cells[row, col].Text.Length;
                    if (len > maxLen)
                    {
                        maxLen = len;
                    }
                }
                ws.Column(col).Width = Math.Max(15, maxLen + 2);
            }
        }


        public static void WriteMySpanningTreeToExcel(List<List<Edge>> spanningTree, string filePath)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("MST");

                sheet.Cells[1, 1].Value = "File 1";
                sheet.Cells[1, 2].Value = "File 2";
                sheet.Cells[1, 3].Value = "Line Matches";

                int row = 2;
                foreach (var edgeList in spanningTree)
                {
                    foreach (var edge in edgeList)
                    {
                        var entry = GlobalVariables.similarityMap[(edge.V1, edge.V2)];

                        sheet.Cells[row, 1].Value = entry.F1Name;
                        sheet.Cells[row, 2].Value = entry.F2Name;
                        sheet.Cells[row, 3].Value = entry.SameLines;
                        row++;
                    }
                }

                ColWidth(sheet);

                package.SaveAs(new FileInfo(filePath));
            }
        }

        private static void ColWidth(ExcelWorksheet ws)
        {
            for (int col = 1; col <= 3; col++)
            {
                ws.Column(col).AutoFit();
            }
        }


    }
}