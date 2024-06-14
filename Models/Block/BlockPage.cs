using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestAPIGatewayAWS.Program;

namespace TestAPIGatewayAWS.Models.Block
{
    public class BlockPage
    {
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
        public RelationShips.RelationshipsLines RelationshipsLines { get; set; }
    }
}
