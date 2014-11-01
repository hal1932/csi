using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    enum CommandLineArgType
    {
        String,
        StringList,
        Boolean,
        Integer,
        Double,
    }

    class CommandLineArgAttribute : Attribute
    {
        public string Name;
        public string Description;
        public object DefaultValue;
        public CommandLineArgType Type;

        public CommandLineArgAttribute(string name)
        {
            Name = name;
        }
    }
}
