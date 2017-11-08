using System;
using System.Diagnostics;
using System.Reflection;

namespace CoffeeMapper.Domain
{
    public class VersionControl
    {
        public static Version CurrentVersion
        {
            get
            {
                var ver = Assembly.GetExecutingAssembly().GetName().Version;
                Debug.WriteLine(ver);
                return ver;
            }
        }
    }
}
