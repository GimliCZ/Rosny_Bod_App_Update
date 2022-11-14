using Nametagshow;
using PropertyChanged;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfDocumentViewerXps;

namespace Rosny_Bod_App
{
    [AddINotifyPropertyChangedInterface] //Implementace propperty change pro standardní proměnné
    public partial class MainWindow : Window
    {

        /// <summary>
        /// List naměřených teplot z PT100
        /// </summary>
        public List<double> TemperaturesList { get; set; } = new List<double>();

        public ObservableCollection<DetectionRecord> Record { get; set; } = new ObservableCollection<DetectionRecord>();

        /// <summary>
        /// Časovač UI
        /// </summary>

        public int GraphUpdatetimer { get; set; } = 0;

        /// <summary>
        /// Časovač Logu
        /// </summary>

        public int Logtimer { get; set; } = 0;

        /// <summary>
        /// Interní timer pro záznam do listu teplot
        /// </summary>
        public int ListUpdatetimer { get; set; } = 0;

        /// <summary>
        /// Proměnná obsahující požadavek na dosaženou teplotu - Pro regulátor
        /// </summary>
        public double RequestedValue { get; set; } = 20;

        /// <summary>
        /// Proměnná vracející string z requested value
        /// </summary>
        public string RequestedString { get; set; }

        /// <summary>
        /// Reakce regulátoru
        /// </summary>
        public double RegulatorResponse { get; set; } = 0;

        /// <summary>
        /// Reakce regulátoru v absolutní hodnotě
        /// </summary>
        public double RegulatorResponseAbs { get; set; } = 0;

        /// <summary>
        /// Konstanta kontrolující aktivitu samonaváděcího režimu
        /// </summary>
        public bool AutomodeActive { get; set; } = false;

        /// <summary>
        /// Konstanta kontrolující aktivitu manuálního režimu
        /// </summary>
        public bool ManualmodeActive { get; set; } = false;

        /// <summary>
        /// Konstanta kontrolující aktivitu vlákna
        /// </summary>
        public bool ThreadActive { get; set; } = false;

        /// <summary>
        /// Konstanta kontrolující aktivitu regulátoru
        /// </summary>
        public bool RegulatorActive { get; set; } = false;

        /// <summary>
        /// Zpětná vazba UI na manuální zadání požadované teploty
        /// </summary>
        public string RequestTemperatureManualText { get; set; } = "Nápověda";

        public string PowerManualText { get; set; } = "Nápověda";

        /// <summary>
        /// Konstanta připravující program na ukončení spojení se zařízením
        /// </summary>
        public bool ReadyForDisconnect { get; set; } = false;

        /// <summary>
        /// Konstanta kontrolující korektnost manuálně zadané hodnoty
        /// </summary>
        public bool RequestOK { get; set; } = false;

        /// <summary>
        /// Manuálně nastavitelná hodnota teploty
        /// </summary>
        public double Number = 20.12;

        /// <summary>
        /// Deklarace funkce pro seriovou komunikaci
        /// </summary>
        public Serial_COM ComPort { get; set; } = new Serial_COM();

        /// <summary>
        /// Deklarace funkce regulátoru
        /// </summary>
        public Regulator HBridgeControl { get; set; } = new Regulator();

        /// <summary>
        /// Deklarace funkce autonavádění
        /// </summary>
        public FindMode Automode { get; set; } = new FindMode();

        /// <summary>
        /// Funkce analyzující seriový provoz
        /// </summary>
        public Serial_Analyzer Report { get; set; } = new Serial_Analyzer();

        /// <summary>
        /// časovač
        /// </summary>
        public CustomTimer Atimer { get; set; } = new CustomTimer();
        public CustomTimer Stabilize_temp { get; set; } = new CustomTimer();
        /// <summary>
        /// Proměnná držící požadavek o ukončení
        /// </summary>
        public bool StopMessurementRequest { get; set; } = false;

        /// <summary>
        /// stavy vypínání
        /// </summary>
        public int MessurementStopStates { get; set; } = 0;

        /// <summary>
        /// proměnná kontrolující správnost hesla
        /// </summary>
        public bool PasswordOk { get; set; } = false;

        /// <summary>
        /// proměnná zamikající UI
        /// </summary>
        public bool NotReadableVariables { get; set; } = false;

        /// <summary>
        /// Funkce umožňující zápis do souborů
        /// </summary>
        public File_COM File_Log { get; set; } = null;
        public File_COM File_Record { get; set; } = null;
        /// <summary>
        /// Proměnná držící dnešní datum
        /// </summary>
        public string CurrentDate { get; set; } = DateTime.Now.ToString("s", new CultureInfo("en-GB")).Replace(':', '-');

        /// <summary>
        /// Cesta k programu
        /// </summary>
        public string current_path = Directory.GetCurrentDirectory();

