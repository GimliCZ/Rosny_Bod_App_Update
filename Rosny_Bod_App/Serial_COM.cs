using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface]
    public class Serial_COM
    {
        /// <summary>
        /// Proměnná držící COM adresu Arduina
        /// </summary>
        public string COM_Adress { get; set; }

        /// <summary>
        /// Modifikovaný serial link
        /// </summary>
        public SerialPort SerialLink { get; set; } = new SerialPort();
        public string LastMsg { get; set; } = "";
        /// <summary>
        /// Proměnná držící v paměti neočekávané odpojení USB
        /// </summary>
        public bool Unexpected_termination { get; set; } = false;

        private List<string> ListOfDevices { get; set; }

        public bool AutodetectArduinoPort() //zdroj.: https://stackoverflow.com/questions/3293889/how-to-auto-detect-arduino-com-port
        {
            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Arduino"))
                    {
                        COM_Adress = deviceId;
                    }
                }
            }
            catch (ManagementException e)
            {
                System.Windows.MessageBox.Show(e.Message);
                COM_Adress = null;
                return false;
            }

            if (COM_Adress == null)
            {
                //System.Windows.MessageBox.Show("Zkontrolujte připojení USB, případně nastavte COM port ručně.");
                return false;
            }
            else
            {
                //System.Windows.MessageBox.Show("Zařízení detekováno na adrese " + COM_Adress);
                return true;
            }
        }

        public void Init_Port()
        {
            try { SerialLink.PortName = COM_Adress; }
            catch { }
            SerialLink.BaudRate = 115200;
            SerialLink.Parity = Parity.None;
            SerialLink.DataBits = 8;
            SerialLink.StopBits = StopBits.One;
            SerialLink.WriteTimeout = 100000;
            SerialLink.ReadTimeout = 100000;
            SerialLink.RtsEnable = true;
            // SerialLink.DtrEnable = true;
            try { SerialLink.Open(); }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Chyba při otevírání portu! ");
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public void SerialComWrite(string text)
        {
            // port.RtsEnable = true;
            try
            {
                LastMsg = text;
                SerialLink.WriteLine(text);
            }
            catch (InvalidOperationException ex) {
                System.Windows.MessageBox.Show(ex.Message);
            }
            catch (TimeoutException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public string SerialComRead() // přečte jednu řádku
        {
            try
            {
                string message = SerialLink.ReadLine();
                if (message == string.Empty)
                {
                    //Console.WriteLine("Prázdný string!");
                    return "EMPTY";
                }
                else
                {
                    //Console.WriteLine(message);
                    return message;
                }
            }
            catch (TimeoutException ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return "DOSLO K CHYBE PRI CTENI STRINGU!";
            }
            //Thread.Sleep(200);
            catch (System.IO.IOException)
            {
                Unexpected_termination = true;
                System.Windows.MessageBox.Show("Zařízení neočekávaně odpojeno.");
                return "Device_Disconnect";
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Chyba komunikace se zařízením. Špatný port?" + e.Message);
                return "Device_Disconnect";
            }

        }

        public void Close_port()
        {
            try
            {
                SerialLink.Close();
                System.Windows.MessageBox.Show("Port uzavřen.");
            }
            catch
            {
                System.Windows.MessageBox.Show("Chyba při zavírání portu!");
            }
        }

        public void GetlistOfSerialDevices(ObservableCollection<string> List) //https://stackoverflow.com/questions/2837985/getting-serial-port-information
        {
            using (var searcher = new ManagementObjectSearcher
                ("SELECT * FROM WIN32_SerialPort"))
            {
                string[] portnames = SerialPort.GetPortNames();
                var ports = searcher.Get().Cast<ManagementBaseObject>().ToList();
                ListOfDevices = (from n in portnames
                                 join p in ports on n equals p["DeviceID"].ToString()
                                 select n + " - " + p["Caption"]).ToList();
            }
            foreach (var item in ListOfDevices)
            {
                List.Add(item);
            }
        }
    }
}