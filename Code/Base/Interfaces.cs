using System;
using System.Collections.Generic;
using System.Text;

namespace SoulFab.Core.Base
{
    public interface INamingRule
    {
        string To(string name);
        string From(string name);
    }
}
