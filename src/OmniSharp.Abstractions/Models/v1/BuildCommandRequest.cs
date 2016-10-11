using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OmniSharp.Mef;
using OmniSharp.Models;

namespace OmniSharp.Abstractions.Models.v1
{
    [OmniSharpEndpoint(OmnisharpEndpoints.BuildCommand, typeof(Request), typeof(string), ResponseWriter.Text)]

    public class BuildCommandRequest : Request
    {
    }
}
