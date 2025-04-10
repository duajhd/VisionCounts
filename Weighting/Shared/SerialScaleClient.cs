using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
namespace Weighting.Shared
{
    public class WeightData
    {
        public float Value { get; set; }
        public string Unit { get; set; } = "";
        public bool IsStable { get; set; }
    }

    public class SerialScaleClient : IDisposable
    {
        private SerialPort _serialPort;

        public event Action<string>? DataReceived; // 原始数据通知
        public event Action<WeightData>? WeightReceived; // 解析后数据通知

        public bool IsOpen => _serialPort?.IsOpen ?? false;

        public SerialScaleClient(string portName, int baudRate = 9600)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            _serialPort.Encoding = Encoding.ASCII;
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        public void Open()
        {
            if (!_serialPort.IsOpen)
                _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort.IsOpen)
                _serialPort.Close();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadExisting();
            DataReceived?.Invoke(data);

            // 解析数据
            if (TryParseWeight(data, out var weightData))
            {
                WeightReceived?.Invoke(weightData);
            }
        }

        public void SendCommand(string command)
        {
            if (_serialPort.IsOpen)
                _serialPort.WriteLine(command);
        }

        public void Dispose()
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Dispose();
            }
        }

        private bool TryParseWeight(string raw, out WeightData weight)
        {
            // 示例数据格式: ST,GS,+000.876kg\r\n
            weight = new WeightData();
            try
            {
                if (raw.Contains("ST,GS"))
                {
                    var parts = raw.Split(',');
                    if (parts.Length >= 3)
                    {
                        string numUnit = parts[2].Trim();
                        if (numUnit.EndsWith("kg"))
                        {
                            var numStr = numUnit.Replace("kg", "").Trim();
                            weight.Unit = "kg";
                            weight.Value = float.Parse(numStr);
                            weight.IsStable = true;
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }
    }

}
