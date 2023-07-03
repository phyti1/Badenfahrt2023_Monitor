using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Resources;
using System.Windows.Media;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace RCB_Viewer
{
    internal class Configurations : INotifyPropertyChanged
    {
        [JsonIgnore]
        internal static bool _configIsLoading = false;

        private static Configurations _instance = null;
        public static Configurations Instance
        {
            get
            {
                if (_instance == null)
                {
                    Deserialize(true);
                }
                return _instance;
            }
            private set
            {
                if (_instance == null || !_configIsLoading)
                {
                    _instance = value;
                    _configIsLoading = false;
                }
            }
        }

        internal static void CreateNew()
        {
            _configIsLoading = true;
            Configurations.Instance = new Configurations()
            {

            };
            _configIsLoading = false;
        }

        [JsonIgnore]
        public Backend Backend { get; set; }

        private bool _isTesting = false;
        [JsonIgnore]
        public ICommand TestCommand { get; } = new RelayCommand(async (args) =>
        {
            if (Configurations.Instance._isTesting) { return; }
            Configurations.Instance._isTesting = true;
            await Task.Run(() =>
            {
                foreach (int i in Enumerable.Range(1, 5))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Configurations.Instance.Power += (Instance.MotorPowerMax - Instance.MotorPowerMin) / 5;
                    });
                    Thread.Sleep(3000);
                }
                Thread.Sleep(10000);
                foreach (int i in Enumerable.Range(1, 2))
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Configurations.Instance.Power -= (Instance.MotorPowerMax - Instance.MotorPowerMin) / 2;
                    });
                    Thread.Sleep(10000);
                }
            });
            Configurations.Instance._isTesting = false;
        });


        private int _power;
        [JsonIgnore]
        public int Power
        {
            get => _power;
            set
            {
                _power = value;
                MotorPower = value;
                LightPower = value;
                OnPropertyChanged();
                if (_power > _bestPower)
                {
                    BestPower = _power;
                }
            }
        }
        private int _motorPowerMin = 0;
        public int MotorPowerMin
        {
            get => _motorPowerMin;
            set
            {
                _motorPowerMin = value;
                OnPropertyChanged();
            }
        }
        private int _motorPowerMax = 1000;
        public int MotorPowerMax
        {
            get => _motorPowerMax;
            set
            {
                _motorPowerMax = value;
                OnPropertyChanged();
            }
        }
        private int _lightPowerMin = 250;
        public int LightPowerMin
        {
            get => _lightPowerMin;
            set
            {
                _lightPowerMin = value;
                OnPropertyChanged();
            }
        }
        private int _lightPowerMax = 1000;
        public int LightPowerMax
        {
            get => _lightPowerMax;
            set
            {
                _lightPowerMax = value;
                OnPropertyChanged();
            }
        }

        private int _baudRate = 9600;
        public int BaudRate
        {
            get => _baudRate;
            set
            {
                _baudRate = value;
                OnPropertyChanged();
            }
        }
        private string _comPort = "COM1";
        public string ComPort
        {
            get => _comPort;
            set
            {
                _comPort = value;
                OnPropertyChanged();
            }
        }
        private string _lastError = "";
        public string LastError
        {
            get => _lastError;
            set
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }

        private int getScaled(double value, double min, double max)
        {
            double minScaled = (min / max * 100);
            double valueScaled = value / max * (100 - minScaled);

            //non linear transformation (make lower power have more inpact on motor speed)
            //valueScaled = Math.Sqrt(valueScaled / (100 - minScaled)) * (100 - minScaled);

            valueScaled += minScaled;
            if(valueScaled > max)
            {
                valueScaled = max;
            }

            return (int)Math.Round(valueScaled);
        }

        private int _motorPower;
        [JsonIgnore]
        public int MotorPower
        {
            get => _motorPower;
            set
            {
                _motorPower = getScaled(value, _motorPowerMin, _motorPowerMax);
                if(_motorPower > 100)
                {
                    _motorPower = 100;
                }
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public List<double> MotorPowerHist { get; set; } = new List<double>();

        [JsonIgnore]
        public double MotorPowerSmooth
        {
            get
            {
                try
                {
                    return MotorPowerHist.Average();
                }
                catch
                {
                    return 0;
                }
            }
        }

        private int _lightPower;
        [JsonIgnore]
        public int LightPower
        {
            get => _lightPower;
            set
            {
                _lightPower = getScaled(value, _lightPowerMin, _lightPowerMax);
                if(_lightPower > 100)
                {
                    _lightPower = 100;
                }
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        public List<double> LightPowerHist { get; set; } = new List<double>();
        [JsonIgnore]
        public double LightPowerSmooth
        {
            get
            {
                try
                {
                    return LightPowerHist.Average();
                }
                catch
                {
                    return 0;
                }
            }
        }


        public int RunningAvgCountMS { get; set; } = 5000;

        private int _bestPower;
        public int BestPower
        {
            get => _bestPower;
            set
            {
                _bestPower = value;
                OnPropertyChanged();
            }
        }

        private int _prevDistance = 0;
        public int PrevDistance
        {
            get => _prevDistance;
            set => _prevDistance = value;
        }

        private int _distance = 0;
        [JsonIgnore]
        public int Distance
        {
            get => _distance;
            set
            {
                _distance = value;
                OnPropertyChanged();
                DistanceTotal = 0;
            }
        }

        [JsonIgnore]
        public double DistanceTotal
        {
            get
            {
                return Math.Round(((double)(_distance + _prevDistance)) / 1000, 2);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        private static JsonSerializer _serializer
        {
            get
            {
                var _serializer = new JsonSerializer()
                {
                    //Converters.Add(new JavaScriptDateTimeConverter()),
                    NullValueHandling = NullValueHandling.Include,
                    //this is needed to include types, because of List<Interface> and inherited instances
                    TypeNameHandling = TypeNameHandling.Auto,
                };
                return _serializer;
            }
        }
        static object _lockobj = new object();

        static string _filePath = ".\\config.json";

        public static void Serialize()
        {
            if (_instance == null || _configIsLoading == true) { return; }
            lock (_lockobj)
            {
                try
                {
                    if (File.Exists(Configurations._filePath) == false)
                    {
                        File.Create(Configurations._filePath);
                    }
                    //otherwise it would add it
                    File.WriteAllText(Configurations._filePath, string.Empty);
                    using (var streamWriter = new StreamWriter(Configurations._filePath, true))
                    {
                        string _fileString;
                        using (var _stringWriter = new StringWriter())
                        {
                            _serializer.Serialize(_stringWriter, Configurations.Instance);
                            _fileString = _stringWriter.ToString();
                        }
                        //formatting
                        _fileString = JToken.Parse(_fileString).ToString(Formatting.Indented);
                        streamWriter.WriteLine(_fileString);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        public static Exception Deserialize(bool overrideExisting)
        {
            lock (_lockobj)
            {
                //if (_configIsLoading == true) { return; }
                Instance = null;
                _configIsLoading = true;
                try
                {
                    using (var _streamReader = new StreamReader(Configurations._filePath, true))
                    {
                        Instance = (Configurations)_serializer.Deserialize(_streamReader, typeof(Configurations));
                    }
                    if (_instance == null)
                    {
                        throw new Exception("deserialize returned null");
                    }
                    Configurations.Instance.LightPower = 0;
                    Configurations.Instance.MotorPower = 0;

                    _configIsLoading = false;
                    return null;
                }
                catch (Exception ex)
                {
                    if (overrideExisting)
                    {
                        Configurations.CreateNew();
                        Serialize();
                    }
                    Configurations.Instance.LightPower = 0;
                    Configurations.Instance.MotorPower = 0;

                    _configIsLoading = false;
                    return ex;
                }
            }
        }
        public static string GetAbsolutePath(string anyPath)
        {
            string _absolutePath;
            //ABSOLUTE
            if (anyPath.Length > 0 && anyPath[0] == Path.DirectorySeparatorChar || anyPath.Length > 1 && anyPath[1] == Path.VolumeSeparatorChar)
            {
                _absolutePath = anyPath;
            }
            else
            {
                //RELATIVE
                _absolutePath = Path.GetDirectoryName(Configurations._filePath) + Path.DirectorySeparatorChar + anyPath;
            }
            return _absolutePath;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (!Configurations._configIsLoading)
            {
                Configurations.Serialize();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
