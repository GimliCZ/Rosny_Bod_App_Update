using PropertyChanged;
using System;
using System.Linq;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class FindMode
    {/// <summary>
     /// Proměnná držící posledních 100 hodnot teploty z PT100
     /// </summary>
        public double[] Temperature_PT100_Old { get; set; } = new double[100];

        /// <summary>
        /// Hledaná teplota
        /// </summary>
        private double Tested_Temperature_PT100 { get; set; } = 0;

        /// <summary>
        /// Záznam posledních 5 hodnot z fotoresistoru
        /// </summary>
        private double[] Light_Sence_Old { get; set; } = new double[5];

        /// <summary>
        /// Konstanta obsahující rozdíl mezi testovanou hodnotou a realnou hodnotou
        /// </summary>
        public double Difference { get; set; } = 0;

        /// <summary>
        /// Konstanta čekání naplnění arraye Temperature_PT100_Old
        /// </summary>
        private int X { get; set; } = 0; //counter

        /// <summary>
        /// Konstanta držící současný stav fotoresistoru
        /// </summary>
        public double Lightbackground { get; set; } = 0;

        /// <summary>
        /// Proměnná ovlivňující stoupání / klesání teploty
        /// </summary>
        private bool Flip { get; set; } = false;

        /// <summary>
        /// Counter držící zahřívací čas
        /// </summary>
        private int T1 { get; set; } = 0;

        /// <summary>
        ///  Counter držící chladící čas
        /// </summary>
        private int T2 { get; set; } = 0;

        /// <summary>
        /// Counter resetu na okolní teplotu
        /// </summary>
        private double T3 { get; set; } = 0;

        /// <summary>
        /// konstanta ovlivňující rychlost poklesu teploty
        /// </summary>
        private double T1time { get; set; } = 10;

        /// <summary>
        /// Rosný bod?
        /// </summary>
        private bool Dewpoint { get; set; } = false;

        /// <summary>
        /// Rychlost pádu teploty za tick (Počáteční)
        /// </summary>
        public double Speed_Const { get; set; } = 10;

        /// <summary>
        /// Rychlost pádu teploty za tick (Měřící)
        /// </summary>
        public double Speed2_Const { get; set; } = 300;

        /// <summary>
        /// Offset od světelného pozadí - zahřívání
        /// </summary>
        public double Low_light_Const { get; set; } = 32.27;

        /// <summary>
        /// Offset od světelného pozadí - chlazení
        /// </summary>
        public double High_light_Const { get; set; } = 96.68;

        /// <summary>
        /// Počáteční krok
        /// </summary>
        public double Inicialstep { get; set; } = 0.3;

        /// <summary>
        /// Měřící krok
        /// </summary>
        public double Messuringstep { get; set; } = 0.05;

        /// <summary>
        /// Krok °C
        /// </summary>
        public double Step { get; set; } = 0.05;

        /// <summary>
        /// Proměnná blokující vícenásobné nahrání pozadí
        /// </summary>
        public bool Once { get; set; } = false;

        /// <summary>
        /// Proměnná obsahující pozadí + offset
        /// </summary>
        public double Light_low { get; set; }

        /// <summary>
        /// Proměnná obsahující pozadí + offset
        /// </summary>
        public double Light_high { get; set; }

        /// <summary>
        /// Výchozí teplota
        /// </summary>
        public double Inicial_temperature { get; set; }

        /// <summary>
        /// Proměnná počítající ustálení
        /// </summary>
        private int Stabilization_counter { get; set; } = 0;

        public double temperaturecheck;

        //  public double light_background_converted { get; set; }
        //  public double light_low_converted { get; set; } = 32.265625;
        //  public double light_high_converted { get; set; } = 96.6796875;


        public void Get_background(int Light_Sence, double Temperature_env)
        {

            if (Once == false)
            {
                Lightbackground = Convert_to_double(Light_Sence); //měřené pozadí
                Light_low = Lightbackground + Low_light_Const; // hodnota navrácení se k ochlazování
                Light_high = Lightbackground + High_light_Const; // hodnota navráceníse k ohřívání
                Inicial_temperature = Math.Round(Temperature_env, 1); //výchozí teplota okolního prostředí
                if (Inicial_temperature == 0)
                {
                    Once = false;
                }
                else
                {
                    Once = true;
                }
            }
        }

        public void ResetTemperatureMessurement()
        {
            Difference = 0;
            Dewpoint = false;
            Flip = false;

        }

        public double Temperature_Set()
        {
            Tested_Temperature_PT100 = Inicial_temperature; // nastav výchozí testovanou hodnotu na zaokrouhlenou hodnotu vnějšího prostředí

            if (X > 99) // po naplnění průměrovacího bufferu teploty
            {
                if (Math.Abs(Tested_Temperature_PT100 - Difference - Queryable.Average(Temperature_PT100_Old.AsQueryable())) < 0.05) //pokud je rozdíl žádané teploty a realné teploty nižší nežli 0.03 tak
                {
                    if (Flip == false) // ochlazování
                    {
                        T1++; //counter posunu teploty při chlazení
                        if (Light_Sence_Old[0] - Light_Sence_Old[4] > 2)
                        { // pokud detekuješ velkou změnu světla, tak počkej s krokem
                            T1 = 0;
                        }
                        if (Dewpoint)
                        {
                            T1time = Speed2_Const;//udělej pokles teploty každých n tiků - standardní měření
                            Step = Messuringstep;
                        }
                        else
                        {
                            T1time = Speed_Const;//-náběh zařízení
                            if (Light_Sence_Old[0] > Light_low)
                            {
                                Step = Messuringstep;
                            }
                            else
                            {
                                Step = Inicialstep;
                            }
                        }
                        if (T1 > T1time)
                        {
                            Difference += Step;
                            T1 = 0;
                            T2 = 0;
                        }
                        temperaturecheck = Tested_Temperature_PT100;
                    }
                    if (Flip == true) // zahřívání
                    {
                        T2++;
                        if (T2 > 100)
                        {
                            Difference -= Step;
                            T2 = 0;
                            T1 = 0;
                        }
                        /*  if (temperaturecheck+2< Tested_Temperature_PT100) //pokud při zahřívání zjistíš, že
                          {
                              Get_background(Queryable.Min(Light_Sence_Old.AsQueryable()),Queryable.Min(Temperature_PT100_Old.AsQueryable()));
                          }*/
                    }
                    if (Tested_Temperature_PT100 < 0 && Flip == false) //Pojistka, která by neměla nastat - Pokud se zařízení podchladí pod 0, tak se vrať na výchozí hodnotu teploty
                    {
                        Difference = 0;
                    }
                }
            }
            return Tested_Temperature_PT100 -= Difference;
        }

        public bool Prepare_for_shutdown(double Temp_Env) // navedení teploty tařízení do stabilní polohy
        {
            Difference = 0;
            if (Math.Abs(Queryable.Average(Temperature_PT100_Old.AsQueryable()) - Temp_Env) < 0.5)
            {
                Stabilization_counter++;
            }
            if (Stabilization_counter > 300)
            {
                return true;
            }
            else { return false; }
        }

        public bool Dewpoint_Detect()
        {
            if (X > 99) // po naplnění průměrovacího bufferu světla
            {
                /* if (Lightbackground == 0)
                 {
                     Lightbackground = 400;// Convert.ToInt32(Math.Round(Queryable.Average(Light_Sence_Old.AsQueryable())));
                 }*/

                if ((Queryable.Average(Light_Sence_Old.AsQueryable()) - Light_high) > 3 && Flip == false)
                {
                    Flip = true;
                    Dewpoint = true;
                    return true;
                }
                if ((Queryable.Average(Light_Sence_Old.AsQueryable()) - Light_low) < 3 && Flip == true)
                {
                    Flip = false;
                    return false;
                }
            }
            return false;
        }

        public void Update(int Light_Sence, double Temperature_PT100, double Temp_Env)
        {
            int y = 0;
            int z = 0;

            Light_Sence_Old[0] = Convert_to_double(Light_Sence);
            while (y < 4) // update světla
            {
                Light_Sence_Old[y + 1] = Light_Sence_Old[y];
                y++;
            }

            Temperature_PT100_Old[0] = Temperature_PT100;
            while (z < 99) // update teploty
            {
                Temperature_PT100_Old[z + 1] = Temperature_PT100_Old[z];
                z++;
            }
            X++;
            if (X > 100)
            {
                X = 101;
            }
            if (Math.Abs(Inicial_temperature - Temp_Env) > 2)// Pokud se vnější teplota změní o 2°C, tak resetuj hledání
            {
                Difference = 0;
                Flip = false;
                Dewpoint = false;
                T3++;
                T2 = 0; // vyneguj veškeré změny
                T1 = 0;
                if (T3 > 50000) // po stabilizaci
                {
                    Once = false;
                    Get_background(Light_Sence, Temp_Env);
                }
            }
        }
        public void Forceupdatebackground(double Temperature_env)
        {
            Light_low = Lightbackground + Low_light_Const; // hodnota navrácení se k ochlazování
            Light_high = Lightbackground + High_light_Const; // hodnota navráceníse k ohřívání
            Inicial_temperature = Math.Round(Temperature_env, 1);
        }
        public int Convert_to_int(double voltage)
        {
            return (int)Math.Round((voltage / 1000) / (3.3 / 1024), 0);
        }
        public double Convert_to_double(int light)
        {
            return light * (3.3 / 1024) * 1000;
        }
    }
}