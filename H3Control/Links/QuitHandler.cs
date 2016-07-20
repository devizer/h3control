using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control.Links
{
    using System.Diagnostics;
    using System.Security.Principal;

    using Mono.Unix;
    using Mono.Unix.Native;

    public class UnixExitSignal /*: IExitSignal*/
    {

        public static void Bind(Action<string> signalHandler, params Signal[] signals)
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return;

            try
            {
                BindImplementation(signalHandler, signals);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Ctlr-C handler via Mono.Posix failed" + Environment.NewLine + ex);
            }
        }

        private static void BindImplementation(Action<string> signalHandler, Signal[] signals)
        {
            if (signals == null || signals.Length == 0)
                signals = new[] {Signal.SIGTERM, Signal.SIGINT, Signal.SIGUSR1,};

            var signs = signals.Select(x => new UnixSignal((Signum) (int) x)).ToArray();

            Task.Factory.StartNew(() =>
            {
                // blocking call to wait for any kill signal
                int index = UnixSignal.WaitAny(signs, -1);

                if (signalHandler != null)
                {
                    signalHandler(index + ": " + (index >= 0 && index < signals.Length ? signals[index].ToString() : ""));
                }
            });
        }
    }

    public enum Signal
    {
        SIGHUP = 1,
        SIGINT = 2,
        SIGQUIT = 3,
        SIGILL = 4,
        SIGTRAP = 5,
        SIGABRT = 6,
        SIGIOT = 6,
        SIGBUS = 7,
        SIGFPE = 8,
        SIGKILL = 9,
        SIGUSR1 = 10,
        SIGSEGV = 11,
        SIGUSR2 = 12,
        SIGPIPE = 13,
        SIGALRM = 14,
        SIGTERM = 15,
        SIGSTKFLT = 16,
        SIGCHLD = 17,
        SIGCLD = 17,
        SIGCONT = 18,
        SIGSTOP = 19,
        SIGTSTP = 20,
        SIGTTIN = 21,
        SIGTTOU = 22,
        SIGURG = 23,
        SIGXCPU = 24,
        SIGXFSZ = 25,
        SIGVTALRM = 26,
        SIGPROF = 27,
        SIGWINCH = 28,
        SIGIO = 29,
        SIGPOLL = 29,
        SIGPWR = 30,
        SIGSYS = 31,
        SIGUNUSED = 31,
    }

}
