using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core;

public static class DiagnosticsConfig
{
    public static readonly ActivitySource ActivitySource =
        new ("LAV.GraphDbFramework", "1.0.0");

    public static readonly Meter Meter = new("LAV.GraphDbFramework", "1.0.0");

    public static readonly Counter<long> QueryCount =
        Meter.CreateCounter<long>("graphdb.queries.count");

    public static readonly Histogram<double> QueryDuration =
        Meter.CreateHistogram<double>("graphdb.queries.duration");
}
