﻿using PropertyChanged;
using Rosny_Bod_App;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;

namespace WpfDocumentViewerXps
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    [AddINotifyPropertyChangedInterface]
    public partial class Window1 : Window
    {

        CustomTimer customTimer = new CustomTimer();
        CustomTimer customTimer2 = new CustomTimer();
        public int HeightFix { get; set; } = 400;
        public Window1()

        {
            InitializeComponent();
            var path = Directory.GetCurrentDirectory() + "//" + "help" + "//" + "help.xps";
            //With GetFixedDocumentSequence method, XpsDocument can get XPS file content

            XpsDocument xps = new XpsDocument(path, System.IO.FileAccess.Read);

            documentViewer1.Document = xps.GetFixedDocumentSequence();
           
            DataContext = this;
        }





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




        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var heightfixtemp = Convert.ToInt32((e.NewSize.Height - 50));
            if (heightfixtemp <= 0)
            {
                heightfixtemp = 0;
            }
            HeightFix = heightfixtemp;


        }

        private void How_to_Click(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + "//" + "help" + "//" + "help.xps";
            //With GetFixedDocumentSequence method, XpsDocument can get XPS file content

            XpsDocument xps = new XpsDocument(path, System.IO.FileAccess.Read);

            documentViewer1.Document = xps.GetFixedDocumentSequence();
        }

        private void Electric_blueprint_Click(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + "//" + "help" + "//" + "Elec_blueprint.xps";
            //With GetFixedDocumentSequence method, XpsDocument can get XPS file content

            XpsDocument xps = new XpsDocument(path, System.IO.FileAccess.Read);

            documentViewer1.Document = xps.GetFixedDocumentSequence();
        }

        private void Blueprint_Click(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + "//" + "help" + "//" + "Mech_blueprint.xps";
            XpsDocument xps = new XpsDocument(path, System.IO.FileAccess.Read);

            documentViewer1.Document = xps.GetFixedDocumentSequence();
        }

        private void Function_Click(object sender, RoutedEventArgs e)
        {
            var path = Directory.GetCurrentDirectory() + "//" + "help" + "//" + "Princip.xps";
            //With GetFixedDocumentSequence method, XpsDocument can get XPS file content

            XpsDocument xps = new XpsDocument(path, System.IO.FileAccess.Read);

            documentViewer1.Document = xps.GetFixedDocumentSequence();
        }
    }
}
