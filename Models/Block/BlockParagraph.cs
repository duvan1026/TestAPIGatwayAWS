using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAPIGatewayAWS.Models.RelationShips;

namespace TestAPIGatewayAWS.Models.Block
{
    public class BlockParagraph
    {
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
        public string Class { get; set; }
        public Models.RelationShips.RelationshipsLines RelationshipsLines { get; set; }

        public BlockParagraph() 
        {
            Geometry = new Geometry();
            RelationshipsLines = new RelationshipsLines
            {
                Lines = new List<BlockLine>()
            };
        }
    }
}
