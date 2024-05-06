using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlagiarismValidation
{
    public class ExcelHelper
    {
        public static void ExportToExcel(List<Component> groups, string filePath)
        {




            foreach (var component in groups)
            {

                Sort.MergeSort(component.Vertices, x => -x);
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



        public static void WriteSpanningTreeToExcel(List<List<Edge>> spanningTree, Dictionary<(int, int), Entry> similarityMap, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SpanningTree");

                worksheet.Cell(1, 1).Value = "File 1";
                worksheet.Cell(1, 2).Value = "File 2";
                worksheet.Cell(1, 3).Value = "Line Matches";

                int row = 2;
                foreach (var edgeList in spanningTree)
                {
                    foreach (var edge in edgeList)
                    {
                        var entry = similarityMap[(edge.V1, edge.V2)];
                        
                        worksheet.Cell(row, 1).Value = entry.F1Name;
                        worksheet.Cell(row, 2).Value = entry.F2Name;
                        worksheet.Cell(row, 3).Value = entry.SameLines;
                        row++;
                    }
                }

                workbook.SaveAs(filePath);
            }
        }






    }
}