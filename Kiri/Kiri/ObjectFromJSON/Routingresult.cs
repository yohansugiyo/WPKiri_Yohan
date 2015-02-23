using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Kiri
{
    class Routingresult
    {
        public List<List<object>> steps { get; set; }
        public string traveltime { get; set; }        
    }
}
