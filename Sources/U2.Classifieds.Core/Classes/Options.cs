using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace U2.Classifieds.Core;

public sealed class Options
{
    public bool Images { get; set; }

    public bool Topics { get; set; }

    public bool Branches { get; set; }

    public bool InitBranches { get; set; }
}