        /// <summary>
        /// Kolekce COM zařízení
        /// </summary>
        public ObservableCollection<string> ObservableDevices { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Pustit log?
        /// </summary>
        public bool LogON { get; set; } = true;

        public bool AlertTriggered { get; set; } = false; // přehřátí 40°C

        public bool AlertTriggered2 { get; set; } = false; // podchlazení 0°C

        public bool AlertTriggered3 { get; set; } = false; // přehřátí chladiče 60°C

        public int Controlsize { get; set; } = 12;

        public int Controlsize2 { get; set; } = 9;

        public int Controlsize3 { get; set; } = 18;

        public int Heightfix { get; set; } = 130;

        public int Heightfix2 { get; set; } = 400;

        public string Heart { get; set; } = "00";

        public Graphs Teplota1 { get; set; } = new Graphs();
        public Graphs Vnejsi_Teplota1 { get; set; } = new Graphs();
        public Graphs Atmos_press1 { get; set; } = new Graphs();
        public Graphs Osvetleni1 { get; set; } = new Graphs();
        public Graphs Proud1 { get; set; } = new Graphs();
        public Graphs Vykon1 { get; set; } = new Graphs();
        public Graphs Teplota_chladice1 { get; set; } = new Graphs();
        /*  public Graphs Teplota2 { get; set; } = new Graphs( 100);
          public Graphs Vnejsi_Teplota2 { get; set; } = new Graphs( 100);
          public Graphs Atmos_press2 { get; set; } = new Graphs( 100);
          public Graphs Osvetleni2 { get; set; } = new Graphs( 100);
          public Graphs Proud2 { get; set; } = new Graphs( 100);
          public Graphs Vykon2 { get; set; } = new Graphs( 100);
          public Graphs Teplota_chladice2 { get; set; } = new Graphs( 100);*/
        public int oldselect { get; set; } = 1;
        public int com_updater { get; set; } = 0;
        public Options Settings { get; set; } = new Options();
        public bool settings_load_success { get; set; } = false;
        public bool isInicialized { get; set; } = false;
        public double ManualRequestedPowerPositive { get; set; } = 0;
        public double ManualRequestedPowerNegative = 0;
        public bool EnableManualParse { get; set; } = false;
        public int ImageHeightFix { get; set; } = 400;
        public int ImageWeightFix { get; set; } = 710;
        public int AnimationCounter = 0;
        public bool wasExperimentRun = false;
        public FileStream Logger = null;
        public FileStream Records = null;
        public Window1 win2;
        public Window2 win3;
        public int Refresh_delay = 90;
        public double ReqPow { get; set; } = 0;

        public MainWindow()
        {
            InitializeComponent();
            //Navaž spojení s arduinem
            if (!isInicialized)
            {
                ComPort.GetlistOfSerialDevices(ObservableDevices);

                Create_folder("Log"); //IncomingString + ";" + HBridgeControl.SD + ";" + HBridgeControl.SI + ";" + HBridgeControl.SP + ";" + HBridgeControl.U + ";" + Report.CoolerTempSence + ";"+ Report.AmpSence
                Create_folder("Records");

                Createfile("Datalog" + CurrentDate + ".csv", "Log");
                Createfile("Záznam" + CurrentDate + ".csv", "Records");

                File_Log = new File_COM("Datalog" + CurrentDate + ".csv", "Log", true);
                File_Record = new File_COM("Záznam" + CurrentDate + ".csv", "Records", false);

                File_Log.Add_Data("Bezpecnostni bity; Napeti na fotorezistoru [mV]; Teplota venku [°C]; Tlak venku [HPa]; Proud [A]; Teplota Chladice [°C]; Teplota Zrcadla [°C]; Hodnota (D)erivacniho zasahu Regulatoru; Hodnota (I)ntegracniho zasahu Regulatoru; Hodnota (P)roporcionalniho zasahu Regulatoru; Celkovy zasah Regulatoru \r");
                File_Record.Add_Data("Datum;Vnejsi teplota [°C];Vnitrni teplota [°C];Tlak [hPa];Relativni vlhkost [%]\r");

                Graph_selector_1.SelectedIndex = 0;
                Graph_selector_2.SelectedIndex = 0;

                if (Settings.ReadOptions())
                {
                    if (!ComPort.AutodetectArduinoPort())
                    {
                        ComPort.COM_Adress = Settings.COM_adresa;
                    }
                    Automode.Speed_Const = Settings.Speed_Const;
                    speed_TextBox.Text = Settings.Speed_Const.ToString("G", CultureInfo.CurrentCulture);
                    Automode.Speed2_Const = Settings.Speed2_Const;
                    speed2_TextBox.Text = Settings.Speed2_Const.ToString("G", CultureInfo.CurrentCulture);
                    Automode.Low_light_Const = Settings.Low_light_Const;
                    Low_Light_TextBox.Text = Settings.Low_light_Const.ToString("G", CultureInfo.CurrentCulture);
                    Automode.High_light_Const = Settings.High_light_Const;
                    High_Light_TextBox.Text = Settings.High_light_Const.ToString("G", CultureInfo.CurrentCulture);
                    Automode.Inicialstep = Settings.Inicialstep;
                    InicialStep_TextBox.Text = Settings.Inicialstep.ToString("G", CultureInfo.CurrentCulture);
                    Automode.Messuringstep = Settings.Messuringstep;
                    MessuringStep_TextBox.Text = Settings.Messuringstep.ToString("G", CultureInfo.CurrentCulture);
                    HBridgeControl.R0 = Settings.R0;
                    r0_TextBox.Text = Settings.R0.ToString("G", CultureInfo.CurrentCulture);
                    HBridgeControl.Ti = Settings.Ti;
                    Ti_TextBox.Text = Settings.Ti.ToString("G", CultureInfo.CurrentCulture);
                    HBridgeControl.Td = Settings.Td;
                    Td_TextBox.Text = Settings.Td.ToString("G", CultureInfo.CurrentCulture);
                }
                else
                {
                    settings_load_success = false;
                    Settings.Generate_Options(); ;
                }
                /*  WpfPlot1.Plot.AddScatter(Teplota1.XAxisData, Teplota1.YAxisData);
                  WpfPlot1.Plot.XLabel("Vzorky [-]");
                  WpfPlot1.Plot.YLabel("Teplota [°C]");
                  WpfPlot1.Plot.Title("Graf Teploty Senzoru PT100");
                  WpfPlot1.Refresh();*/
                //Binding dat z recordu
                // Auto_Messuring_Results.ItemsSource = record;
                WpfPlot2.Refresh();
                WpfPlot2.Render();
                WpfPlot1.Refresh();
                WpfPlot1.Render();
                var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
                manualPow.KeyDown += new KeyEventHandler(ManualPow_KeyDown);
                Request_temperature_manual.KeyDown += new KeyEventHandler(Request_temperature_manual_KeyDown);
                Password_box.KeyDown += new KeyEventHandler(Password_box_KeyDown);


                DataContext = this;
            }
        }

        private void Request_temperature_manual_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReqConfirm_Click(this,new RoutedEventArgs());
            }
        }

