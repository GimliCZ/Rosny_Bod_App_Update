using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace Rosny_Bod_App
{
    public class Options
    {//Serial com
     //COM adresa:COM_Adress
     //Find mode
     //Čas poklesu teploty za n ticků (Stav náběhu):Speed_Const
     //Čas poklesu teploty za n ticků (Stav měření):Speed2_Const
     //Offset od světelného pozadí - zahřívání:Low_light_Const
     //Offset od světelného pozadí - chlazení:High_light_Const
     //Náběhový krok změny teploty:Inicialstep
     //Měřící krok změny teploty:Messuringstep
     //Regulator
     //Proporcionální konstanta závislá na čase:R0
     //Derivační konstanta závislá na čase:Td
     //Integrační konstanta závislá na čase:Ti
        public File_COM soubory = new File_COM("Options.cfg", "Settings");
        public readonly string data = @"-COM_adresa:COM1
Čas poklesu teploty za n ticků (Stav náběhu)-Speed_Const:10
Čas poklesu teploty za n ticků (Stav měření)-Speed2_Const:300
Offset od světelného pozadí(zahřívání)-Low_light_Const:32.27
Offset od světelného pozadí(chlazení)-High_light_Const:96.68
Náběhový krok změny teploty-Inicialstep:0.3
Měřící krok změny teploty-Messuringstep:0.05
Proporcionální konstanta závislá na čase-R0:80
Derivační konstanta závislá na čase-Td:0.002
Integrační konstanta závislá na čase-Ti:14";
        public string COM_adresa = "";
        public double Speed_Const = 0;
        public double Speed2_Const = 0;
        public double Low_light_Const = 0;
        public double High_light_Const = 0;
        public double Inicialstep = 0;
        public double Messuringstep = 0;
        public double R0 = 0;
        public double Td = 0;
        public double Ti = 0;
        public const string slozka = "Settings";
        public string current_path = Directory.GetCurrentDirectory();

        public void Create_folder(string foldername) //Založ složku
        {
            try
            {
                Directory.CreateDirectory(current_path + "//" + foldername);
            }
            catch (Exception)
            {
            }
        }

        public string Readfile()
        {
            string data;
            try
            {
                data = soubory.Read_Data();
            }
            catch (DirectoryNotFoundException)
            {
                return "-1";
            }
            catch (FileNotFoundException)
            {
                return "-1";
            }


            return data;
        }

        public void Createfile(string filename, string foldername)
        {
            var file = File.Create(current_path + "\\" + foldername + "\\" + filename);
            file.Close();
        }

        public void Generate_Options()
        {
            Create_folder(slozka);
            Createfile("Options.cfg","Settings");

            soubory.Add_Data(data);
        }
        public bool ReadOptions()
        {
            bool once = true;
            string Options_data = Readfile();
            if (Options_data == "-1")
            {
                Generate_Options();
                if (once == true)
                {
                    Options_data = Readfile();
                    once = false;
                }
                else { throw new InvalidOperationException("Selhalo načtení Options"); }
            }
            try
            {
                char Before = '-';
                char After = ':';
                char End = '\n';
                int Countb = 0;
                int Counta = 0;
                int Counte = 0;
                const int Countexpected = 10;
                string[,] Readdata = new string[2, 11];
                int Index = 0;
                int Indexb = 0;
                int Indexa = 0;
                int Indexe = 0;
                foreach (char c in Options_data)
                {
                    if (c == Before)
                    {
                        Indexb = Index;
                        Countb++;
                    }
                    if (c == After)
                    {
                        Indexa = Index + 1;
                        Counta++;
                    }
                    if (c == End)
                    {
                        Indexe = Index - 1;
                        Counte++;
                    }
                    if (Countb == Countexpected && Counta == Countexpected && Counte == Countexpected - 1)
                    {
                        Indexe = Options_data.Length;
                        Counte++;//add end of file
                    }
                    if (Countb == Counta && Counta == Counte && Counta != 0 && Counta <= Countexpected)
                    {
                        Readdata[0, Counte] = Options_data.Substring(Indexb, Indexa - Indexb);
                        Readdata[1, Counte] = Options_data.Substring(Indexa, Indexe - Indexa);
                    }
                    Index++;
                }
                if (!(Countb == Counta && Counta == Counte && Counta == Countexpected))
                {
                    return false;
                }
                else
                {
                    if (!Options_data.Contains("COM_adresa") || !Options_data.Contains("Speed_Const") || !Options_data.Contains("Speed2_Const") || !Options_data.Contains("Low_light_Const") ||
                    !Options_data.Contains("High_light_Const") || !Options_data.Contains("Inicialstep") || !Options_data.Contains("Messuringstep") || !Options_data.Contains("R0") ||
                    !Options_data.Contains("Td") || !Options_data.Contains("Ti"))
                    {
                        return false;
                    }

                    for (int x = 0; x < Countexpected + 1; x++)
                    {
                        if (Readdata[0, x] == "-COM_adresa:")
                        {
                            COM_adresa = Readdata[1, x];
                        }
                        if (Readdata[0, x] == "-Speed_Const:")
                        {
                            Speed_Const = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Speed2_Const:")
                        {
                            Speed2_Const = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Low_light_Const:")
                        {
                            Low_light_Const = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-High_light_Const:")
                        {
                            High_light_Const = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Inicialstep:")
                        {
                            Inicialstep = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Messuringstep:")
                        {
                            Messuringstep = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-R0:")
                        {
                            R0 = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Td:")
                        {
                            Td = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }
                        if (Readdata[0, x] == "-Ti:")
                        {
                            Ti = double.Parse(Readdata[1, x], NumberStyles.Float | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                        }

                    }
                    return true;
                }
            }
            catch (Exception) {
                MessageBox.Show("Došlo k chybě při načítání nastavení");
                return false;
            }


        }
    }
}
