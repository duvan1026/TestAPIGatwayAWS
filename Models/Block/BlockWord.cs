using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPIGatewayAWS.Models.Block
{
    public class BlockWord
    {
        public double? Confidence { get; set; }
        public string Text { get; set; }
        public string TextType { get; set; }
        public Geometry Geometry { get; set; }
        public string Id { get; set; }
    }
}