        private void ManualPow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReqPowConfirm_Click(this, new RoutedEventArgs());
            }
        }
        private void Password_box_KeyDown(object sender, KeyEventArgs e){
            if (e.Key == Key.Enter)
            {
                Password_Confirm_Click(this, new RoutedEventArgs());
            }
        }

        private void Connect_to_device_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                ComPort.Init_Port();
            }

            if (ComPort.SerialLink.IsOpen) // Pokud došlo k úspěšnému navázání komunikace, tak pravidelně kontroluj vstupní data
            {
                ReadyForDisconnect = false;
                Disconnect_from_device.IsEnabled = true;
                Disconnect_from_device2.IsEnabled = true;
                Connect_to_device.IsEnabled = false;
                Connect_to_device2.IsEnabled = false;
                Auto_Messurement_Start.IsEnabled = true;
                Manual_Messurement_Start.IsEnabled = true;
                Graph_selector_1.IsEnabled = true;
                Graph_selector_2.IsEnabled = true;

                if (ThreadActive == false) // pokud je puštěn pouze jeden thread
                {
                    Heart = Heart_beat(Heart);
                    Connect_to_device.Background = Brushes.Green;
                    Connect_to_device.IsEnabled = false;
                    Disconnect_from_device.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0"); ;
                    Connect_to_device2.Background = Brushes.Green;
                    Connect_to_device2.IsEnabled = false;
                    Disconnect_from_device2.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0"); ;

                    ComPort.SerialComWrite(Heart + "111111;000;001;000"); // Po připojení spusť plnou komunikaci se zařízením

                    Thread workerThread2 = new Thread(() =>
                    {
                        ThreadActive = true; //zablokování puštění dalšího threadu
                        while (ReadyForDisconnect == false)
                        {
                            Heart = Heart_beat(Heart); //Dokud je zařízení připojeno, generuj hearth beat

                            string IncomingString = ComPort.SerialComRead(); // Vezmi jeden příchozí string
                            Report.AnalyzeString(IncomingString);// Analyzuj ho

                            #region Bezpečnostní funkce

                            if (ComPort.Unexpected_termination)
                            {
                                ComPort.SerialLink.Close();
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Connect_to_device.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0");
                                    Connect_to_device2.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0");
                                    Disconnect_from_device.Background = Brushes.Red;
                                    Disconnect_from_device2.Background = Brushes.Red;
                                    Disconnect_from_device.IsEnabled = false;
                                    Disconnect_from_device2.IsEnabled = false;
                                    Connect_to_device.IsEnabled = true;
                                    Connect_to_device2.IsEnabled = true;
                                    Status.Text = "Neaktivní";
                                    Status.Background = Brushes.LightGray;
                                });
                                break;
                            }

                            if (Report.PT100TempSence > 40) //pokud teplota přesáhne 40°C, spusť ventilátor - dochlazování
                            {
                                ComPort.SerialComWrite(Heart + "111111;000;001;255");
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Status.Text = "Ochrana proti přehřátí aktivní";
                                    Status.Background = Brushes.Orange;
                                });
                                AlertTriggered = true;
                            }
                            else
                            {
                                if (AlertTriggered)
                                {
                                    AlertTriggered = false;
                                }
                            }
                            if (Report.PT100TempSence < 0) // pokud teplota padne pod 0°C, spusť ventilátor a začni ukončovat proces
                            {
                                ComPort.SerialComWrite(Heart + "111111;000;001;255");
                                StopMessurementRequest = true;
                                AlertTriggered2 = true;
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Status.Text = "Ochrana proti podchlazení aktivní";
                                    Status.Background = Brushes.Orange;
                                });
                            }

                            else
                            {
                                if (AlertTriggered2)
                                {
                                    AlertTriggered2 = false;
                                }
                            }
                            if (Report.CoolerTempSence > 60) //V tento moment už je pravděpodobně Peltierův článek poškozen
                            {
                                ComPort.SerialComWrite(Heart + "111111;001;001;255");
                                StopMessurementRequest = true;
                                AlertTriggered3 = true;
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    Status.Text = "Přehřátí chladiče";
                                    Status.Background = Brushes.Red;
                                });
                            }
                            else
                            {
                                if (AlertTriggered3)
                                {
                                    AlertTriggered3 = false;
                                }
                            }
                            #endregion

                            // Modifikace UI a jeho refresh
                            if (StopMessurementRequest)
                            {
                                if (MessurementStopStates == 0)
                                {
                                    App.Current.Dispatcher.Invoke(() =>
                                    {
                                        Connect_to_device.IsEnabled = false;
                                        Connect_to_device2.IsEnabled = false;
                                        Disconnect_from_device.IsEnabled = false;
                                        Disconnect_from_device2.IsEnabled = false;
                                        Auto_Messurement_Start.IsEnabled = false;
                                        Manual_Messurement_Start.IsEnabled = false;
                                    });
                                    RequestedValue = Math.Round(Report.EnvTempReport);
                                    RegulatorActive = true;
                                    ManualmodeActive = false;
                                    AutomodeActive = false;
                                    HBridgeControl.R0 = 80;
                                    HBridgeControl.Ti = 14;
                                    HBridgeControl.Td = 0.002;

                                    Automode.Update(Report.LightSensorReport, Convert.ToDouble(Report.PT100TempSence), Convert.ToDouble(Report.EnvTempReport));
                                    if (Automode.Prepare_for_shutdown(Convert.ToDouble(Report.EnvTempReport)))
                                    {
                                        RegulatorActive = false;
                                        MessurementStopStates = 1;
                                    }

                                }
                                if (MessurementStopStates == 1)
                                {
                                    ComPort.SerialComWrite(Heart + "111111;001;001;255");
                                    Atimer.Start_timer();
                                    if (Atimer.Timer_enlapsed(60))
                                    {
                                        MessurementStopStates = 2;
                                        Atimer.Stop_timer();
                                    }
                                }
                                if (MessurementStopStates == 2)
                                {
                                    ComPort.SerialComWrite(Heart + "111111;001;001;000");
                                    Atimer.Start_timer();
                                    if (Atimer.Timer_enlapsed(1))
                                    {
                                        HBridgeControl.I = 0;
                                        HBridgeControl.P = 0;
                                        HBridgeControl.U = 0;
                                        HBridgeControl.D = 0;
                                        HBridgeControl.SD = HBridgeControl.D.ToString("F2", CultureInfo.CurrentCulture);
                                        HBridgeControl.SI = HBridgeControl.I.ToString("F2", CultureInfo.CurrentCulture);
                                        HBridgeControl.SP = HBridgeControl.P.ToString("F2", CultureInfo.CurrentCulture);
                                        HBridgeControl.Su = HBridgeControl.U.ToString("F2", CultureInfo.CurrentCulture);
                                        MessurementStopStates = 0;
                                        StopMessurementRequest = false;
                                        App.Current.Dispatcher.Invoke(() =>
                                        {
                                            Disconnect_from_device.IsEnabled = true;
                                            Disconnect_from_device2.IsEnabled = true;
                                        });
                                        Atimer.Stop_timer();
                                    }
                                }
                            }
                            if (AutomodeActive)
                            { //pokud je aktivní automatické řízení
                              // pravidelně aktualizuj vstupní parametry
                                Automode.Get_background(Report.LightSensorReport, Report.EnvTempReport); // Nahraj výchozí hodnoty pro autommatické řízení
                                Automode.Update(Report.LightSensorReport, Convert.ToDouble(Report.PT100TempSence), Convert.ToDouble(Report.EnvTempReport));
                                if (Automode.Dewpoint_Detect()) //pokud je detekován rosný bod, tak
                                {
                                    // Console.WriteLine("Našel jsem rosný bod");
                                    // lock (record)
                                    // {
                                    App.Current.Dispatcher.Invoke(() => //udělej záznam do tabulky record
                                    {
                                        Record.Add(new DetectionRecord(Report.PT100TempSence, Report.EnvTempReport, Report.EnvPressureReport, DateTime.Now));
                                        Export_list(); ;
                                    });
                                    // }
                                }
                                RequestedValue = Automode.Temperature_Set(); //nastavení teploty regulátoru
                                RequestedString = RequestedValue.ToString("F4", CultureInfo.InvariantCulture);//zpětná vazba uživateli                                                                            //   Console.WriteLine("request" + requested_value);
                            }

                            if (ManualmodeActive)
                            {
                                //TODO:
                                Automode.Get_background(Report.LightSensorReport, Report.EnvTempReport);
                                ComPort.SerialComWrite(Heart + "111111;" + ManualRequestedPowerPositive.ToString("000") + ";" + ManualRequestedPowerNegative.ToString("000") + ";255");
                            }
                            if (RegulatorActive) //Pokud je aktivní manuální režim proveď:
                            {

                                RegulatorResponse = -Math.Round(HBridgeControl.PIDWithClamping(RequestedValue, Report.PT100TempSence));
                                //Console.WriteLine(H_Bridge_Control.U);
                                if (RegulatorResponse < 0)
                                {
                                    RegulatorResponseAbs = Math.Abs(RegulatorResponse);
                                    //   Console.Write("11111111;" + regulator_response_abs.ToString("000") + ";001;255");
                                    ComPort.SerialComWrite(Heart + "111111;" + RegulatorResponseAbs.ToString("000") + ";000;255");
                                }
                                if (RegulatorResponse == 0)
                                {
                                    ComPort.SerialComWrite(Heart + "111111;001;001;255");
                                    //       Console.Write("11111111;001;001;255");
                                }
                                if (RegulatorResponse > 0)
                                {
                                    RegulatorResponseAbs = Math.Abs(RegulatorResponse);
                                    //          Console.Write("11111111;001;" + regulator_response.ToString("000") + ";255");
                                    ComPort.SerialComWrite(Heart + "111111;000;" + RegulatorResponseAbs.ToString("000") + ";255");
                                }
                            }
                            Logtimer++;
                            if (LogON)
                            {
                                if (Logtimer > Refresh_delay)
                                {
                                    string data = Report.Safetybit1.ToString(CultureInfo.InvariantCulture) + Report.Safetybit2.ToString(CultureInfo.InvariantCulture) + Report.Safetybit3.ToString(CultureInfo.InvariantCulture) +
                                    Report.Safetybit4.ToString(CultureInfo.InvariantCulture) + ";" + Report.LightSensorReport_mV.ToString(CultureInfo.InvariantCulture) + ";" + Report.EnvTempReport.ToString(CultureInfo.InvariantCulture) + ";" +
                                    Report.EnvPressureReport.ToString(CultureInfo.InvariantCulture) + ";" + Report.AmpSence.ToString(CultureInfo.InvariantCulture) + ";" + Report.CoolerTempSence.ToString(CultureInfo.InvariantCulture) + ";" +
                                    Report.PT100TempSence.ToString(CultureInfo.InvariantCulture) + ";" + HBridgeControl.D.ToString(CultureInfo.InvariantCulture) + ";" + HBridgeControl.I.ToString(CultureInfo.InvariantCulture) + ";" +
                                    HBridgeControl.P.ToString(CultureInfo.InvariantCulture) + ";" + HBridgeControl.U.ToString(CultureInfo.InvariantCulture) + ";" + "\r";
                                    File_Log.Add_Data(data);
                                    Logtimer = 0;
                                }
                            }
                        }
                        ThreadActive = false;
                        ComPort.Unexpected_termination = false;
                    });
                    workerThread2.Start();
                }
            }
        }
        #region Triggers
        private void Disconnect_from_device_Click(object sender, RoutedEventArgs e)
        {
            Disconnect_from_device.IsEnabled = false;
            Disconnect_from_device2.IsEnabled = false;
            Auto_Messurement_Start.IsEnabled = false;
            Manual_Messurement_Start.IsEnabled = false;
            Graph_selector_1.IsEnabled = false;
            Graph_selector_2.IsEnabled = false;
            int x = 0;
            ReadyForDisconnect = true;
            Report.LastMessage = "";
            ComPort.LastMsg = "";
            while (true)
            {
                Heart = Heart_beat(Heart);
                ComPort.SerialComWrite(Heart + "111111;001;001;000");
                if (ThreadActive == false)
                {
                    ComPort.SerialLink.Close();
                }
                if (!ComPort.SerialLink.IsOpen)
                {
                    Connect_to_device.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0"); ;
                    Connect_to_device2.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0");
                    Connect_to_device.IsEnabled = true;
                    Connect_to_device2.IsEnabled = true;
                    Disconnect_from_device.Background = Brushes.Red;
                    Disconnect_from_device2.Background = Brushes.Red;
                    Disconnect_from_device.IsEnabled = false;
                    Disconnect_from_device2.IsEnabled = false;
                    Status.Text = "Neaktivní";
                    Status.Background = Brushes.LightGray;
                    break;
                }
                x++;
                if (x > 1000)
                {
                    ComPort.SerialLink.Close();
                    if (!ComPort.SerialLink.IsOpen)
                    {
                        Connect_to_device.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0"); ;
                        Disconnect_from_device.Background = Brushes.Red;
                        Connect_to_device2.Background = (Brush)new BrushConverter().ConvertFrom("#f0f0f0"); ;
                        Disconnect_from_device2.Background = Brushes.Red;
                        Connect_to_device.IsEnabled = true;
                        Connect_to_device2.IsEnabled = true;
                        Disconnect_from_device.IsEnabled = false;
                        Disconnect_from_device2.IsEnabled = false;
                        Status.Text = "Neaktivní";
                        Status.Background = Brushes.LightGray;
                        break;
                    }
                };
            }
        }

        private void Manual_Messurement_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                MessageBox.Show("Zařízení není připojeno.");
                return;
            }

            Auto_Messurement_Start.IsEnabled = false;
            wasExperimentRun = true;
            manualPow.IsEnabled = true;
            ReqPowConfirm.IsEnabled = true;
            Manual_Messurement_Start.IsEnabled = false;
            Manual_Messurement_Stop.IsEnabled = true;
            Disconnect_from_device.IsEnabled = false;
            Auto_Messuring_Results.IsEnabled = false;

            AutomodeActive = false;
            ManualmodeActive = true;

            Status.Text = "Manuální režim aktivní";
            Status.Background = Brushes.Green;
            ComPort.SerialComWrite(Heart + "111111;001;001;255"); // Po spuštění měření pust ventilátor na plno
        }

        private void Manual_Messurement_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                MessageBox.Show("Zařízení není připojeno.");
                return;
            }
            AutomodeActive = false;
            Auto_Messurement_Stop.IsEnabled = false;
            Automatic_temp_control_ON.IsChecked = true;
            Manual_Messurement_Stop.IsEnabled = false;
            manualPow.IsEnabled = false;
            ReqPowConfirm.IsEnabled = false;
            ManualRequestedPowerNegative = 0;
            ManualRequestedPowerPositive = 0;
            manualPow.Text = "0";
            ReqPow = 0;
            Auto_Messuring_Results.IsEnabled = true;
            if (ManualmodeActive)
            {
                StopMessurementRequest = true;
                Status.Text = "Zastavuji měření";
                Status.Background = Brushes.Yellow;
                ManualmodeActive = false;
            }
            else
            {
                MessageBox.Show("Není co zastavovat.");
            }
        }

        private void Request_temperature_manual_Text_Changed(object sender, TextChangedEventArgs e)
        {
            var culture = CultureInfo.InvariantCulture;
            NumberStyles styles = NumberStyles.Number;

            string requested = Request_temperature_manual.Text;
            if (!requested.Contains(","))
            {
                if (double.TryParse(requested, styles, culture, out Number))
                {
                    if (Number > 1)
                    {
                        if (Number < Report.EnvTempReport)
                        {
                            RequestTemperatureManualText = "OK potvrďte zápis";
                            RequestOK = true;
                        }
                        else
                        {
                            RequestTemperatureManualText = "teplota > okolí";
                            RequestOK = false;
                        }
                    }
                    else
                    {
                        RequestTemperatureManualText = "Moc nízká teplota";
                        RequestOK = false;
                    }
                }
                else
                {
                    RequestTemperatureManualText = "CHYBA";
                    RequestOK = false;
                }
            }
            else
            {
                requested= requested.Replace(',', '.');
                if (double.TryParse(requested, styles, culture, out Number))
                {
                    if (Number > 1)
                    {
                        if (Number < Report.EnvTempReport)
                        {
                            RequestTemperatureManualText = "OK potvrďte zápis";
                            RequestOK = true;
                        }
                        else
                        {
                            RequestTemperatureManualText = "teplota > okolí";
                            RequestOK = false;
                        }
                    }
                    else
                    {
                        RequestTemperatureManualText = "Moc nízká teplota";
                        RequestOK = false;
                    }
                }
                else
                {
                    RequestTemperatureManualText = "Neplatné číslo";
                    RequestOK = false;
                }
            }

        }

        private void ReqConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (RequestOK)
            {
                RequestedValue = Number;
            }
            else
            {
                RequestedValue = Math.Round((double)Report.EnvTempReport, 2);
            }
        }

        private void Request_temperature_manual_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var culture = CultureInfo.InvariantCulture;
            NumberStyles styles = NumberStyles.Number;

            string requested = Request_temperature_manual.Text;
            if (!requested.Contains(","))
            {
                if (double.TryParse(requested, styles, culture, out Number))
                {
                    if (Number > 1)
                    {
                        if (Number < Report.EnvTempReport)
                        {
                            RequestTemperatureManualText = "OK potvrďte zápis";
                            RequestOK = true;
                        }
                        else
                        {
                            RequestTemperatureManualText = "teplota > okolí";
                            RequestOK = false;
                        }
                    }
                    else
                    {
                        RequestTemperatureManualText = "Moc nízká teplota";
                        RequestOK = false;
                    }
                }
                else
                {
                    RequestTemperatureManualText = "Neplatné číslo";
                    RequestOK = false;
                }
            }
            else
            {
                requested = requested.Replace(',', '.');
                if (double.TryParse(requested, styles, culture, out Number))
                {
                    if (Number > 1)
                    {
                        if (Number < Report.EnvTempReport)
                        {
                            RequestTemperatureManualText = "OK potvrďte zápis";
                            RequestOK = true;
                        }
                        else
                        {
                            RequestTemperatureManualText = "teplota > okolí";
                            RequestOK = false;
                        }
                    }
                    else
                    {
                        RequestTemperatureManualText = "Moc nízká teplota";
                        RequestOK = false;
                    }
                }
                else
                {
                    RequestTemperatureManualText = "Neplatné číslo";
                    RequestOK = false;
                }
            }
        }

        private void Auto_Messurement_Start_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                MessageBox.Show("Zařízení není připojeno.");
                return;
            }
            //automode assumed active
            Add_row2.IsEnabled = false;
            Remove_row2.IsEnabled = false;
            Add_row.IsEnabled = false;
            Remove_row.IsEnabled = false;
            wasExperimentRun = true;
            Auto_Messurement_Start.IsEnabled = false;
            Auto_Messurement_Stop.IsEnabled = true;
            Manual_Messurement_Start.IsEnabled = false;
            Automatic_temp_control_ON.IsEnabled = true;
            Disconnect_from_device.IsEnabled = false;
            Auto_Messuring_Results.IsEnabled = false;
            //automode enabled?
            AutomodeActive = true;
            ManualmodeActive = false;
            Add_row2.IsEnabled = false;
            Remove_row2.IsEnabled = false;
            Add_row.IsEnabled = false;
            Remove_row.IsEnabled = false;

            Status.Text = "Automatický režim aktivní";
            Status.Background = Brushes.Green;

            RegulatorActive = true;

            ComPort.SerialComWrite(Heart + "111111;001;001;255");
        }

        private void Auto_Messurement_Stop_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                MessageBox.Show("Zařízení není připojeno.");
                return;
            }
            Automatic_temp_control_ON.IsChecked = true;
            Automatic_temp_control_ON.IsEnabled = false;
            ReqConfirm.IsEnabled = false;
            Request_temperature_manual.IsEnabled = false;
            Add_row2.IsEnabled = true;
            Remove_row2.IsEnabled = true;
            Add_row.IsEnabled = true;
            Remove_row.IsEnabled = true;
            AutomodeActive = false;
            ManualmodeActive = false;
            Auto_Messurement_Stop.IsEnabled = false;
            Manual_Messurement_Stop.IsEnabled = false;
            Auto_Messuring_Results.IsEnabled = true;

            if (RegulatorActive)
            {
                //regulator will run here
                StopMessurementRequest = true;
                Status.Text = "Zastavuji měření";
                Status.Background = Brushes.Yellow;
            }
            else
            {
                MessageBox.Show("Není co zastavovat."); //shoudlnt show
            }
        }

        private void Password_Confirm_Click(object sender, RoutedEventArgs e)
        {
            List<string> list = new List<string>();
            list.Add("dew point");
            list.Add("dewpoint");
            list.Add("DEWPOINT");
            list.Add("DEW POINT");

            if (list.Contains(Password_box.Text.ToString().ToUpperInvariant()) )
            {
                PasswordOk = true;
                Password_box.Background = Brushes.Green;
            }
            else
            {
                PasswordOk = false;
                if (!list.Contains(Password_box.Text.ToString().ToUpperInvariant()))
                {
                    Password_box.Background = Brushes.Red;
                }
                else
                {
                    Password_box.Background = Brushes.Green;
                }
            }
            NotReadableVariables = !PasswordOk;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Automode.Forceupdatebackground(Report.EnvTempReport);
            var culture = CultureInfo.InvariantCulture;
            NumberStyles styles = NumberStyles.Number;
            double r0 = 0;
            if (PasswordOk)
            {
                if (!r0_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(r0_TextBox.Text, styles, culture, out r0))
                    {
                        HBridgeControl.R0 = r0;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné R0.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná R0 obsahuje ',' nahraďte ji prosím '.'");
                }

                double Ti = 0;
                if (!Ti_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(Ti_TextBox.Text, styles, culture, out Ti))
                    {
                        HBridgeControl.Ti = Ti;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Ti.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Ti obsahuje ',' nahraďte ji prosím '.'");
                }

                double Td = 0;
                if (!Td_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(Td_TextBox.Text, styles, culture, out Td))
                    {
                        HBridgeControl.Td = Td;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Td.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Td obsahuje ',' nahraďte ji prosím '.'");
                }

                double low_light = 0;
                if (!Low_Light_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(Low_Light_TextBox.Text, styles, culture, out low_light))
                    {
                        Automode.Low_light_Const = low_light;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Hranice chlazení.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Hranice chlazení obsahuje ',' nahraďte ji prosím '.'");
                }
                double high_light = 0;
                if (!High_Light_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(High_Light_TextBox.Text, styles, culture, out high_light))
                    {
                        Automode.High_light_Const = high_light;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Hranice zahřívání.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Hranice zahřívání obsahuje ',' nahraďte ji prosím '.'");
                }
                double speed = 0;
                if (!speed_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(speed_TextBox.Text, styles, culture, out speed))
                    {
                        Automode.Speed_Const = speed;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Zpoždění náběhu.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Zpoždění náběhu obsahuje ',' nahraďte ji prosím '.'");
                }

                double speed2 = 0;
                if (!speed2_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(speed2_TextBox.Text, styles, culture, out speed2))
                    {
                        Automode.Speed2_Const = speed2;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Zpoždění měření.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Zpoždění měření obsahuje ',' nahraďte ji prosím '.'");
                }

                double messuringstep = 0;
                if (!MessuringStep_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(MessuringStep_TextBox.Text, styles, culture, out messuringstep))
                    {
                        Automode.Messuringstep = messuringstep;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Měřící krok.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Měřící krok obsahuje ',' nahraďte ji prosím '.'");
                }

                double InicialStep = 0;
                if (!InicialStep_TextBox.Text.Contains(","))
                {
                    if (double.TryParse(InicialStep_TextBox.Text, styles, culture, out InicialStep))
                    {
                        Automode.Inicialstep = InicialStep;
                    }
                    else
                    {
                        MessageBox.Show("Nastala chyba při převodu čísla U proměnné Měřící krok.");
                    }
                }
                else
                {
                    MessageBox.Show("Proměnná Měřící krok obsahuje ',' nahraďte ji prosím '.'");
                }
                MessageBox.Show("Proměnné úspěšně zapsány.");
            }
            else
            {
                MessageBox.Show("Heslo není správné!");
            }
        }

        private void Close_app(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            /* while (true)
             {*/

            if (ThreadActive == false)
            {
                //ComPort.SerialComWrite("11111111;001;001;000");
                ComPort.SerialLink.Close();
                // break;
                App.Current.Dispatcher.Invoke(() =>
                {
                    if (wasExperimentRun)
                    {
                        File_Log.Close();
                        File_Log.Dispose();
                        File_Record.Close();
                        File_Record.Dispose();
                    }
                    else
                    {
                        File_Log.File_Delete();
                        File_Record.File_Delete();
                    }
                });

                if (win2 != null)
                {
                    win2.Close();
                }

                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("Zastavte měření a ukončete komunikaci se zařízením.");
            }
            //}
        }

        private void Export_list()
        {
            string data = "";
            foreach (var DetectionRecord in Record)
            {
                data += DetectionRecord.Time.ToString("f", new CultureInfo("en-GB")) + ";" + DetectionRecord.ENV_Temperature.ToString(CultureInfo.InvariantCulture) + ";" + DetectionRecord.PT100_Temperature.ToString(CultureInfo.InvariantCulture) + ";" + DetectionRecord.ENV_Pressure.ToString(CultureInfo.InvariantCulture) + ";" + DetectionRecord.Humidity.ToString(CultureInfo.InvariantCulture) + System.Environment.NewLine;
            }
            File_Record.Add_Data(data);
        }

        private void Remove_row_Click(object sender, RoutedEventArgs e)
        {
            int selectedRowCount = Auto_Messuring_Results.SelectedIndex;
            if (selectedRowCount > -1)
            {
                Record.RemoveAt(Auto_Messuring_Results.SelectedIndex);
            }
            Auto_Messuring_Results.Items.Refresh();
            Export_list();
        }

        private void Add_row_Click(object sender, RoutedEventArgs e)
        {
            int selectedRowCount = Auto_Messuring_Results.SelectedIndex;
            if (selectedRowCount > -1)
            {
                Record.Insert(Auto_Messuring_Results.SelectedIndex, new DetectionRecord(Report.PT100TempSence, Report.EnvTempReport, Report.EnvPressureReport, DateTime.Now));
            }
            else
            {
                Record.Add(new DetectionRecord(Report.PT100TempSence, Report.EnvTempReport, Report.EnvPressureReport, DateTime.Now));
            }
            Auto_Messuring_Results.Items.Refresh();
            Export_list();
        }


        private void Run_fan_Click(object sender, RoutedEventArgs e)
        {
            if (!ComPort.SerialLink.IsOpen)
            {
                MessageBox.Show("Zařízení není připojeno.");
                return;
            }
            ComPort.SerialComWrite(Heart + "111111;000;001;255");
        }

        private void SerialList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string temp = (string)SerialList.SelectedItem;
            string[] temp2 = temp.Split(' ');
            ComPort.COM_Adress = temp2[0];
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState.Maximized == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_default.png")));
            }

            if (WindowState.Normal == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_default.png")));
            }

            int rozsah_min = 12;//modifikace fontu
            int rozsah_max = 14;
            int rozsah_min2 = 10;
            int rozsah_max2 = 14;
            Controlsize = Convert.ToInt32(e.NewSize.Width - 800) * (rozsah_max - rozsah_min) / (1920 - 800) + rozsah_min;
            Controlsize2 = Convert.ToInt32((e.NewSize.Width - 800) * (rozsah_max2 - rozsah_min2) / (1920 - 800) + rozsah_min2);
            Controlsize3 = Controlsize2 * 2;
            var heightfixtemp = Convert.ToInt32((e.NewSize.Height - 600) * (610 - 130) / (1080 - 600) + 130); //modifikace velikosti čítače naměřených hodnot
            var imageHeightFix = Convert.ToInt32((e.NewSize.Height - 600) * (975 - 500) / (1080 - 600) + 500);
            if (heightfixtemp <= 0)
            {
                Heightfix = 0;
                ImageHeightFix = 0;
                ImageWeightFix = 0;
            }
            else
            {
                ImageHeightFix = imageHeightFix;
                ImageWeightFix = imageHeightFix * 16 / 9;
                Heightfix = heightfixtemp;
                Heightfix2 = heightfixtemp + 270;
            }
        }


        private void Automatic_temp_control_ON_Checked(object sender, RoutedEventArgs e)
        {
            if (Automatic_temp_control_ON.IsChecked == true && StopMessurementRequest == false && ManualmodeActive == false && AutomodeActive == false)
            {
                if (IsLoaded)
                {
                    Add_row2.IsEnabled = false;
                    Remove_row2.IsEnabled = false;
                    Add_row.IsEnabled = false;
                    Remove_row.IsEnabled = false;
                    Automode.ResetTemperatureMessurement();
                    AutomodeActive = true;
                    Request_temperature_manual.IsEnabled = false;
                    ReqConfirm.IsEnabled = false;
                }
            }
        }

        private void Automatic_temp_control_ON_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Automatic_temp_control_ON.IsChecked == false && StopMessurementRequest == false && ManualmodeActive == false && AutomodeActive == true)
            {
                if (IsLoaded)
                {
                    Add_row2.IsEnabled = true;
                    Remove_row2.IsEnabled = true;
                    Add_row.IsEnabled = true;
                    Remove_row.IsEnabled = true;
                    AutomodeActive = false;
                    Request_temperature_manual.IsEnabled = true;
                    ReqConfirm.IsEnabled = true;
                }
            }
        }

        private void Magic_Click(object sender, RoutedEventArgs e)
        {
            Password_box.Text = "dew point";
        }
        public string Heart_beat(string Heart)
        {
            if (Heart == "00")
            {
                return "01";
            }
            if (Heart == "01")
            {
                return "10";
            }
            if (Heart == "10")
            {
                return "11";
            }
            if (Heart == "11")
            {
                return "00";
            }
            return "00";
        }
        CustomTimer customTimer = new CustomTimer();
        CustomTimer customTimer2 = new CustomTimer();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var currentstate = WindowState;

            if (e.LeftButton == MouseButtonState.Pressed && Dragfield.IsMouseOver == true)
            {
                DragMove();
                if (currentstate == WindowState.Maximized)
                {

                    // Application.Current.MainWindow.WindowState = WindowState.Normal;
                    Point locationFromScreen = this.Dragfield.PointToScreen(new Point(0, 0));
                    PresentationSource source = PresentationSource.FromVisual(this);
                    System.Windows.Point targetPoints = source.CompositionTarget.TransformFromDevice.Transform(locationFromScreen);
                    Point temp = e.GetPosition(this);
                    WindowState = WindowState.Normal;
                    Left = targetPoints.X + temp.X / 2;
                    Top = targetPoints.Y + temp.Y / 2;
                    DragMove();
                }
            }
            if (customTimer.Running == false)
            {
                customTimer.Start_timer();
            }
            else
            {
                if (!customTimer.Timer_enlapsed(0.3))
                {
                    bool changedone = false;
                    if (WindowState.Normal == WindowState && changedone == false)
                    {
                        Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_default.png")));
                        WindowState = WindowState.Maximized;
                        changedone = true;
                    }
                    if (WindowState.Maximized == WindowState && changedone == false)
                    {
                        Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_default.png")));
                        WindowState = WindowState.Normal;

                    }
                }
                customTimer.Stop_timer();
                customTimer.Start_timer();
            }
        }

        private void Exit_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Exit_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Exit_pressed.png")));
        }

        private void Exit_button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        /*  private void Exit_button_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs E)
          {
              var tempimage = new Image()
              {
                  Height = 25,
                  Width = 40,
                  HorizontalAlignment = System.Windows.HorizontalAlignment.Right
              };
              Exit_button.Source = new BitmapImage(new Uri (("pack://application:,,,/img/Exit_howered.png")));
          }*/

        private void Exit_button_MouseLeave(object sender, MouseEventArgs e)
        {
            Exit_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Exit_default.png"))); ;
        }

        private void Exit_button_MouseEnter(object sender, MouseEventArgs e)
        {
            Exit_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Exit_howered.png")));
        }

        private void Maximize_button_MouseEnter(object sender, MouseEventArgs e)
        {
            if (WindowState.Normal == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_howered.png")));
            }
            if (WindowState.Maximized == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_hower.png")));
            }
        }


        private void Maximize_button_MouseLeave(object sender, MouseEventArgs e)
        {
            if (WindowState.Normal == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_default.png")));
            }
            if (WindowState.Maximized == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_default.png")));
            }
        }

        private void Maximize_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState.Normal == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_pressed.png")));
            }
            if (WindowState.Maximized == WindowState)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_pressed.png")));
            }
        }

        private void Maximize_button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            bool changedone = false;
            if (WindowState.Normal == WindowState && changedone == false)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/ReturnMaximize_default.png")));
                WindowState = WindowState.Maximized;
                changedone = true;
            }
            if (WindowState.Maximized == WindowState && changedone == false)
            {
                Maximize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Maximize_default.png")));
                WindowState = WindowState.Normal;

            }

        }

        private void Minimize_button_MouseEnter(object sender, MouseEventArgs e)
        {
            Minimize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Minimize_howered.png")));
        }

        private void Minimize_button_MouseLeave(object sender, MouseEventArgs e)
        {
            Minimize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Minimize_default.png")));
        }

        private void Minimize_button_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Minimize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Minimize_pressed.png")));
        }

        private void Minimize_button_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Minimize_button.Source = new BitmapImage(new Uri(("pack://application:,,,/img/Minimize_default.png")));
            WindowState = WindowState.Minimized;
        }

        private void Graph_selector_1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GraphSelect();

            /*
                <ComboBoxItem Content="Teplota zrcadla" />
                <ComboBoxItem Content="Teplota okolí" />
                <ComboBoxItem Content="Atmosférický tlak" />
                <ComboBoxItem Content="Fotoresistor" />
                <ComboBoxItem Content="Proud" />
                <ComboBoxItem Content="Výkon" />
                <ComboBoxItem Content="Teplota chladiče" />
*/


        }
        private void Graph_selector_2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GraphSelect2();
            /*
                <ComboBoxItem Content="Teplota zrcadla" />
                <ComboBoxItem Content="Teplota okolí" />
                <ComboBoxItem Content="Atmosférický tlak" />
                <ComboBoxItem Content="Fotoresistor" />
                <ComboBoxItem Content="Proud" />
                <ComboBoxItem Content="Výkon" />
                <ComboBoxItem Content="Teplota chladiče" />
*/


        }
        public void GraphSelect()
        {
            if (Graph_selector_1.SelectedIndex == 0)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Teplota1.XAxisData, Teplota1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Teplota [°C]");
                WpfPlot1.Plot.Title("Graf Teploty Na Povrchu Zrcadla");
                WpfPlot1.Plot.SetAxisLimitsY(-2, 35);
            }
            if (Graph_selector_1.SelectedIndex == 1)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Vnejsi_Teplota1.XAxisData, Vnejsi_Teplota1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Teplota [°C]");
                WpfPlot1.Plot.Title("Graf Teploty Okolí");
                WpfPlot1.Plot.SetAxisLimitsY(-2, 30);
            }
            if (Graph_selector_1.SelectedIndex == 2)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Atmos_press1.XAxisData, Atmos_press1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Tlak [HPa]");
                WpfPlot1.Plot.Title("Graf Atmosférického Tlaku");
                WpfPlot1.Plot.SetAxisLimitsY(870, 1084);
            }
            if (Graph_selector_1.SelectedIndex == 3)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Osvetleni1.XAxisData, Osvetleni1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Napětí Na Fotorezistoru [mV]");
                WpfPlot1.Plot.Title("Graf Intenzity Osvětlení");
                WpfPlot1.Plot.SetAxisLimitsY(1000, 1750);
            }
            if (Graph_selector_1.SelectedIndex == 4)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Proud1.XAxisData, Proud1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Proud [A]");
                WpfPlot1.Plot.Title("Graf Proudu v Čase");
                WpfPlot1.Plot.SetAxisLimitsY(0, 5);
            }
            if (Graph_selector_1.SelectedIndex == 5)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Vykon1.XAxisData, Vykon1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Příkon [W]");
                WpfPlot1.Plot.Title("Graf Příkonu V Čase");
                WpfPlot1.Plot.SetAxisLimitsY(0, 65);
            }
            if (Graph_selector_1.SelectedIndex == 6)
            {
                WpfPlot1.Plot.Clear();
                WpfPlot1.Plot.AddScatter(Teplota_chladice1.XAxisData, Teplota_chladice1.YAxisData);
                WpfPlot1.Plot.XLabel("Vzorky [-]");
                WpfPlot1.Plot.YLabel("Teplota Chladiče [°C]");
                WpfPlot1.Plot.Title("Graf Teploty Chladiče V Čase");
                WpfPlot1.Plot.SetAxisLimitsY(0, 100);
            }

        }
        public void GraphSelect2()
        {
            if (Graph_selector_2.SelectedIndex == 0)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Teplota1.XAxisData, Teplota1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Teplota [°C]");
                WpfPlot2.Plot.Title("Graf Teploty Na Povrchu Zrcadla");
                WpfPlot2.Plot.SetAxisLimitsY(-2, 35);
            }
            if (Graph_selector_2.SelectedIndex == 1)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Vnejsi_Teplota1.XAxisData, Vnejsi_Teplota1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Teplota [°C]");
                WpfPlot2.Plot.Title("Graf Teploty Okolí");
                WpfPlot2.Plot.SetAxisLimitsY(-2, 30);
            }
            if (Graph_selector_2.SelectedIndex == 2)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Atmos_press1.XAxisData, Atmos_press1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Tlak [HPa]");
                WpfPlot2.Plot.Title("Graf Atmosférického Tlaku");
                WpfPlot2.Plot.SetAxisLimitsY(870, 1084);
            }
            if (Graph_selector_2.SelectedIndex == 3)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Osvetleni1.XAxisData, Osvetleni1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Napětí Na Fotorezistoru [mV]");
                WpfPlot2.Plot.Title("Graf Intenzity Osvětlení");
                WpfPlot2.Plot.SetAxisLimitsY(1000, 1750);
            }
            if (Graph_selector_2.SelectedIndex == 4)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Proud1.XAxisData, Proud1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Proud [A]");
                WpfPlot2.Plot.Title("Graf Proudu v Čase");
                WpfPlot2.Plot.SetAxisLimitsY(0, 5);
            }
            if (Graph_selector_2.SelectedIndex == 5)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Vykon1.XAxisData, Vykon1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Příkon [W]");
                WpfPlot2.Plot.Title("Graf Příkonu V Čase");
                WpfPlot2.Plot.SetAxisLimitsY(0, 65);
            }
            if (Graph_selector_2.SelectedIndex == 6)
            {
                WpfPlot2.Plot.Clear();
                WpfPlot2.Plot.AddScatter(Teplota_chladice1.XAxisData, Teplota_chladice1.YAxisData);
                WpfPlot2.Plot.XLabel("Vzorky [-]");
                WpfPlot2.Plot.YLabel("Teplota Chladiče [°C]");
                WpfPlot2.Plot.Title("Graf Teploty Chladiče V Čase");
                WpfPlot2.Plot.SetAxisLimitsY(0, 100);
            }

        }

        private void manualPow_TextChanged(object sender, TextChangedEventArgs e)
        {
            var culture = CultureInfo.InvariantCulture;
            NumberStyles styles = NumberStyles.Number;

            if (!manualPow.Text.Contains(","))
            {
                if (double.TryParse(manualPow.Text, styles, culture, out Number))
                {
                    Number = Math.Round(Number, 2);
                    if (Number >= -4)
                    {
                        if (Number <= 100)
                        {
                            PowerManualText = "OK potvrďte zápis";
                            EnableManualParse = true;
                        }
                        else
                        {
                            PowerManualText = "Moc vysoký výkon";
                            EnableManualParse = false;
                        }
                    }

                    else
                    {
                        PowerManualText = "Moc nízký výkon";
                        EnableManualParse = false;
                    }
                }
                else
                {
                    PowerManualText = "Neplatné číslo";
                    EnableManualParse = false;
                }
            }
            else
            {
                manualPow.Text = manualPow.Text.Replace(',', '.');
                if (double.TryParse(manualPow.Text, styles, culture, out Number))
                {
                    Number = Math.Round(Number, 2);
                    if (Number > -4)
                    {
                        if (Number < 100)
                        {
                            PowerManualText = "OK potvrďte zápis";
                            EnableManualParse = true;
                        }
                        else
                        {
                            PowerManualText = "Moc vysoký výkon";
                            EnableManualParse = false;
                        }
                    }

                    else
                    {
                        PowerManualText = "Moc nízký výkon";
                        EnableManualParse = false;
                    }
                }
                else
                {
                    PowerManualText = "Neplatné číslo";
                    EnableManualParse = false;
                }
            }
        }

        private void ReqPowConfirm_Click(object sender, RoutedEventArgs e)
        {
            var culture = CultureInfo.InvariantCulture;
            NumberStyles styles = NumberStyles.Number;
            double ReqPowRound;
            double tempnumber;
            if (double.TryParse(manualPow.Text, styles, culture, out tempnumber) == true && EnableManualParse == true)
            {
                ReqPow = Math.Round(tempnumber, 2);
                ReqPowRound = Math.Round(ReqPow / 100 * 255);
                if (ReqPowRound < 0)
                {
                    ManualRequestedPowerPositive = 0;
                    ManualRequestedPowerNegative = Math.Abs(ReqPowRound);

                }
                if (ReqPowRound > 0)
                {
                    ManualRequestedPowerPositive = Math.Abs(ReqPowRound);
                    ManualRequestedPowerNegative = 0;
                }
                if (ReqPowRound == 0)
                {
                    ManualRequestedPowerPositive = 1;
                    ManualRequestedPowerNegative = 0;
                }
            }

        }
        public void Create_folder(string foldername) //Založ složku
        {
            try
            {
                Directory.CreateDirectory(current_path + "//" + foldername);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void Createfile(string filename, string foldername)
        {
            try
            {
                var file = File.Create(current_path + "\\" + foldername + "\\" + filename);
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            win2 = new Window1();
            win2.Show();
        }
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        private void MenuItem_Click2(object sender, RoutedEventArgs e)
        {
            win3 = new Window2();
            win3.Show();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            if (ComPort.SerialLink.IsOpen)
            {
                Teplota1.UpdateGraphData(Report.PT100TempSence);
                Vnejsi_Teplota1.UpdateGraphData(Report.EnvTempReport);
                Atmos_press1.UpdateGraphData(Report.EnvPressureReport);
                Osvetleni1.UpdateGraphData(Report.LightSensorReport_mV);
                Proud1.UpdateGraphData(Report.AmperageShow);
                Vykon1.UpdateGraphData(Report.WattageShow);
                Teplota_chladice1.UpdateGraphData(Report.CoolerTempSence);

                App.Current.Dispatcher.Invoke(() =>
                {

                #region Graph_Update
                    GraphSelect();
                    GraphSelect2();
                    WpfPlot2.Refresh();
                    WpfPlot2.Render();
                    WpfPlot1.Refresh();
                    WpfPlot1.Render();
                #endregion
                #region Záznam komunikace 

                    comm_field.AppendText(Report.LastMessage + '\r');
                    comm_field.AppendText(ComPort.LastMsg + '\r');
                    if (com_updater > Heightfix2 / 40)
                    {
                        comm_field.SelectAll();
                        comm_field.Selection.Text = "";
                        com_updater = 0;
                    }
                    com_updater++;
                #endregion
                #region Stoping_States
                    if (StopMessurementRequest && MessurementStopStates == 0)
                    {


                        Status.Text = "Stabilizuji teplotu regulátorem";
                        Status.Background = Brushes.LightGreen;
                    }
                    if (StopMessurementRequest && MessurementStopStates == 1)
                    {
                        Status.Text = "Chladím " + Math.Abs(60 - Atimer.Difference.TotalSeconds).ToString("F2", CultureInfo.CurrentCulture) + "s";
                        Status.Background = Brushes.LightGreen;
                    }
                    if (!StopMessurementRequest && !RegulatorActive && !AutomodeActive && !ManualmodeActive)
                    {
                        Status.Text = "Neaktivní";
                        Status.Background = Brushes.LightGray;
                        Auto_Messurement_Start.IsEnabled = true;
                        Manual_Messurement_Start.IsEnabled = true;
                    }
                #endregion
                });
            }

            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
#endregion