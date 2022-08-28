using PropertyChanged;
using System;
using System.Globalization;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class Regulator
    {
        //Deklarace konstant regulátoru
        /*  H_Bridge_Control.E = 0;
              H_Bridge_Control.R0 = 80;
              H_Bridge_Control.Ti = 14;
              H_Bridge_Control.Td = 0.002;
              H_Bridge_Control.Umin = -10;
              H_Bridge_Control.Umax = 255;*/

        /// <summary>
        /// Konstanta obsahující odchylku od regulovaných hodnot
        /// </summary>
        public double E { get; set; } = 0;

        /// <summary>
        /// Proporcionální konstanta závislá na čase
        /// </summary>
        public double R0 { get; set; } = 80;

        /// <summary>
        /// Derivační konstanta závislá na čase
        /// </summary>
        public double Td { get; set; } = 0.002;

        /// <summary>
        /// Integrační konstanta závislá na čase
        /// </summary>
        public double Ti { get; set; } = 14;

        /// <summary>
        /// Akční zásah regulátoru
        /// </summary>
        public double U { get; set; }

        /// <summary>
        /// string U
        /// </summary>
        public string Su { get; set; }

        /// <summary>
        /// Minimální Akční zásah
        /// </summary>
        public double Umin { get; set; } = -10;

        /// <summary>
        /// Maximální akční zásah
        /// </summary>
        public double Umax { get; set; } = 255;

        /// <summary>
        /// Požadovaná hodnota
        /// </summary>
        public string B_requested_value { get; set; }

        /// <summary>
        /// Stará integrační proměnná
        /// </summary>
        private double OldI { get; set; }

        /// <summary>
        /// Stará odchylka
        /// </summary>
        private double Olde { get; set; }

        /// <summary>
        /// Integrační zásah
        /// </summary>
        public double I { get; set; }

        /// <summary>
        /// Proporcionální zásah
        /// </summary>
        public double P { get; set; }

        /// <summary>
        /// Derivační zásah
        /// </summary>
        public double D { get; set; }

        /// <summary>
        /// čas cyklu [ms]
        /// </summary>
        public TimeSpan Ts { get; set; }

        /// <summary>
        /// String I
        /// </summary>
        public string SI { get; set; }

        /// <summary>
        /// String P
        /// </summary>
        public string SP { get; set; }

        /// <summary>
        /// String D
        /// </summary>
        public string SD { get; set; }

        /// <summary>
        /// String Ts
        /// </summary>
        public string STs { get; set; }

        /// <summary>
        /// Čas minulého cyklu
        /// </summary>
        private DateTime Timeold { get; set; }

        /// <summary>
        /// Čas současného cyklu
        /// </summary>
        private DateTime Time { get; set; }

        public Regulator()
        {
            Timeold = DateTime.MinValue;
        }

        public double PIDWithClamping(double RequestedValue, double RealValue)
        {
            B_requested_value = RequestedValue.ToString();
            E = RealValue - RequestedValue;
            Time = DateTime.Now;
            if (Timeold == DateTime.MinValue)
            {
                I = 100;
                P = R0 * E;
                D = 0;
            }
            else
            {
                Ts = Time - Timeold;
                if (Ti == 0)
                {
                    I = 0;
                }
                else
                {
                    I = (R0 * Ts.Milliseconds / 1000) / (2 * Ti) * (E + Olde) + OldI;
                }
                P = R0 * E;
                D = (R0 * Td) / (Ts.Milliseconds * 0.0001) * (E - Olde);
                if (double.IsNaN(D))
                {
                    D = 0;
                }
                if (double.IsInfinity(D))
                {
                    D = 0;
                }
            }
            if (I + P + D > Umax)
            {
                I = OldI;
                U = Umax;
            }
            else if (I + P + D < Umin)
            {
                I = OldI;
                U = Umin;
            }
            else
            {
                U = I + P + D;
            }
            OldI = I;
            Olde = E;
            Timeold = Time;
            SD = D.ToString("F2", CultureInfo.CurrentCulture);
            SI = I.ToString("F2", CultureInfo.CurrentCulture);
            SP = P.ToString("F2", CultureInfo.CurrentCulture);
            Su = U.ToString("F2", CultureInfo.CurrentCulture);
            STs = Ts.Milliseconds.ToString(CultureInfo.CurrentCulture);

            return U;
        }
    }
}