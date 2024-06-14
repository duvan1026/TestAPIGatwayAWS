using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPIGatewayAWS.Models
{
    public class Polygon
    {
        public double X { get; set; }
        public double Y { get; set; }

        // Método para reescalar los valores del polígono
        public void Rescale(double newWidth, double newHeight)
        {
            // Reescala los valores de X y Y proporcionalmente al nuevo ancho y alto
            X *= newWidth;
            Y *= newHeight;
        }
    }
}
