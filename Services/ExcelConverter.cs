using System.IO;
using System.Text;
using ClosedXML.Excel;

namespace MarkToPdf.Services
{
    public class ExcelConverter : IDocumentConverter
    {
        public string SupportedExtension => ".xlsx";
        
        public string ConvertToHtml(string filepath) 
        {
            var sb = new StringBuilder();
            sb.Append (@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8' />
                    <style>
                        body {
                            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                            padding: 30px;
                            color: #333;
                        }
                        h2 {
                            color: #1f4e78;
                            border-bottom: 2px solid #1f4e78;
                            padding-bottom: 6px;
                            margin-top: 30px;
                        }
                        table {
                            border-collapse: collapse;
                            width: 100%;
                            margin-bottom: 25px;
                            font-size: 10pt;
                        }
                        th, td {
                            border: 1px solid #d9d9d9;
                            padding: 8px 12px;
                            text-align: left;
                        }
                        th {
                            background-color: #f2f4f7;
                            font-weight: bold;
                            color: #111;
                        }
                        tr:nth-child(even) {
                            background-color: #fcfcfc;
                        }
                    </style>
                </head>
                <body>");
                
            using (var workbook = new XLWorkbook(filepath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    var range = worksheet.RangeUsed();
                     if (range == null) continue;

                     sb.Append($"<h2>{worksheet.Name}</h2>");
                    sb.Append("<table>"); 

                    bool isFirstRow = true;
                    var rows = range.RowsUsed();
                   
                   foreach (var row in rows)

                    {
                     sb.Append("<tr>");   
                     foreach (var cell in row.Cells(1, range.ColumnCount()))
                        {
                            string cellValue = cell.GetFormattedString();
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                cellValue = "&nbsp;";
                            }
                            
                            if (isFirstRow) 
                            {
                                sb.Append($"<th>{cellValue}</th>");

                            }
                            else
                            {
                                sb.Append($"<td>{cellValue}</td>");
                            }

                        }
                        sb.Append("</tr>");
                        isFirstRow = false;


                    }
                    sb.Append("</table>");

                }
            }
            sb.Append("</body></html>");
                return sb.ToString();

        }
    }
}