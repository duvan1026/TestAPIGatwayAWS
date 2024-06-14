using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPIGatewayAWS.Models
{
    public class BoundingBox
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }

        /// <summary>
        /// Método para reescalar los valores de la bounding box
        /// </summary>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        public void Rescale(double newWidth, double newHeight)
        {
            // Calcula el factor de escala para el ancho y el alto
            double widthScaleFactor = newWidth / Width;
            double heightScaleFactor = newHeight / Height;

            // Multiplica los valores originales por el factor de escala correspondiente
            Left *= widthScaleFactor;
            Top *= heightScaleFactor;
            Width *= widthScaleFactor;
            Height *= heightScaleFactor;
        }

        /// <summary>
        /// Método para calcular un nuevo BoundingBox que englobe dos BoundingBox
        /// </summary>
        /// <param name="bb1"></param>
        /// <param name="bb2"></param>
        /// <returns></returns>
        public static BoundingBox Unify(BoundingBox bb1, BoundingBox bb2)
        {
            double left = Math.Min(bb1.Left, bb2.Left);
            double top = Math.Min(bb1.Top, bb2.Top);
            double right = Math.Max(bb1.Left + bb1.Width, bb2.Left + bb2.Width);
            double bottom = Math.Max(bb1.Top + bb1.Height, bb2.Top + bb2.Height);

            return new BoundingBox
            {
                Left = left,
                Top = top,
                Width = right - left,
                Height = bottom - top
            };
        }
    }
}
