using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeMapper.Domain
{
    public class VersionControl
    {
        public static Version CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version;
    }
}
