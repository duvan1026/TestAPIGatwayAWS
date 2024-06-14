using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPIGatewayAWS.Models
{
    public class Relationship
    {
        public string Type { get; set; }
        public List<string> Ids { get; set; }
    }
}
