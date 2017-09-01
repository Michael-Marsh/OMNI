using System;
using System.Windows.Threading;

namespace OMNI.Helpers
{
    /// <summary>
    /// Update Timer Interaction Logic
    /// </summary>
    public sealed class UpdateTimer
    {
        #region Properties

        /// <summary>
        /// Dispatch Timer for Updates
        /// </summary>
        public static DispatcherTimer UpdateDispatchTimer { get; private set; }

        /// <summary>
        /// Method or method group subscribed to the UpdateDispatchTimer_Tick method
        /// </summary>
        public static Action UpdateTimerTick { get; set; }

        #endregion

        /// <summary>
        /// Update Timer Constructor
        /// </summary>
        public UpdateTimer()
        {

        }

        /// <summary>
        /// Update Timer Tick
        /// </summary>
        /// <param name="sender">Dispatch Timer</param>
        /// <param name="e">Tick Events</param>
        public static void UpdateDispatchTimer_Tick(object sender, EventArgs e)
        {
            UpdateTimerTick?.Invoke();
        }

        /// <summary>
        /// Initialize the Update Timer
        /// </summary>
        /// <param name="timeSpan">Interval to use for update ticks</param>
        public static void IntializeUpdateTimer(TimeSpan timeSpan)
        {
            UpdateDispatchTimer = new DispatcherTimer();
            UpdateDispatchTimer.Tick += new EventHandler(UpdateDispatchTimer_Tick);
            UpdateDispatchTimer.Interval = timeSpan;
            UpdateDispatchTimer.Start();
        }

        /// <summary>
        /// Checks the invocation list to see if it contains the method group
        /// </summary>
        /// <param name="action">Method group to search for</param>
        /// <returns>True = invocation list contains method, False = action has not been added yet</returns>
        public static bool ContainsMethod(Action action)
        {
            if (UpdateTimerTick == null)
            {
                return false;
            }
            foreach (Action _action in UpdateTimerTick.GetInvocationList())
            {
                if (_action.Method.Name.Equals(action.Method.Name))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add an action to the Update Timer Tick function
        /// </summary>
        /// <param name="action">Method or Action to add</param>
        public static void Add(Action action)
        {
            if (UpdateTimerTick == null)
            {
                UpdateTimerTick += action;
            }
            else
            {
                foreach (Action _action in UpdateTimerTick.GetInvocationList())
                {
                    if (_action.Method.Name.Equals(action.Method.Name))
                    {
                        UpdateTimerTick -= _action;
                    }
                }
                UpdateTimerTick += action;
            }
        }

        /// <summary>
        /// Remove an action from the Update Timer Tick function
        /// </summary>
        /// <param name="action">Method or Action to remove</param>
        public static void Remove(Action action)
        {
            if (ContainsMethod(action))
            {
                UpdateTimerTick -= action;
            }
        }
    }
}
