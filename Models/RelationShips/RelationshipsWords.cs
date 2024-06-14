using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TestAPIGatewayAWS.Program;

namespace TestAPIGatewayAWS.Models.RelationShips
{
    public class RelationshipsWords
    {
        public string Type { get; set; }
        public List<Models.Block.BlockWord> Words { get; set; }
    }
}
