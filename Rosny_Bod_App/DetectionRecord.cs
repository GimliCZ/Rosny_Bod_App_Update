using PropertyChanged;
using System;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class DetectionRecord
    {
        /// <summary>
        /// teplota PT100
        /// </summary>
        public float PT100_Temperature { get; set; }

        /// <summary>
        /// teplota okolního prostředí
        /// </summary>
        public float ENV_Temperature { get; set; }

        /// <summary>
        /// Okolní tlak
        /// </summary>
        public float ENV_Pressure { get; set; }

        /// <summary>
        /// čas
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Vlhkost
        /// </summary>
        public float Humidity { get; set; }

        public float Calculate_Humidity(float temperature_in, float temperature_out)
        {
            double Aprox_Density_in = 5.018 + 0.32321 * temperature_in + 8.1847 * Math.Pow(10, -3) * Math.Pow(temperature_in, 2) + 3.1243 * Math.Pow(10, -4) * Math.Pow(temperature_in, 3);
            double Aprox_Density_out = 5.018 + 0.32321 * temperature_out + 8.1847 * Math.Pow(10, -3) * Math.Pow(temperature_out, 2) + 3.1243 * Math.Pow(10, -4) * Math.Pow(temperature_out, 3);
            float Relative_Humidity = (float)(Aprox_Density_in / Aprox_Density_out) * 100;
            return Relative_Humidity;
        }

        public DetectionRecord(float tempin, float tempout, float press, DateTime time)
        {
            PT100_Temperature = tempin;
            ENV_Temperature = tempout;
            ENV_Pressure = press;
            Time = time;
            Humidity = Calculate_Humidity(tempin, tempout);
        }
    }
}