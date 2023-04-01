using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Newtonsoft.Json;
using Concept2API;
using System.Windows;
using System.Diagnostics;
using System.IO.Ports;

namespace RCB_Viewer
{
    internal class Backend
    {
        static private List<Concept2Device> devices = new List<Concept2Device>();
        static SerialPort port = new SerialPort(Configurations.Instance.ComPort, Configurations.Instance.BaudRate, Parity.None, 8, StopBits.One);


        internal Backend()
        {
            int init_counter = 1;

            devices = new List<Concept2Device>();
            //Initialise connection(s)
            ushort deviceCount = Concept2Device.Initialize("Concept2 Performance Monitor 5 (PM5)");
            //Start up API(s)
            for (ushort i = 0; i < deviceCount; i++)
            {
                devices.Add(new Concept2Device(i));
                devices[i].Reset();
            }


            Task.Run(() =>
            {
                //Monitor data changes, and display
                while (true)
                {
                    Thread.Sleep(100);
                    if(Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (init_counter % (10*60*10) == 0)
                                {
                                    port.Dispose();
                                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                                    Application.Current.Shutdown();
                                    //reinitialize
                                    //for (ushort i = 0; i < deviceCount; i++)
                                    //{
                                    //    devices[i].Reset();
                                    //}
                                }
                                if (init_counter % 10 == 0)
                                {
                                    //send serial data
                                    port.Open();
                                    port.WriteLine($"{Configurations.Instance.MotorPower}");
                                    port.Close();
                                }
                                init_counter += 1;

                                for (int i = 0; i < devices.Count; i++)
                                {
                                    devices[i].UpdateData();
                                    var _phase = devices[i].GetStrokePhase();
                                    if (_phase == StrokePhase.Catch || _phase == StrokePhase.Idle)
                                    {
                                        Configurations.Instance.Power = 0;
                                    }
                                    else
                                    {
                                        Configurations.Instance.Power = devices[i].GetPower();
                                    }
                                    Configurations.Instance.Distance = devices[i].GetDistance();

                                    //Console.WriteLine("Device " + i + ":" +
                                    //                  "\n    Phase: " + devices[i].GetStrokePhase() +
                                    //                  "\n    Distance: " + devices[i].GetDistance() +
                                    //                  "\n    Drag: " + devices[i].GetDrag() +
                                    //                  "\n    Power: " + devices[i].GetPower() +
                                    //                  "\n    Time: " + devices[i].GetTime());
                                    Debug.WriteLine(Configurations.Instance.Power.ToString());
                                    Debug.WriteLine(Configurations.Instance.Distance.ToString());
                                }
                            }
                            catch(Exception e)
                            {
                                init_counter += 1;
                                Configurations.Instance.LastError = e.ToString();
                            }
                        });
                    }
                }
            });

        } /* constructor */

    } /* class */
}
