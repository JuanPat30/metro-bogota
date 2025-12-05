using DomainLayer.Dtos;
using iText.Layout;
using iText.Kernel.Pdf;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using System.Globalization;
using iText.IO.Font;
using iText.Kernel.Events;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas;
using iText.Html2pdf;
using Commun.Helpers;

namespace Commun
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para generar Pdf
    /// </summary>
    public class PDFGenerator
    {
        /// <summary>
        /// Gabriela Muñoz
        /// Método para generar un pdf con los detalles de la conversacion
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="userId"></param>
        /// <returns>base 64</returns>

        public virtual string GeneratePdf(ConversationDto conversation, string userId)
        {

            string outputPathDesktop = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            string resourcesDesktop = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources");

            if (!Directory.Exists(outputPathDesktop))
            {
                Directory.CreateDirectory(outputPathDesktop);
            }

            string outputPath = System.IO.Path.Combine(outputPathDesktop, $"Conversation {conversation.UuidConversation}.pdf");
            using (PdfWriter writer = new PdfWriter(outputPath))
            {
                using (PdfDocument pdfDoc = new PdfDocument(writer))
                {
                    Document doc = new Document(pdfDoc, PageSize.LETTER);
                    doc.SetMargins(110f, 36f, 7f, 80f);

                    var eventHandler = new HeaderEventHandler();
                    pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, eventHandler);


                    PdfFont font = PdfFontFactory.CreateFont(System.IO.Path.Combine(resourcesDesktop, $"Gotham-Light.otf"), PdfEncodings.IDENTITY_H);
                    PdfFont fontBold = PdfFontFactory.CreateFont(System.IO.Path.Combine(resourcesDesktop, $"Gotham-Bold.otf"), PdfEncodings.IDENTITY_H);

                    Table bodyTableHeader = new Table(1).UseAllAvailableWidth();

                    Image img2 = new Image(ImageDataFactory.Create(System.IO.Path.Combine(resourcesDesktop, $"Logo 3x.png")));
                    bodyTableHeader.AddCell(img2.SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                         .SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER));

                    Table bodyTable = new Table(1);
                    bodyTable.SetWidth(UnitValue.CreatePercentValue(90));

                    Cell clauseCell = new Cell().Add(new Paragraph("Detalle de la consulta").SetFont(fontBold).SetFontSize(20).SetMultipliedLeading(1));
                    clauseCell.SetTextAlignment(TextAlignment.CENTER);
                    clauseCell.SetBorder(Border.NO_BORDER);
                    clauseCell.SetPaddingBottom(20f);
                    bodyTable.AddCell(clauseCell);

                    ContentConversation(conversation, bodyTable, font, fontBold, userId);
                    Details(conversation, bodyTable, font, fontBold);

                    bodyTable.SetMarginBottom(60f);
                    doc.Add(bodyTable);

                }
        }

            using (var fs = new FileStream(outputPath, FileMode.Open, FileAccess.Read))
            {
                byte[] fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, (int)fs.Length);

                string base64String = Convert.ToBase64String(fileBytes);

                return base64String;
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Clase para generar encabezado
        /// </summary>
        public class HeaderEventHandler : IEventHandler
        {
            /// <summary>
            /// Gabriela Muñoz
            /// Método para escribir encabezado
            /// </summary>
            /// <param name="event"></param>
            public void HandleEvent(Event @event)
            {
                string resourcesDesktop = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Resources");
                PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                PdfCanvas pdfCanvas = new PdfCanvas(page);
                Canvas canvas = new Canvas(pdfCanvas, page.GetPageSize());

                // Header
                float headerTableWidth = 500f;
                Table headerTable = new Table(1)
                    .UseAllAvailableWidth()
                    .SetFixedPosition((pdfDoc.GetDefaultPageSize().GetWidth() - headerTableWidth) / 2,
                                        pdfDoc.GetDefaultPageSize().GetTop() - 100,
                                        headerTableWidth);


                Table innerTable = new Table(1).UseAllAvailableWidth();

                Image img2 = new Image(ImageDataFactory.Create(System.IO.Path.Combine(resourcesDesktop, $"Logo 3x.png")))
                    .ScaleAbsolute(2f * 40.50f, 2.49f * 14.15f);
                innerTable.AddCell(new Cell(4, 1).Add(img2.SetHorizontalAlignment(HorizontalAlignment.RIGHT))
                     .SetHorizontalAlignment(HorizontalAlignment.RIGHT).SetBorder(Border.NO_BORDER));

                headerTable.AddCell(new Cell().Add(innerTable).SetBorder(Border.NO_BORDER));

                canvas.Add(headerTable);
                canvas.Close();
                pdfCanvas.Release();
            }
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Metodo privado para escribir información de la conversacion dinamicamente
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="bodyTable"></param>
        /// <param name="font"></param>
        /// <param name="fontBold"></param>
        /// <param name="userId"></param>
        private void ContentConversation(ConversationDto conversation, Table bodyTable, PdfFont font, PdfFont fontBold, string userId)
        {
            TimeZoneInfo zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
            Table innerTable = new Table(3).UseAllAvailableWidth();

            innerTable.AddCell(new Cell().Add(new Paragraph("Estado:").SetFont(fontBold).SetFontSize(12).SetMultipliedLeading(1)).SetBorder(Border.NO_BORDER));
            innerTable.AddCell(new Cell().Add(new Paragraph("Fecha de consulta:").SetFont(fontBold).SetFontSize(12).SetMultipliedLeading(1)).SetBorder(Border.NO_BORDER));
            innerTable.AddCell(new Cell().Add(new Paragraph("Usuario:").SetFont(fontBold).SetFontSize(12).SetMultipliedLeading(1)).SetBorder(Border.NO_BORDER));

            Cell estadoCell = new Cell().SetBorder(Border.NO_BORDER);
            estadoCell.SetPaddingLeft(20f);
            Paragraph estadoText = new Paragraph(conversation.Estado ? "Activo                " : "Inactivo                ")
                .SetFont(font)
                .SetFontSize(12)
                .SetMultipliedLeading(1);
            estadoCell.Add(estadoText);
            estadoCell.SetNextRenderer(new EstadoCircleRenderer(estadoCell, conversation.Estado));
            innerTable.AddCell(estadoCell);

            string dateString = conversation.Date.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(conversation.Date.Value, zonaHoraria).ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) : "Fecha no disponible";
            innerTable.AddCell(new Cell().Add(new Paragraph($"{dateString}            ").SetFont(font).SetFontSize(12).SetMultipliedLeading(1)).SetBorder(Border.NO_BORDER));
            innerTable.AddCell(new Cell().Add(new Paragraph(userId).SetFont(font).SetFontSize(12).SetMultipliedLeading(1)).SetBorder(Border.NO_BORDER));


            Cell innerCell = new Cell().Add(innerTable)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetHorizontalAlignment(HorizontalAlignment.LEFT)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(30f);
            bodyTable.AddCell(innerCell);
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método privado para escribir detalles de la conversacion dinamicamente
        /// </summary>
        /// <param name="conversation"></param>
        /// <param name="bodyTable"></param>
        /// <param name="font"></param>
        /// <param name="fontBold"></param>
        private void Details(ConversationDto conversation, Table bodyTable, PdfFont font, PdfFont fontBold)
        {
            if (conversation.Messages != null)
            {
                conversation.Messages.ForEach(m =>
                {
                    switch (m.IdPersona)
                    {
                        case string s when !s.ToLower().Equals(Constants.ID_CHATBOT):
                            Consult(m, bodyTable, font, fontBold);
                            break;
                        default:
                            Reply(m, bodyTable, font, fontBold);
                            break;
                    }
                });
            }
        }

        private void Consult(MessageChatBotDto m, Table bodyTable, PdfFont font, PdfFont fontBold)
        {
            string messageAsHtml = MarkdownUtils.ToHtml(m.Message);

            // Título "Consulta"
            Cell paragraphCell = new Cell()
                .Add(new Paragraph("Consulta:").SetFont(fontBold).SetFontSize(12).SetMultipliedLeading(1))
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(5f);

            bodyTable.AddCell(paragraphCell);

            // Convertir el HTML a elementos iText
            IList<IElement> elements;
            using (MemoryStream htmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(messageAsHtml)))
            {
                elements = HtmlConverter.ConvertToElements(htmlStream, new ConverterProperties());
            }

            // Crear la celda para el contenido Markdown renderizado
            Cell paragraphCell2 = new Cell()
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(15f);

            // Agregar cada elemento convertido, aplicando la fuente si es necesario
            foreach (IElement elem in elements)
            {
                if (elem is IBlockElement block)
                {
                    if (block is Paragraph p)
                    {
                        p.SetFont(font).SetFontSize(12);
                    }
                    paragraphCell2.Add(block);
                }
            }

            bodyTable.AddCell(paragraphCell2);
        }

        private void Reply(MessageChatBotDto m, Table bodyTable, PdfFont font, PdfFont fontBold)
        {
            string messageAsHtml = MarkdownUtils.ToHtml(m.Message);

            // Título "Respuesta"
            Cell paragraphCell = new Cell()
                .Add(new Paragraph("Respuesta:").SetFont(fontBold).SetFontSize(12).SetMultipliedLeading(1))
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(5f);

            bodyTable.AddCell(paragraphCell);

            // Convertir el HTML a elementos iText
            IList<IElement> elements;
            using (MemoryStream htmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(messageAsHtml)))
            {
                elements = HtmlConverter.ConvertToElements(htmlStream, new ConverterProperties());
            }

            // Crear la celda para el contenido Markdown renderizado
            Cell paragraphCell2 = new Cell()
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetBorder(Border.NO_BORDER)
                .SetPaddingBottom(15f);

            // Agregar cada elemento convertido, aplicando la fuente si es necesario
            foreach (IElement elem in elements)
            {
                if (elem is IBlockElement block)
                {
                    if (block is Paragraph p)
                    {
                        p.SetFont(font).SetFontSize(12);
                    }
                    paragraphCell2.Add(block);
                }
            }

            bodyTable.AddCell(paragraphCell2);
        }

    }
}
