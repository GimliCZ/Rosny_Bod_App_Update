using PropertyChanged;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class Serial_Analyzer
    {
        public int Safetybit1 { get; set; } //safety_current_left_on
        public int Safetybit2 { get; set; } //safety_current_right_on
        public int Safetybit3 { get; set; } //safety_relay_1_on
        public int Safetybit4 { get; set; } //safety_relay_2_on
        public int LightSensorReport { get; set; }

        public double LightSensorReport_mV { get; set; }

        public float EnvTempReport { get; set; }
        public float EnvPressureReport { get; set; }
        public float AmpSence { get; set; }
        /// <summary>
        /// List záznamu o proudu
        /// </summary>
        public float[] Amperage { get; set; } = new float[100];

        /// <summary>
        /// counter limitující velikost dat v arrayi
        /// </summary>
        public static int AmperageCounter { get; set; } = 0;
        public double CoolerTempSence { get; set; }
        public double CoolerVoltSence { get; set; }
        public string CoolerTempSencetext { get; set; } = "0";
        public float PT100TempSence { get; set; }
        public float AmperageShow { get; set; } = 0;
        public float WattageShow { get; set; } = 0;
        public float CoolingShow { get; set; } = 0;
        public float MessuredMinimumAmperage { get; set; } = 1024;
        public string LastMessage { get; set; }


        public bool AnalyzeString(string MergedString)
        {
            LastMessage = MergedString;
            if (MergedString== "Device_Disconnect") {
                return false;
            }
            if (string.IsNullOrEmpty(MergedString))
            {
                return false;
            }
            try
            {
                if (MergedString == "H mustek - Nespravne napeti!") {
                  MessageBox.Show("H můstek zahlásil chybu řídícího napětí! Pro reset chyby vypněte a zapněte zdroj! Poruchu nahlaste!");
                }
                if (MergedString.Contains("MAX 31865")) {
                    MessageBox.Show("MAX 31865 zahlásil chybu:" + MergedString + "Pro reset chyby vyndejte a zandejte USB kabel! Poruchu nahlaste!");
                }
                
                if(MergedString.Contains("BMP280"))
                {
                    MessageBox.Show("BMP280 zahlásil chybu spojení! Pro reset chyby vyndejte a zandejte USB kabel! Poruchu nahlaste!");
                }
                string[] Reports = MergedString.Split(';');
                char[] tempsafetychar = Reports[0].ToCharArray();
                Safetybit1 = (int)(tempsafetychar[0] - '0');
                Safetybit2 = (int)(tempsafetychar[1] - '0');
                Safetybit3 = (int)(tempsafetychar[2] - '0');
                Safetybit4 = (int)(tempsafetychar[3] - '0');
                LightSensorReport = Int32.Parse(Reports[1]);
                LightSensorReport_mV = Math.Round(LightSensorReport * (3.3 / 1024) * 1000, 3);
                EnvTempReport = float.Parse(Reports[2], CultureInfo.InvariantCulture);
                EnvPressureReport = (float)Math.Round(double.Parse(Reports[3], CultureInfo.InvariantCulture),2) + 5;
                if (MessuredMinimumAmperage > float.Parse(Reports[4]))
                {
                    MessuredMinimumAmperage = float.Parse(Reports[4]);
                }
                AmpSence = Mapfloat(float.Parse(Reports[4]), MessuredMinimumAmperage, 1024, 0, 20);
                CoolerVoltSence = double.Parse(Reports[5], CultureInfo.InvariantCulture) / 1024 * 5;//
                Amperage[AmperageCounter] = AmpSence; // Proveď průměrování 100 prvků vzorů proudu (eliminace pwm)             
                if (AmperageCounter > 98)
                {
                    AmperageCounter = -1;
                    AmperageShow = (float)Math.Round(Queryable.Average(Amperage.AsQueryable()), 2);
                    WattageShow = AmperageShow * 12; //výpočet příkonu
                    CoolingShow = (float)Math.Round(55 * AmperageShow / 4.5, 2); //výpočet chladícího výkonu
                }
                // CoolerRessSence = 5 * 25000 / CoolerVoltSence - 25000;
                CoolerTempSence = CoolerVoltSence * 24.436;//Math.Round((3950 * 25) / (3950 + (25 * Math.Log(CoolerRessSence / 25))), 2);
                CoolerTempSencetext = CoolerTempSence.ToString("F2", CultureInfo.InvariantCulture);
                //(Math.Log((double.Parse(Reports[5])) / 100000) * 5693 + 79000+ 11926 * Math.PI);
                PT100TempSence = float.Parse(Reports[6], CultureInfo.InvariantCulture);
                AmperageCounter++;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Při konverzi dat došlo k fatalní chybě. Zkontrolujte zařízení! " + ex.Message);
            }
            return true;
        }

        public float Mapfloat(float x, float y, float z, float a, float b)
        {
            float temp = (x - y) * (b - a) / (z - y) + a;
            return temp;
        }
    }
}