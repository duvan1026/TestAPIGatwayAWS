using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestAPIGatewayAWS.Models.Block;

namespace TestAPIGatewayAWS.Models.RelationShips
{
    public class RelationshipsLines
    {
        public string Type { get; set; }
        public List<BlockLine> Lines { get; set; }
    }
}
