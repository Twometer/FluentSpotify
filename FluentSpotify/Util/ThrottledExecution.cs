using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSpotify.Util
{
    public class ThrottledExecution
    {
        private TimeSpan delay;

        private DateTime lastExecution;

        private TimeSpan TimeSinceLastExecution => DateTime.Now - lastExecution;

        private volatile bool isAwaiting;

        public ThrottledExecution(TimeSpan delay)
        {
            this.delay = delay;
        }

        public async void Run(Action action)
        {
            if (TimeSinceLastExecution > delay)
                action.Invoke();
            else if (!isAwaiting)
            {
                isAwaiting = true;
                await Task.Delay((int)(delay.TotalMilliseconds - TimeSinceLastExecution.TotalMilliseconds));
                isAwaiting = false;
                if (TimeSinceLastExecution > delay)
                    action.Invoke();
            }
            lastExecution = DateTime.Now;
        }

    }
}
