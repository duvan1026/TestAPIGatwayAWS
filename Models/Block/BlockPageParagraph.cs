using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPIGatewayAWS.Models.Block
{
    public class BlockPageParagraph
    {
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
        public string Class { get; set; }
        public List<BlockParagraph> BlockParagraph { get; set; }
    }
}
