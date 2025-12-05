using DomainLayer.Dtos;
using OfficeOpenXml;

namespace Commun
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase que construye un excel
    /// </summary>
    public class ReportExcel
    {
        /// <summary>
        /// Gabriela Muñoz
        /// Método que construye un excel en base a la conversacion que llegue
        /// </summary>
        /// <param name="conversations"></param>
        /// <returns></returns>
        public virtual string GenereteExcel(List<ConversationsUserDto> conversations)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Hoja1");

                worksheet.Cells["A1"].Value = "Estado";
                worksheet.Cells["B1"].Value = "Fecha de consulta";
                worksheet.Cells["C1"].Value = "Usuario";
                worksheet.Cells["D1"].Value = "Consulta";


                using (var range = worksheet.Cells["A1:D1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    range.AutoFitColumns();
                }

                int row = 2;
                foreach (var conversation in conversations)
                {
                    WriteContracts(conversation, worksheet, row);
                    row++;
                }

                string outputPathDesktop = Path.Combine(Directory.GetCurrentDirectory(), "Reports");

                if (!Directory.Exists(outputPathDesktop))
                {
                    Directory.CreateDirectory(outputPathDesktop);
                }

                string outputPath = Path.Combine(outputPathDesktop, $"Reporte Conversaciones.xlsx");
                excelPackage.SaveAs(outputPath);

                using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))
                {
                    byte[] fileBytes = new byte[fs.Length];
                    fs.Read(fileBytes, 0, (int)fs.Length);

                    string base64String = Convert.ToBase64String(fileBytes);

                    return base64String;
                }
            }


        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método privado para escribir campos dinamicos de excel
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="worksheet"></param>
        /// <param name="row"></param>
        private void WriteContracts(ConversationsUserDto conversation, ExcelWorksheet worksheet, int row)
        {

            worksheet.Cells[row, 1].Value = conversation.Estado;
            worksheet.Cells[row, 2].Value = conversation.Date;
            worksheet.Cells[row, 3].Value = conversation.UserName;
            worksheet.Cells[row, 4].Value = conversation.Name;
        }
    }
}
