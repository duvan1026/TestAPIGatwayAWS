using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestAPIGatewayAWS.Program;

namespace TestAPIGatewayAWS.Models
{
    public class Geometry
    {
        public BoundingBox BoundingBox { get; set; }
        public List<Polygon> Polygon { get; set; }

        /// <summary>
        /// Método para verificar si la geometría contiene datos significativos
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return BoundingBox.Width > 0 &&
                   BoundingBox.Height > 0 &&
                   BoundingBox.Left >= 0 &&
                   BoundingBox.Top >= 0;
        }


        /// <summary>
        /// Método estático para copiar y reescalar la geometría de la página
        /// </summary>
        /// <param name="originalGeometry"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Geometry CopyAndRescaleGeometry(Geometry originalGeometry, double _newWidth, double _newHeight)
        {
            Geometry copiedGeometry = new Geometry();

            // Copiar la caja de delimitación (BoundingBox)
            copiedGeometry.BoundingBox = new BoundingBox
            {
                Width = originalGeometry.BoundingBox.Width * _newWidth,
                Height = originalGeometry.BoundingBox.Height * _newHeight,
                Left = originalGeometry.BoundingBox.Left * _newWidth,
                Top = originalGeometry.BoundingBox.Top * _newHeight
            };

            // Copiar y reescalar los polígonos (Polygons)
            copiedGeometry.Polygon = new List<Polygon>();
            foreach (var originalPolygon in originalGeometry.Polygon)
            {
                Polygon copiedPolygon = new Polygon
                {
                    X = originalPolygon.X * _newWidth,
                    Y = originalPolygon.Y * _newHeight
                };

                copiedGeometry.Polygon.Add(copiedPolygon);
            }

            return copiedGeometry;
        }

        /// <summary>
        /// Método para unificar dos geometrías
        /// </summary>
        /// <param name="geo1"></param>
        /// <param name="geo2"></param>
        /// <returns></returns>
        public static Geometry Unify(Geometry geo1, Geometry geo2)
        {
            var unifiedBoundingBox = BoundingBox.Unify(geo1.BoundingBox, geo2.BoundingBox);
            var unifiedPolygons = geo1.Polygon.Concat(geo2.Polygon).ToList();

            return new Geometry
            {
                BoundingBox = unifiedBoundingBox,
                Polygon = unifiedPolygons
            };
        }
    }
}
