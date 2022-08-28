using System;

namespace Rosny_Bod_App
{
    public class CustomTimer
    {
        /// <summary>
        /// Drží čas startu
        /// </summary>
        public DateTime Cas { get; set; }

        /// <summary>
        /// Drží aktuální čas
        /// </summary>
        public DateTime Cas2 { get; set; }

        /// <summary>
        /// Rozdíl časů
        /// </summary>
        public TimeSpan Difference { get; set; }

        /// <summary>
        /// Pracuje časovač?
        /// </summary>
        public bool Running { get; set; } = false;

        public void Start_timer()
        {
            if (!Running)
            {
                Cas = DateTime.Now;
            }
            Running = true;
        }

        public bool Timer_enlapsed(double doba)
        {
            if (!Running)
            {
                throw new InvalidOperationException("Timer didnt start");
            }
            Cas2 = DateTime.Now;
            Difference = Cas2 - Cas;
            if (Difference.TotalSeconds > doba)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stop_timer()
        {
            Cas = DateTime.MinValue;
            Cas2 = DateTime.MinValue;
            Running = false;
        }
    }
}