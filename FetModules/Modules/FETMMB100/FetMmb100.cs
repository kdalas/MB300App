using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FetComDriver.FetModules;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Threading;
using static Usb2.Usb2;
using Usb2;
using FetComDriver.Modules;
using _2po_app;
using System.Windows.Forms.DataVisualization.Charting;

namespace FetComDriver
{
    public partial class FetMmb100 : UserControl
    {
        public event EventHandler SettingsChanged;

        public class SettingsChangedEventArgs : EventArgs
        {
            public UInt16 Vmag { set; get; }
            public UInt16 Vdbs { set; get; }
            public UInt16 Offset { set; get; }
        }

        protected virtual void OnSettingsChanged(EventArgs e)
        {
            EventHandler handler = SettingsChanged;
            handler?.Invoke(this, e);
        }

        public enum eRegAddresses : ushort
        {
            AllRegs = 200,
            Enable = 201,
            ResetTick = 202,
            ImageParams = 203,
            SoftSignals = 204,
        }

        public class cRegs
        {
            private const string saveFilePathName = "FET-MMB_Registers.xml";
            private const int byteArrLength = 34;

            public UInt16 Vmag { set; get; }
            public UInt16 Vdbs { set; get; }
            public UInt16 Offset { set; get; }
            public UInt16 XmemsFreq { set; get; }
            public UInt16 YmemsFreq { set; get; }
            public UInt16 Xres { set; get; }
            public UInt16 Yres { set; get; }
            public UInt16 Xphase { set; get; }
            public UInt16 Yphase { set; get; }
            public UInt32 StartSweepFreq { set; get; }
            public UInt32 StopSweepFreq { set; get; }
            public UInt16 SweepTime { set; get; }
            public UInt32 TickFreq { set; get; }
            public UInt16 FramePixels { set; get; }
            public UInt16 FrameXres { set; get; }
            public UInt16 FrameYres { set; get; }

            public bool resampling { set; get; } = false;
            public float resampAmpX { set; get; } = 1;
            public float resampAmpY { set; get; } = 1;
            public float resampFreqX { set; get; } = 600;
            public float resampFreqY { set; get; } = 600 * 32;
            public float resampPhaseX { set; get; } = 0;
            public float resampPhaseY { set; get; } = 0;
            public float resampVariance { set; get; } = 0.001f;
            public int resampNPoints { set; get; } = 10;

            public cRegs()
            {
                Vmag = 100;
                Vdbs = 100;
                Offset = 128;
            }

            public cRegs(byte[] byteArr)
            {
                Vmag = 100;
                Vdbs = 100;
                Offset = 128;
                this.FromBytes(byteArr);
            }

