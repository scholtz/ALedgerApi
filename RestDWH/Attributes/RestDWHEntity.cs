using RestDWH.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestDWH.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class RestDWHEntity : System.Attribute
    {
        public string Name { get; set; }
        public Type? Events { get; set; }

        public RestDWHEntity(string name, Type? events = null)
        {
            Name = name;
            Events = events;
        }
    }
}
