namespace H3Control.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Common;

    using Universe;

    public class ProcessController
    {
        public static List<PsProcessInfo> Select(PsSortOrder order, int topN, bool isIe8OrBelow)
        {
            Stopwatch sw3 = Stopwatch.StartNew();
            var list = PsListener_OnLinux.Select(order, topN);
            var time = sw3.Elapsed;
            FirstRound.Only("Select Processes", RoundCounter.Twice, () =>
            {
                NiceTrace.Message("Processes ({0} msec):{1}{2}",
                    time, Environment.NewLine,
                    string.Join(Environment.NewLine, list.Select(x => "           " + x.ToString())));
            });

            // Trim command with args for IE8
            if (isIe8OrBelow)
                foreach (var pi in list)
                    pi.Args = pi.Args != null && pi.Args.Length > 80 ? (pi.Args.Substring(0, 77) + "...") : pi.Args;
            return list;
        }

    }
}