using System;
using System.Threading;

namespace TinyTcpServer.Core.FunctionalTests.TestUtils
{
    internal static class TimeDelay
    {
        public static void Delay(Int32 pause)
        {
            ManualResetEventSlim delayEvent = new ManualResetEventSlim(false);
            Timer timer = new Timer(arg =>
            {
                ManualResetEventSlim signal = (arg as ManualResetEventSlim);
                if (signal == null)
                    throw new ArgumentNullException("arg");
                signal.Set();
            }, delayEvent, pause, -1);
            delayEvent.Wait();
            timer.Dispose();
            delayEvent.Dispose();
        }
    }
}