            public byte[] ImageParamsToBytes()
            {
                byte[] byteArr = new byte[byteArrLength];

                int cnt = 0;
                byteArr[cnt++] = (byte)(Vmag & 0xFF);
                byteArr[cnt++] = (byte)((Vmag >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Vdbs & 0xFF);
                byteArr[cnt++] = (byte)((Vdbs >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Offset & 0xFF);
                byteArr[cnt++] = (byte)((Offset >> 8) & 0xFF);

                return byteArr;
            }

            public bool ImageParamsFromBytes(byte[] byteArr)
            {
                if (byteArr.Length < byteArrLength)
                    return false;

                int cnt = 0;
                Vmag = (UInt16)byteArr[cnt++];
                Vmag += (UInt16)(byteArr[cnt++] << 8);
                Vdbs = (UInt16)byteArr[cnt++];
                Vdbs += (UInt16)(byteArr[cnt++] << 8);
                Offset = (UInt16)byteArr[cnt++];
                Offset += (UInt16)(byteArr[cnt++] << 8);
                
                return true;
            }
            public byte[] ToBytes()
            {
                byte[] byteArr = new byte[byteArrLength];

                int cnt = 0;
                byteArr[cnt++] = (byte)(Vmag & 0xFF);
                byteArr[cnt++] = (byte)((Vmag>>8) & 0xFF);
                byteArr[cnt++] = (byte)(Vdbs & 0xFF);
                byteArr[cnt++] = (byte)((Vdbs >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Offset & 0xFF);
                byteArr[cnt++] = (byte)((Offset >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(XmemsFreq & 0xFF);
                byteArr[cnt++] = (byte)((XmemsFreq >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(YmemsFreq & 0xFF);
                byteArr[cnt++] = (byte)((YmemsFreq >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Xres & 0xFF);
                byteArr[cnt++] = (byte)((Xres >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Yres & 0xFF);
                byteArr[cnt++] = (byte)((Yres >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Xphase & 0xFF);
                byteArr[cnt++] = (byte)((Xphase >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(Yphase & 0xFF);
                byteArr[cnt++] = (byte)((Yphase >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(StartSweepFreq & 0xFF);
                byteArr[cnt++] = (byte)((StartSweepFreq >> 8) & 0xFF);
                byteArr[cnt++] = (byte)((StartSweepFreq >> 16) & 0xFF);
                byteArr[cnt++] = (byte)((StartSweepFreq >> 24) & 0xFF);
                byteArr[cnt++] = (byte)(StopSweepFreq & 0xFF);
                byteArr[cnt++] = (byte)((StopSweepFreq >> 8) & 0xFF);
                byteArr[cnt++] = (byte)((StopSweepFreq >> 16) & 0xFF);
                byteArr[cnt++] = (byte)((StopSweepFreq >> 24) & 0xFF);
                byteArr[cnt++] = (byte)(SweepTime & 0xFF);
                byteArr[cnt++] = (byte)((SweepTime >> 8) & 0xFF);
                byteArr[cnt++] = (byte)(TickFreq & 0xFF);
                byteArr[cnt++] = (byte)((TickFreq >> 8) & 0xFF);
                byteArr[cnt++] = (byte)((TickFreq >> 16) & 0xFF);
                byteArr[cnt++] = (byte)((TickFreq >> 24) & 0xFF);
                byteArr[cnt++] = (byte)(FramePixels & 0xFF);
                byteArr[cnt++] = (byte)((FramePixels >> 8) & 0xFF);
                return byteArr;
            }

            public bool FromBytes(byte[] byteArr)
            {
                if (byteArr.Length < byteArrLength)
                    return false;

                int cnt = 0;
                Vmag = (UInt16)byteArr[cnt++];
                Vmag += (UInt16)(byteArr[cnt++] << 8);
                Vdbs = (UInt16)byteArr[cnt++];
                Vdbs += (UInt16)(byteArr[cnt++] << 8);
                Offset = (UInt16)byteArr[cnt++];
                Offset += (UInt16)(byteArr[cnt++] << 8);
                XmemsFreq = (UInt16)byteArr[cnt++];
                XmemsFreq += (UInt16)(byteArr[cnt++] << 8);
                YmemsFreq = (UInt16)byteArr[cnt++];
                YmemsFreq += (UInt16)(byteArr[cnt++] << 8);
                Xres = (UInt16)byteArr[cnt++];
                Xres += (UInt16)(byteArr[cnt++] << 8);
                Yres = (UInt16)byteArr[cnt++];
                Yres += (UInt16)(byteArr[cnt++] << 8);
                Xphase = (UInt16)byteArr[cnt++];
                Xphase += (UInt16)(byteArr[cnt++] << 8);
                Yphase = (UInt16)byteArr[cnt++];
                Yphase += (UInt16)(byteArr[cnt++] << 8);
                StartSweepFreq = (UInt32)byteArr[cnt++];
                StartSweepFreq += (UInt32)(byteArr[cnt++] << 8);
                StartSweepFreq += (UInt32)(byteArr[cnt++] << 16);
                StartSweepFreq += (UInt32)(byteArr[cnt++] << 24);
                StopSweepFreq = (UInt32)byteArr[cnt++];
                StopSweepFreq += (UInt32)(byteArr[cnt++] << 8);
                StopSweepFreq += (UInt32)(byteArr[cnt++] << 16);
                StopSweepFreq += (UInt32)(byteArr[cnt++] << 24);
                SweepTime = (UInt16)byteArr[cnt++];
                SweepTime += (UInt16)(byteArr[cnt++] << 8);
                TickFreq = (UInt32)byteArr[cnt++];
                TickFreq += (UInt32)(byteArr[cnt++] << 8);
                TickFreq += (UInt32)(byteArr[cnt++] << 16);
                TickFreq += (UInt32)(byteArr[cnt++] << 24);
                FramePixels = (UInt16)byteArr[cnt++];
                FramePixels += (UInt16)(byteArr[cnt++] << 8);
                return true;
            }

            static public cRegs ReadFromFile()
            {
                cRegs regs = null;

                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(cRegs));

                    using (FileStream fs = new FileStream(saveFilePathName, FileMode.Open))
                    {
                        regs = (cRegs)ser.Deserialize(fs);
                    }
                }
                catch (Exception e) { }

                return regs;
            }

            static public cRegs ReadFromFile(string file)
            {
                cRegs regs = null;

                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(cRegs));

                    using (FileStream fs = new FileStream(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + file, FileMode.Open))
                    {
                        regs = (cRegs)ser.Deserialize(fs);
                    }
                }
                catch (Exception e) { }

                return regs;
            }
            static public bool SaveToFile(cRegs allRegsToFile)
            {
                XmlSerializer ser = new XmlSerializer(typeof(cRegs));

                using (FileStream fs = new FileStream(saveFilePathName, FileMode.Create))
                {
                    ser.Serialize(fs, allRegsToFile);
                }

                return true;
            }
            static public bool SaveToFile(cRegs allRegsToFile, string file)
            {
                XmlSerializer ser = new XmlSerializer(typeof(cRegs));

                using (FileStream fs = new FileStream(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + file, FileMode.Create))
                {
                    ser.Serialize(fs, allRegsToFile);
                }

                return true;
            }
        }

        private cRegs regsFromFile;
        public CommonModRegsExe ComModRegsExe { get; set; }
        public bool settingsFromFileOK { get; set; }
        public string getCurrentPath { get { return Directory.GetCurrentDirectory(); } }
        public string getExecutingPath { get { return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); } }
        public Usb3 usb3;
        public string moduleName;
        int channel = 0;
        private bool FetEnableStatus = false;
        ScottPlot.WinForms.FormsPlot histogramPlot;

        public FetMmb100(string moduleName, int channel, CommonModRegsExe comModRegsExe, Usb3 usb3)
        {
            InitializeComponent();
            this.moduleName = moduleName;
            this.channel = channel;
            this.usb3 = usb3;

            ComModRegsExe = comModRegsExe;
            comModRegsExe.SetModuleTypeAddr((byte)ModuleBase.eModTypes.FETMMB100);
            ComModRegsExe.GetModuleInfo(eComPriority.NORMAL);

            // załadowanie plików z ustawieniami do comboboxa
            GetSettingsFiles(moduleName);

            regsFromFile = cRegs.ReadFromFile(moduleName + "_Registers.xml");
            if (regsFromFile != null)
            {
                SetAllRegs(regsFromFile);
                SetControlsFromDevice(regsFromFile);
                settingsFromFileOK = true;
            }
            else { settingsFromFileOK = false; };

            GetAllRegsFromDevice();

            histogramPlot = new ScottPlot.WinForms.FormsPlot() { Dock = DockStyle.Fill };
            histogramPlot.Plot.Axes.SetLimitsX(0, 255);
            tableLayoutPanel4.Controls.Add(histogramPlot, 0, 0);

            Thread oStatusThread = new Thread(new ThreadStart(StatusThread));
            oStatusThread.Name = "ModuleStatusThread: " + comModRegsExe.serNum + "," + comModRegsExe.modAddr.ToString();
            oStatusThread.IsBackground = true;
            oStatusThread.Start();
        }

        private void GetSettingsFiles(string moduleName)
        {
            string[] settingsFiles = Directory.GetFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\", moduleName + "_Registers*" + ".xml");

            foreach (var item in settingsFiles)
            {
                comboBoxSettings.Items.Add(Path.GetFileName(item));
            }
        }

        private void SetSettingsFromComboBox()
        {
            string fileName = (string)comboBoxSettings.SelectedItem;
            //string pathFileName = Directory.GetFiles(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)) + "\\" + fileName;

            regsFromFile = cRegs.ReadFromFile(fileName);
            if (regsFromFile != null)
            {
                SetAllRegs(regsFromFile);
                settingsFromFileOK = true;
            }
            else { settingsFromFileOK = false; };
        }

        public void SetUsb3(Usb3 usb3)
        {
            this.usb3 = usb3;
            SetAllRegs(regsFromFile);
        }


        public void GetAdcSettigns(ref UInt16 Vmag, ref UInt16 Vdbs, ref UInt16 Offset, ref UInt32 SamplingFreq)
        {
            Vmag = allRegsFromDev.Vmag;
            Vdbs = allRegsFromDev.Vdbs;
            Offset = allRegsFromDev.Offset;
            SamplingFreq = allRegsFromDev.StopSweepFreq / allRegsFromDev.Xres * allRegsFromDev.FramePixels;
        }

        public ErrorStatus ModuleComExe(UsbModuleCommand com)
        {
            ErrorStatus status = ErrorStatus.UNKNOWN_ERROR;

            switch ((eRegAddresses)com.regAddr)
            {
                // REJESTRY MODUŁU
                case eRegAddresses.AllRegs:
                    {
                        if (com.data.GetType() == typeof(byte[]))
                        {
                            if (((byte[])com.data).Length != 0)
                                SetRegsFromDevice((byte[])com.data);
                            status = ErrorStatus.OK;
                        }
                        break;
                    }
                case eRegAddresses.Enable:
                    {
                        if (com.data.GetType() == typeof(bool))
                        {
                            //SetRegsFromDevice((byte[])com.data);
                            status = ErrorStatus.OK;
                            FetEnableStatus = (bool)com.data;
                        }
                        break;
                    }
                case eRegAddresses.ResetTick:
                    {
                        if (com.data.GetType() == typeof(byte))
                        {
                            //SetRegsFromDevice((byte[])com.data);
                            status = ErrorStatus.OK;
                        }
                        break;
                    }
                case eRegAddresses.ImageParams:
                    {
                        if (com.data.GetType() == typeof(byte[]))
                        {
                            if (((byte[])com.data).Length != 0)
                                SetImageParamsFromDevice((byte[])com.data);
                            status = ErrorStatus.OK;
                        }
                        break;
                    }
                case eRegAddresses.SoftSignals:
                    {
                        if (com.data.GetType() == typeof(byte[]))
                        {
                            if (((byte[])com.data).Length != 0)
                            {
                                // todo!!!
                            }
                            status = ErrorStatus.OK;
                        }
                        break;
                    }

                default:
                    status = ErrorStatus.REGADDR_ERROR;
                    break;
            }

            return status;
        }

        public string GetStatus()
        {
            return FetEnableStatus ? "Start" : "Stop";
        }

        double SLOAvarage = 0;
        double SLOMaxAvarage = 0;
        double SLOHistogramRangeMin = 0;
        double SLOHistogramRangeMax = 255;

        private void CountSLOImageParameteres(System.Drawing.Image image)
        {
            double PixelSum = 0;
            int[] Histogram = new int[256];

            int ROIWidth = image.Width;
            int ROIHeight = image.Height;

            //byte[] imgArray = new byte[ROIWidth * ROIHeight];
            byte[] imgArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                stream.Close();

                imgArray = stream.ToArray();
            }

            // obliczenie histogramu z RIO
            for (int x = 0; x < ROIWidth; x++)
            {
                for (int y = 0; y < ROIHeight; y++)
                {
                    byte Pix = imgArray[54 + (x * ROIHeight + y) * 4]; // 54 = bmp header size, ()*4 for ARGB offset between pixels
                    PixelSum += Pix;

                    Histogram[Pix]++;
                }
            }

            // obliczenie paramtrów obrazu
            SLOAvarage = PixelSum / (ROIWidth * ROIHeight);
            if (SLOMaxAvarage < SLOAvarage)
                SLOMaxAvarage = SLOAvarage;

            double HistogramRangeThreshold = 10;

            SLOHistogramRangeMin = 0;
            for (int i = 0; i < Histogram.Count(); i++)
            {
                if (Histogram[i] > HistogramRangeThreshold)
                {
                    SLOHistogramRangeMin = i;
                    break;
                }
            }

            SLOHistogramRangeMax = Histogram.Count();
            for (int i = Histogram.Count() - 1; i >= 0; i--)
            {
                if ((Histogram[i] > HistogramRangeThreshold))
                {
                    SLOHistogramRangeMax = i;
                    break;
                }
            }

            // jeśli histogram cały jest poniżej progu to ustal szerokość histogramu na max
            if (SLOHistogramRangeMin > SLOHistogramRangeMax)
            {
                SLOHistogramRangeMin = 0;
                SLOHistogramRangeMax = Histogram.Count();
            }

            ThreadSafeCalls.SetControlText(labelHistAvarage, SLOAvarage.ToString("F1"));
            ThreadSafeCalls.SetControlText(labelHistMaxAvarage, SLOMaxAvarage.ToString("F1"));
            ThreadSafeCalls.SetControlText(labelHistRangeMin, SLOHistogramRangeMin.ToString("F0"));
            ThreadSafeCalls.SetControlText(labelHistRangeMax, SLOHistogramRangeMax.ToString("F0"));
            ThreadSafeCalls.SetControlText(labelHistRange, (SLOHistogramRangeMax - SLOHistogramRangeMin).ToString("F0"));
            ThreadSafeCalls.SetControlText(labelHistCenter, ((SLOHistogramRangeMin + SLOHistogramRangeMax) / 2).ToString("F0"));

            //SetChartSeriesThreadSafe(chartHistogram, Histogram);
            histogramPlot.Plot.Clear();
            histogramPlot.Plot.Add.Signal(Histogram);
            histogramPlot.Plot.Axes.AutoScaleY();
            histogramPlot.Refresh();

        }

        private delegate void SetChartSeriesDelegate(Chart chart, int[] points);
        private void SetChartSeriesThreadSafe(Chart chart, int[] points)
        {
            if (chart.InvokeRequired)
            {
                chart.Invoke(new SetChartSeriesDelegate(SetChartSeriesThreadSafe), new object[] { chart, points });
            }
            else
            {
                chart.Series[0].Points.Clear();
                for (int i = 0; i < points.Length; i++)
                {
                    chart.Series[0].Points.AddXY(i, points[i]);
                }

                chart.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                chart.ChartAreas[0].AxisY.Maximum = Double.NaN;
                //chart.ChartAreas[0].RecalculateAxesScale();
            }
        }

        // FUNKCJE PUBLICZNE 
        #region PUBLIC
        public void DrawHistogram(System.Drawing.Image image)
        {
            if (checkBoxHistogramOnoff.Checked)
                CountSLOImageParameteres(image);
        }
        public void EnableAcq(bool enable)
        {
            object regsToSendObj = enable;
            ComModRegsExe.ModuleSetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.Enable, ref regsToSendObj);
        }
        public void ResetTick()
        {
            object regsToSendObj = (byte)0; // nie ważne co wysłane
            ComModRegsExe.ModuleSetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.ResetTick, ref regsToSendObj);
        }

        public void SetSoftSignal(UInt16 signal)
        {
            byte[] signalBytes = new byte[2];
            signalBytes[0] = (byte)signal;
            signalBytes[1] = (byte) (signal >> 8);
            object regsToSendObj = signalBytes;
            ComModRegsExe.ModuleSetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.SoftSignals, ref regsToSendObj);
        }

        public void StartAcq(int fetBitmapsToViewQSelector)
        {
            usb3.fetBitmapsToViewQSelector = fetBitmapsToViewQSelector;
            usb3.fetTakeBitmapsToView = true;
            EnableAcq(true);
        }
        public void StartAcq()
        {
            //usb3.fetBitmapsToViewQSelector = 100;
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
            usb3.fetTakeBitmapsToView = true;
            EnableAcq(true);
        }
        public void StopAcq()
        {
            usb3.fetTakeBitmapsToView = false;
            EnableAcq(false);
        }

        public SettingsChangedEventArgs GetImgSettings()
        {
            SettingsChangedEventArgs args = new SettingsChangedEventArgs();
            args.Vmag = (UInt16)trackBarVmag.Value;
            args.Vdbs = (UInt16)trackBarVdbs.Value;
            args.Offset = (UInt16)trackBarOffset.Value;

            return args;
        }

        public void SetImgSettings(SettingsChangedEventArgs args)
        {
            ThreadSafeCalls.SetTrackBarValue(trackBarVmag, args.Vmag);
            ThreadSafeCalls.SetTrackBarValue(trackBarVdbs, args.Vdbs);
            ThreadSafeCalls.SetTrackBarValue(trackBarOffset, args.Offset);
            SetAllRegsFromForm();
        }

        public void SetResampling(bool resampling)
        {
            checkBoxResampling.Checked = resampling;
        }

        public void SwitchResampling()
        {
            checkBoxResampling.Checked = !checkBoxResampling.Checked;
        }


        #endregion

        cRegs allRegsFromDev;
        void SetControlsFromDevice(cRegs regsFromDevice)
        {
            ThreadSafeCalls.SetTrackBarValue(trackBarVmag, regsFromDevice.Vmag);
            ThreadSafeCalls.SetTrackBarValue(trackBarVdbs, regsFromDevice.Vdbs);
            ThreadSafeCalls.SetTrackBarValue(trackBarOffset, regsFromDevice.Offset);

            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXres, regsFromDevice.Xres);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYres, regsFromDevice.Yres);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXphase, regsFromDevice.Xphase);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownStartXSweepFreq, (int)regsFromDevice.StartSweepFreq);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXFreq, (int)regsFromDevice.XmemsFreq);
            uint yFreq = (regsFromDevice.StopSweepFreq * regsFromDevice.Xres / regsFromDevice.Yres);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYFreq, (int)regsFromDevice.YmemsFreq);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownSweepTime, regsFromDevice.SweepTime);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownTickFreq, (int)regsFromDevice.TickFreq);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFramePixels, (int)regsFromDevice.FramePixels);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFrameXres, (int)regsFromDevice.FrameXres);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFrameYres, (int)regsFromDevice.FrameYres);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYphase, regsFromDevice.Yphase);
        }
        void SetImageParamsFromDevice(byte[] dataFromDevice)
        {
            allRegsFromDev = new cRegs();

            allRegsFromDev.ImageParamsFromBytes(dataFromDevice);

            ThreadSafeCalls.SetControlText(labelVmagValue, allRegsFromDev.Vmag.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarVmag, allRegsFromDev.Vmag);
            ThreadSafeCalls.SetControlText(labelVdbsValue, allRegsFromDev.Vdbs.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarVdbs, allRegsFromDev.Vdbs);
            ThreadSafeCalls.SetControlText(labelOffsetValue, allRegsFromDev.Offset.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarOffset, allRegsFromDev.Offset);
        }

        void SetRegsFromDevice(byte[] dataFromDevice)
        {
            allRegsFromDev = new cRegs();

            allRegsFromDev.FromBytes(dataFromDevice);

            ThreadSafeCalls.SetControlText(labelVmagValue, allRegsFromDev.Vmag.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarVmag, allRegsFromDev.Vmag);
            ThreadSafeCalls.SetControlText(labelVdbsValue, allRegsFromDev.Vdbs.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarVdbs, allRegsFromDev.Vdbs);
            ThreadSafeCalls.SetControlText(labelOffsetValue, allRegsFromDev.Offset.ToString());
            ThreadSafeCalls.SetTrackBarValue(trackBarOffset, allRegsFromDev.Offset);

            ThreadSafeCalls.SetControlText(labelXres, allRegsFromDev.Xres.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXres, allRegsFromDev.Xres);

            ThreadSafeCalls.SetControlText(labelYres, allRegsFromDev.Yres.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYres, allRegsFromDev.Yres);

            ThreadSafeCalls.SetControlText(labelXphase, allRegsFromDev.Xphase.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXphase, allRegsFromDev.Xphase);

            ThreadSafeCalls.SetControlText(labelYphase, allRegsFromDev.Yphase.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYphase, allRegsFromDev.Yphase);

            ThreadSafeCalls.SetControlText(labelStartXSweepFreq, allRegsFromDev.StartSweepFreq.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownStartXSweepFreq, (int)allRegsFromDev.StartSweepFreq);

            ThreadSafeCalls.SetControlText(labelXFreq, allRegsFromDev.XmemsFreq.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownXFreq, (int)allRegsFromDev.XmemsFreq);

            //uint yFreq = (allRegsFromDev.StopSweepFreq * allRegsFromDev.Xres / allRegsFromDev.Yres);
            ThreadSafeCalls.SetControlText(labelYFreq, allRegsFromDev.YmemsFreq.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownYFreq, (int)allRegsFromDev.YmemsFreq);

            ThreadSafeCalls.SetControlText(labelSweepTime, allRegsFromDev.SweepTime.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownSweepTime, allRegsFromDev.SweepTime);

            ThreadSafeCalls.SetControlText(labelTickFreq, allRegsFromDev.TickFreq.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownTickFreq, (int)allRegsFromDev.TickFreq);

            ThreadSafeCalls.SetControlText(labelFramePixels, allRegsFromDev.FramePixels.ToString());
            //ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFramePixels, (int)allRegsFromDev.FramePixels);

            int frameSize = allRegsFromDev.FramePixels;
            double frameResX = Math.Sqrt((double)frameSize);
            int frameResY = (int) (frameSize / frameResX);
            double frameRate = (int)(allRegsFromDev.XmemsFreq / allRegsFromDev.Xres);
            double adcFreq = frameSize * frameRate / 1000000;

            ThreadSafeCalls.SetControlText(labelFrameSize, frameSize.ToString());
            ThreadSafeCalls.SetControlText(labelFrameRes, "~" + frameResX.ToString("0") + "x" + frameResX.ToString("0")); 
            ThreadSafeCalls.SetControlText(labelFramerate, frameRate.ToString() + "fps");
            ThreadSafeCalls.SetControlText(labelAdcFreq, adcFreq.ToString() + "MHz");

            usb3.FetMMB_SetFramePixels((int)numericUpDownFrameXres.Value, (int)numericUpDownFrameYres.Value);
        }

        void SetAllRegs(cRegs allRegsToSend)
        {
            if (allRegsToSend == null)
                return;
            object regsToSendObj = allRegsToSend.ToBytes();
            ComModRegsExe.ModuleSetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.AllRegs, ref regsToSendObj);

            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownPhaseX, allRegsToSend.resampPhaseX);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownPhaseY, allRegsToSend.resampPhaseY);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownAmpX, allRegsToSend.resampAmpX);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownAmpY, allRegsToSend.resampAmpY);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFreqX, allRegsToSend.resampFreqX);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownFreqY, allRegsToSend.resampFreqY);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownVariance, allRegsToSend.resampVariance);
            ThreadSafeCalls.SetNumericUpDownValue(numericUpDownNPoints, allRegsToSend.resampNPoints);
            ThreadSafeCalls.SetCheckBoxChecked(checkBoxResampling, allRegsToSend.resampling);

            if (usb3 != null)
            {
                usb3.SetFetMmbResolution(allRegsToSend.Xres, allRegsToSend.Yres, allRegsToSend.FramePixels);
                usb3.SetResamplingAmpX(channel, allRegsToSend.resampAmpX);
                usb3.SetResamplingAmpY(channel, allRegsToSend.resampAmpY);
                usb3.SetResamplingFreqX(channel, allRegsToSend.resampFreqX);
                usb3.SetResamplingFreqY(channel, allRegsToSend.resampFreqY);
                usb3.SetResamplingPhaseX(channel, allRegsToSend.resampPhaseX);
                usb3.SetResamplingPhaseY(channel, allRegsToSend.resampPhaseY);
                usb3.SetResamplingVariance(channel, allRegsToSend.resampVariance);
                usb3.SetResamplingNPoints(channel, allRegsToSend.resampNPoints);
                usb3.SetResampling(channel, allRegsToSend.resampling);
            }
        }

        void SetImageParams(cRegs allRegsToSend)
        {
            if (allRegsToSend == null)
                return;
            object regsToSendObj = allRegsToSend.ImageParamsToBytes();
            ComModRegsExe.ModuleSetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.ImageParams, ref regsToSendObj);
        }

        cRegs GetAllRegsFromForm()
        {
            regsFromFile.Vmag = (UInt16)trackBarVmag.Value;
            regsFromFile.Vdbs = (UInt16)trackBarVdbs.Value;
            regsFromFile.Offset = (UInt16)trackBarOffset.Value;
            regsFromFile.XmemsFreq = (UInt16)numericUpDownXFreq.Value;
            regsFromFile.YmemsFreq = (UInt16)numericUpDownYFreq.Value;
            regsFromFile.Xres = (UInt16)numericUpDownXres.Value;
            regsFromFile.Yres = (UInt16)numericUpDownYres.Value;
            regsFromFile.Xphase = (UInt16)numericUpDownXphase.Value;
            regsFromFile.Yphase = (UInt16)numericUpDownYphase.Value;
            regsFromFile.StartSweepFreq = (UInt32)numericUpDownStartXSweepFreq.Value;
            regsFromFile.StopSweepFreq = (UInt32)numericUpDownXFreq.Value;
            regsFromFile.SweepTime = (UInt16)numericUpDownSweepTime.Value;
            regsFromFile.TickFreq = (UInt32)numericUpDownTickFreq.Value;
            regsFromFile.resampFreqX = (float)numericUpDownFreqX.Value;
            regsFromFile.resampFreqY = (float)numericUpDownFreqY.Value;
            regsFromFile.resampAmpX = (float)numericUpDownAmpX.Value;
            regsFromFile.resampAmpY = (float)numericUpDownAmpY.Value;
            regsFromFile.resampPhaseX = (float)numericUpDownPhaseX.Value;
            regsFromFile.resampPhaseY = (float)numericUpDownPhaseY.Value;
            regsFromFile.resampVariance = (float)numericUpDownVariance.Value;
            regsFromFile.resampNPoints = (int)numericUpDownNPoints.Value;
            regsFromFile.resampling = checkBoxResampling.Checked;
            regsFromFile.FramePixels = (ushort)numericUpDownFramePixels.Value;
            regsFromFile.FrameXres = (ushort)numericUpDownFrameXres.Value;
            regsFromFile.FrameYres = (ushort)numericUpDownFrameYres.Value;
            return regsFromFile;
        }

        void SetImageParamsFromForm()
        {
            SetImageParams(GetAllRegsFromForm());
        }

        void SetAllRegsFromForm()
        {
            SetImageParams(GetAllRegsFromForm());
            SetAllRegs(GetAllRegsFromForm());
        }

        void GetAllRegsFromDevice()
        {
            cRegs allRegsToSend = new cRegs();
            object regsToSendObj = allRegsToSend.ToBytes();

            ComModRegsExe.ModuleGetRegister(eComPriority.NORMAL, (ushort)eRegAddresses.AllRegs, ref regsToSendObj);
        }

        private void getStatus()
        {
            ComModRegsExe.GetModuleInfo(eComPriority.NORMAL);
            GetAllRegsFromDevice();
        }

        private void timerSloStatus_Tick(object sender, EventArgs e)
        {
            //getStatus();
            if (usb3 != null)
            {
                RefreshFastIO(usb3.FastIO);
                RefreshSoftSignals(usb3.FastIO);
            }
        }

        private void RefreshFastIO(UInt64 fastIO)
        {
            byte [] fastIO_bytes = BitConverter.GetBytes(fastIO);
            labelFastIO_1.Text = IoToLabel(fastIO_bytes[0]);
            labelFastIO_2.Text = IoToLabel(fastIO_bytes[1]);
            labelTrajSynchro.Text = TrajSynchroToLabel(fastIO_bytes[4]);
        }
        private void RefreshSoftSignals(UInt64 fastIO)
        {
            byte[] fastIO_bytes = BitConverter.GetBytes(fastIO);
            radioButtonSoftTriggerState.Checked = (fastIO_bytes[6] & 1) == 1? true : false;
        }
        private string IoToLabel(byte ioValue)
        {
            string ioLabel = "";
            for (int i = 7; i >= 0; i--)
            {
                if (((ioValue >> i) & 1) == 1)
                {
                    //ioLabel += "H"; 
                    ioLabel += "\u26AB";
                }
                else
                {
                    //ioLabel += "L";
                    ioLabel += "\u26AA";
                }
            }

            return ioLabel;
        }

        private string TrajSynchroToLabel(byte syncValue)
        {
            string ioLabel = "";
            for (int i = 1; i >= 0; i--)
            {
                if (((syncValue >> i) & 1) == 1)
                {
                    //ioLabel += "H"; 
                    ioLabel += "\u26AB";
                }
                else
                {
                    //ioLabel += "L";
                    ioLabel += "\u26AA";
                }
            }

            return ioLabel;
        }
        private void refreshStatistics()
        {
            if (usb3 != null)
            {
                ThreadSafeCalls.SetControlText(labelFramesLost, usb3.GetFetFrameLost(channel).ToString());
                ThreadSafeCalls.SetControlText(labelCurrFrame, usb3.GetFetCurrFrame(channel).ToString());
                ThreadSafeCalls.SetControlText(labelCurrTick, usb3.GetFetCurrTick(channel).ToString());
            }
        }

        private void StatusThread()
        {
            while (true)
            {
                //getStatus();
                refreshStatistics();
                Thread.Sleep(500);
            }
        }


        private void SaveRegsFromFormAsDefault()
        {
            cRegs.SaveToFile(GetAllRegsFromForm(), moduleName + "_Registers.xml");
        }

        private void SaveRegsFromForm(string fileName)
        {
            cRegs.SaveToFile(GetAllRegsFromForm(), moduleName + "_Registers_" + fileName + ".xml");
        }

        private void buttonRegSave_Click(object sender, EventArgs e)
        {
            SaveRegsFromFormAsDefault();
        }

        private void numericUpDownViewCoeff_ValueChanged(object sender, EventArgs e)
        {
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
        }

        private void buttonStartView_Click(object sender, EventArgs e)
        {
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
            usb3.fetTakeBitmapsToView = true;
        }

        private void buttonStopView_Click(object sender, EventArgs e)
        {
            usb3.fetTakeBitmapsToView = false;
        }

        private void SettginsChangedEventRaise()
        {
            SettingsChangedEventArgs args = new SettingsChangedEventArgs();
            args.Vmag = (ushort)trackBarVmag.Value;
            args.Vdbs = (ushort)trackBarVdbs.Value;
            args.Offset = (ushort)trackBarOffset.Value;
            OnSettingsChanged(args);
        }

        private void trackBarVmag_Scroll(object sender, EventArgs e)
        {
            //SetAllRegsFromForm();
            SetImageParamsFromForm();
            SettginsChangedEventRaise();
        }

        private void trackBarVdbs_Scroll(object sender, EventArgs e)
        {
            //SetAllRegsFromForm();
            SetImageParamsFromForm();
            SettginsChangedEventRaise();
        }

        private void trackBarOffset_Scroll(object sender, EventArgs e)
        {
            //SetAllRegsFromForm();
            SetImageParamsFromForm();
            SettginsChangedEventRaise();
        }

        private void buttonStartAcq_Click(object sender, EventArgs e)
        {
            StartAcquisition();
        }

        private void StartAcquisition()
        {
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
            usb3.fetTakeBitmapsToView = true;
            EnableAcq(true);
        }

        private void buttonStopAcq_Click(object sender, EventArgs e)
        {
            StopAcquisition();
        }

        private void StopAcquisition()
        {
            usb3.fetTakeBitmapsToView = false;
            EnableAcq(false);
        }

        private void buttonReadReg_Click(object sender, EventArgs e)
        {
            getStatus();
        }

        private void buttonRegSave_Click_1(object sender, EventArgs e)
        {
            SetAllRegsFromForm();
            SaveRegsFromFormAsDefault();
        }

        private void buttonStartView_Click_1(object sender, EventArgs e)
        {
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
            usb3.fetTakeBitmapsToView = true;
        }

        private void buttonStopView_Click_1(object sender, EventArgs e)
        {
            usb3.fetTakeBitmapsToView = false;
        }

        private void buttonResetStatistics_Click(object sender, EventArgs e)
        {
            usb3.ResetFetStatistics(channel);
        }

        private void numericUpDownAmpX_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingAmpX(channel, (float)numericUpDownAmpX.Value);
            }
        }

        private void numericUpDownAmpY_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingAmpY(channel, (float)numericUpDownAmpY.Value);
            }
        }

        private void numericUpDownFreqX_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingFreqX(channel, (float)numericUpDownFreqX.Value);
            }
        }

        private void numericUpDownFreqY_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingFreqY(channel, (float)numericUpDownFreqY.Value);
            }
        }

        private void numericUpDownPhaseX_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingPhaseX(channel, (float)numericUpDownPhaseX.Value);
            }
        }

        private void numericUpDownPhaseY_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingPhaseY(channel, (float)numericUpDownPhaseY.Value);
            }
        }

        public bool GetResampling()
        { return checkBoxResampling.Checked; }

        public void Resampling(bool enable)
        {
            checkBoxResampling.Checked = enable;
        }
        private void checkBoxResampling_CheckedChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResampling(channel, checkBoxResampling.Checked);
            }
        }

        private void numericUpDownStopSweepFreq_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDownViewCoeff_ValueChanged_1(object sender, EventArgs e)
        {
            usb3.fetBitmapsToViewQSelector = (int)numericUpDownViewCoeff.Value;
        }

        private void buttonCheckSettings_Click(object sender, EventArgs e)
        {
            decimal memsXframeFreq = numericUpDownXFreq.Value / numericUpDownXres.Value;
            decimal memsYframeFreq = numericUpDownYFreq.Value / numericUpDownYres.Value;

            if (memsXframeFreq != memsYframeFreq)
            {
                labelCheckingStatus.Text = "Frame frequencies are not equal";
                return;
            }
            labelFramerate.Text = memsXframeFreq.ToString() + "fps";

            decimal adcFreq = numericUpDownFramePixels.Value * memsXframeFreq;
            labelAdcFreq.Text = (adcFreq/1000000).ToString() + "MHz";

            if (adcFreq > 40000000)
            {
                labelCheckingStatus.Text = "ADC frequency is to high (max. 40MHz)";
                return;
            }

            // ustalenie parametrów sweepa
            decimal sweepDepth = numericUpDownStartXSweepFreq.Value / (numericUpDownXFreq.Value * 2);

            if (sweepDepth < 1)     // nie może startować z częstości mniejszej niż docelowa
            {
                labelCheckingStatus.Text = "'Start X Freq' is lower than the '2 * (X Freq)'";
                return;
            }

            //decimal frameXres_estimated = numericUpDownFramePixels.Value / numericUpDownXFreq.Value * numericUpDownYFreq.Value;
            //frameXres_estimated = (decimal)Math.Sqrt((double)frameXres_estimated);
            //decimal frameYres_estimated = frameXres_estimated / numericUpDownYFreq.Value * numericUpDownXFreq.Value;
            //labelFrameRes.Text = "~" + frameXres_estimated.ToString("0") + "x" + frameYres_estimated.ToString("0"); 

            double estimatedFrameRes = Math.Sqrt((double)numericUpDownFramePixels.Value);
            labelFrameRes.Text = "~" + estimatedFrameRes.ToString("0")+ "x" + estimatedFrameRes.ToString("0");


            labelCheckingStatus.Text = "Parameters OK";

            //decimal adcXFreq = 2 * numericUpDownXFreq.Value * numericUpDownXres.Value * numericUpDownFramePixels.Value; 
            //decimal adcYFreq = 2 * numericUpDownYFreq.Value * numericUpDownYres.Value * numericUpDownFramePixels.Value;
            //decimal adcStartSweepFreq = numericUpDownStartXSweepFreq.Value * numericUpDownXres.Value * numericUpDownFramePixels.Value;

            //if (adcXFreq != adcYFreq)
            //{
            //    labelCheckingStatus.Text = "Err: adcXFreq != adcYFreq\nCheck settings";
            //    return;
            //}
            //if (numericUpDownXFreq.Value >= numericUpDownStartXSweepFreq.Value)
            //{
            //    labelCheckingStatus.Text = "Err: Start X freq. to low\nCheck settings";
            //    return;
            //}
            //if (adcXFreq >= 12400000)
            //{
            //    labelCheckingStatus.Text = "Err: ADC freq. to high \nCheck settings";
            //    return;
            //}
            //if (adcStartSweepFreq >= 12400000)
            //{
            //    labelCheckingStatus.Text = "Err: Start X freq. to high\nCheck settings";
            //    return;
            //}
            //labelCheckingStatus.Text = "OK";// + (adcXFreq / 1000000).ToString() + "MHz";
        }

        private void numericUpDownVariance_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingVariance(channel, (float)numericUpDownVariance.Value);
            }
        }

        private void numericUpDownNPoints_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.SetResamplingNPoints(channel, (int)numericUpDownNPoints.Value);
            }
        }
        public double getMulti()
        {
            return (double)numericUpDownYres.Value;
        }
        private void numericUpDownYres_ValueChanged(object sender, EventArgs e)
        {
            if (usb3 != null)
            {
                usb3.Mult = (double)numericUpDownYres.Value;
            }
        }

        private void buttonResetTick_Click(object sender, EventArgs e)
        {
            ResetTick();
        }

        private void buttonLoadSettings_Click(object sender, EventArgs e)
        {
            StopAcquisition();
            //Thread.Sleep(100);
            SetSettingsFromComboBox();
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            SaveRegsFromForm(textBoxSaveSettingsFileName.Text);
        }

        private void numericUpDownAdcResolution_ValueChanged(object sender, EventArgs e)
        {

        }

        void RefreshFrameResolution()
        {
            decimal framePixels = numericUpDownFrameXres.Value * numericUpDownFrameYres.Value;

            if (framePixels < numericUpDownFramePixels.Maximum)
                numericUpDownFramePixels.Value = framePixels;
            else
                MessageBox.Show("FramePixels too high!");
        }
        private void numericUpDownFrameXres_ValueChanged(object sender, EventArgs e)
        {
            RefreshFrameResolution();
        }

        private void numericUpDownFrameYres_ValueChanged(object sender, EventArgs e)
        {
            RefreshFrameResolution();
        }

        private void numericUpDownYphase_ValueChanged(object sender, EventArgs e)
        {
            SetAllRegsFromForm();
        }

        private void checkBoxSoftSignal_CheckedChanged(object sender, EventArgs e)
        {
            UInt16 signalBytes = checkBoxSoftSignal.Checked ? (UInt16)0x0001: (UInt16)0;
            SetSoftSignal(signalBytes);
        }

        private void checkBoxHistogramOnoff_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}