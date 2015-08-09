using System;

namespace Zepheus.Util
{
    public class TimedTask
    {
		readonly Action action;
		private readonly TimeSpan? repeat; // Nullable.
		private DateTime when;

        public TimedTask(Action pAction, DateTime pWhen)
        {
            action = pAction;
            when = pWhen;
            repeat = null;
        }

        public TimedTask(Action pAction, TimeSpan pInterval, TimeSpan pRepeat)
        {
            action = pAction;
            when = DateTime.Now + pInterval;
            repeat = pRepeat;
        }

        /// <summary>
        /// This function tries to run _action on a specific time.
        /// </summary>
        /// <param name="pCurrentTime"></param>
        /// <returns>False when the task still needs to be ran, else True</returns>

        public bool RunTask(DateTime pCurrentTime)
        {
            if (when <= pCurrentTime)
            {
                action();
                if (repeat != null)
                {
                    // This pCurrentTime.Add is done for the small chance that the server
                    // is overloaded and the function couldn't run on time. Small chance,
                    // but we just make sure it will run on time next time it will be started
                    // (I guess).
                    // Stupid VS, complaining about this.
                    // _repeat might be null, and Add doesn't want that, so if it's null, we make a new TimeSpan
                    // This shall never happen though lawl
                    when = pCurrentTime.Add(repeat ?? new TimeSpan(0, 0, 0));
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
