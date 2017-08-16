using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CoffeeMapper
{
    public static class XMLParser
    {
        //returns all bind types (eg. button/axis/etc.)
        public static string[] Binds(string key)
        {
            string BindsXML = AppDomain.CurrentDomain.BaseDirectory + @"\data\KeyMappings.xml";
            List<string> _binds = new List<string>();
            using (XmlReader reader = XmlReader.Create(BindsXML))
            {
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "KeyMappings":
                            break;
                        case "Bind":
                            string value = reader["actualKey"];
                            if (value == key)
                            {
                                _binds.Add(reader["target"]);
                            }
                            break;
                    }
                }
            }
            return _binds.ToArray();
        }

        //returns actual value for bind type (eg. button 13, x axis 31233)
        public static string[] Values(string key)
        {
            string BindsXML = AppDomain.CurrentDomain.BaseDirectory + @"\data\KeyMappings.xml";
            List<string> _values = new List<string>();
            using (XmlReader reader = XmlReader.Create(BindsXML))
            {
                while (reader.Read())
                {
                    switch (reader.Name)
                    {
                        case "KeyMappings":
                            break;
                        case "Bind":
                            string value = reader["actualKey"];
                            if (value == key)
                            {
                                _values.Add(reader["value"]);
                            }
                            break;
                    }
                }
            }
            return _values.ToArray();
        }

    }
}
