using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout.Renderer;

namespace Commun
{
    /// <summary>
    /// Gabriela Muñoz
    /// Clase para dibujar circulo del estado
    /// </summary>
    public class EstadoCircleRenderer : CellRenderer
    {
        private readonly bool isActive;

        /// <summary>
        /// Gabriela Muñoz
        /// Constructor de la clase
        /// </summary>
        /// <param name="modelElement"></param>
        /// <param name="isActive"></param>
        public EstadoCircleRenderer(Cell modelElement, bool isActive) : base(modelElement)
        {
            this.isActive = isActive;
        }

        /// <summary>
        /// Gabriela Muñoz
        /// Método que dibuja el circulo 
        /// </summary>
        /// <param name="drawContext"></param>
        public override void Draw(DrawContext drawContext)
        {
            base.Draw(drawContext);
            PdfCanvas canvas = drawContext.GetCanvas();

            canvas.SaveState();

            Color circleColor = isActive ? new DeviceRgb(0, 200, 0) : new DeviceRgb(200, 0, 0);

            float x = GetOccupiedAreaBBox().GetLeft() + 10;
            float y = GetOccupiedAreaBBox().GetBottom() + 9;

            canvas.SetFillColor(circleColor)
                  .Circle(x, y, 4)
                  .Fill();

            canvas.RestoreState();
        }
    }
}
