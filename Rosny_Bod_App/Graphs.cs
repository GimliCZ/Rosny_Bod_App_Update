using PropertyChanged;
using ScottPlot;
using System.Collections.Generic;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class Graphs
    {
        public double[] XAxisData { get; set; } = new double[101];
        public double[] YAxisData { get; set; } = new double[101];
        List<double> memory = new List<double>();


        public Graphs()
        {
            for (int i = 0; i < 101; i++) {
            XAxisData[i] = i;
            }
            memory.AddRange(new double[101]);
        }
        public void UpdateGraphData(double input) {
            memory.RemoveAt(0);
            memory.Add(input);
            YAxisData = memory.ToArray();
        }
       




        /*
        public Graphs Teplota1 { get; set; } = new Graphs("Teplota zrcadla", 100);
        public Graphs Vnejsi_Teplota1 { get; set; } = new Graphs("Teplota okolí", 100);
        public Graphs Atmos_press1 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Osvetleni1 { get; set; } = new Graphs("Fotoresistor", 100);
        public Graphs Proud1 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Vykon1 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Teplota_chladice1 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Teplota2 { get; set; } = new Graphs("Teplota zrcadla", 100);
        public Graphs Vnejsi_Teplota2 { get; set; } = new Graphs("Teplota okolí", 100);
        public Graphs Atmos_press2 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Osvetleni2 { get; set; } = new Graphs("Fotoresistor", 100);
        public Graphs Proud2 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Vykon2 { get; set; } = new Graphs("Atmosférický tlak", 100);
        public Graphs Teplota_chladice2 { get; set; } = new Graphs("Atmosférický tlak", 100);
        */

    }

}