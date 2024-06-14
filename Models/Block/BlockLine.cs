using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestAPIGatewayAWS.Program;

namespace TestAPIGatewayAWS.Models.Block
{
    public class BlockLine
    {
        public double? Confidence { get; set; }
        public string Text { get; set; }
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
        public RelationShips.RelationshipsWords RelationshipsWords { get; set; }

        /// <summary>
        /// Método para verificar si el objeto contiene datos significativos
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return (Confidence.HasValue && Confidence.Value > 0) ||
                   !string.IsNullOrEmpty(Text) ||
                   (Geometry != null && Geometry.HasData()) ||
                   !string.IsNullOrEmpty(Id) ||
                   (RelationshipsWords != null && RelationshipsWords.Words.Any());
        }
    }
}
