using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace csi
{
    class CommandLineArgsParser<OptionsType>
        where OptionsType: class, new()
    {
        public OptionsType Options { get; private set; }
        public List<string> Values { get; private set; }


        private Type _type;
        private char _prefix;
        private char _splitter;

        private class PropertyContainer
        {
            public PropertyInfo PropertyInfo;
            public bool IsUsed;
        }


        public CommandLineArgsParser(char prefix = '/', char splitter = ':')
        {
            _type = typeof(OptionsType);
            _prefix = prefix;
            _splitter = splitter;

            Options = new OptionsType();
            Values = new List<string>();
        }


        public bool Parse(string[] args = null)
        {
            var propDic = CreatePropertyDic(_type);

            if (args == null) args = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (args.Contains(string.Format("{0}help", _splitter)))
            {
                PrintHelp(_type);
                return false;
            }

            foreach (var arg in args)
            {
                var found = propDic.FirstOrDefault(item =>
                    arg.StartsWith(string.Format("{0}{1}", _prefix, item.Key.Name)));

                if (found.Value == null)
                {
                    Values.Add(arg);
                    continue;
                }

                var attr = found.Key;
                var prop = found.Value;
                string tmpValue = "";
                switch (attr.Type)
                {
                    case CommandLineArgType.String:
                        tmpValue = arg.Split(_splitter)[1];
                        prop.PropertyInfo.SetValue(Options, tmpValue);
                        prop.IsUsed = true;
                        break;

                    case CommandLineArgType.StringList:
                        var list = (List<string>)prop.PropertyInfo.GetValue(Options);
                        if (list == null) list = new List<string>();
                        tmpValue = arg.Split(_splitter)[1];
                        list.Add(tmpValue);
                        prop.PropertyInfo.SetValue(Options, list);
                        prop.IsUsed = true;
                        break;

                    case CommandLineArgType.Boolean:
                        prop.PropertyInfo.SetValue(Options, true);
                        prop.IsUsed = true;
                        break;

                    case CommandLineArgType.Integer:
                        tmpValue = arg.Split(_splitter)[1];
                        prop.PropertyInfo.SetValue(Options, int.Parse(tmpValue));
                        prop.IsUsed = true;
                        break;

                    case CommandLineArgType.Double:
                        tmpValue = arg.Split(_splitter)[1];
                        prop.PropertyInfo.SetValue(Options, double.Parse(tmpValue));
                        prop.IsUsed = true;
                        break;

                    default:
                        throw new InvalidOperationException(
                            string.Format("invalid CommandLineArgType: {0}", attr.Type.ToString()));
                }
            }

            foreach (var unused in propDic.Where(item => !item.Value.IsUsed))
            {
                var defaultValue = unused.Key.DefaultValue;
                if (defaultValue != null)
                {
                    unused.Value.PropertyInfo.SetValue(Options, defaultValue);
                }
            }

            return true;
        }


        private void PrintHelp(Type type)
        {
            foreach (var prop in type.GetProperties())
            {
                var attrs = Attribute.GetCustomAttributes(prop)
                    .Where(attr => attr is CommandLineArgAttribute);
                foreach (CommandLineArgAttribute attr in attrs)
                {
                    Console.WriteLine("{0}: {1}", attr.Name, attr.Description);
                }
            }
        }


        private Dictionary<CommandLineArgAttribute, PropertyContainer> CreatePropertyDic(Type type)
        {
            var dic = new Dictionary<CommandLineArgAttribute, PropertyContainer>();

            foreach (var prop in type.GetProperties())
            {
                var attrs = Attribute.GetCustomAttributes(prop)
                    .Where(attr => attr is CommandLineArgAttribute);
                foreach (CommandLineArgAttribute attr in attrs)
                {
                    dic[attr] = new PropertyContainer()
                    {
                        PropertyInfo = prop,
                        IsUsed = false,
                    };
                }
            }

            return dic;
        }
    }
}
