using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using static Usb2.Usb2;
using _2po_app;

namespace FetComDriver
{
    public partial class FetModules : Form
    {
        // przykładowa klasa modułu
        public class ModuleBase
        {
            public const UInt16 cMaxRegFrameLength = 1024;

            public enum eModTypes : byte
            {
                Broadcast = 0,
                Mainboard = 1,
                MemsDriver = 2,
                LaserDriver = 3,
                MultiDriver = 4,
                OledDriver = 5,
                PhotoSensor = 6,
                VoaDriver = 7,
                LedDriver = 8,
                TempDriver = 9,
                PowerBoardA = 10,
                PowerBoardB = 11,
                SloDriver = 12, // SLO100 - wg nowego nazewnictwa
                OctDriver = 13,
                FetDriver = 14,
                MB200 = 15,
                MEMS200 = 16,
                LED200 = 17,
                HSLO100 = 18,
                LD200 = 19,
                LTR100 = 20,
                DAC100 = 21,
                SLO200 = 22,
                GM100 = 23,
                ADC100 = 24,
                SMD100 = 25,
                MM100 = 26,
                FETMMB100 = 27,
                //...
            }

            protected enum eCommonRegisters : byte
            {
                // rejestry systemowe
                Reset = 0,
                Init = 1,
                Baudrate = 2,
                Fdg = 3,                    // FDG (Frame Data Gap)
                Rag = 4,                    // RAG (Req-Ack Gap)
                SaveSystemSettings = 5,
                StatusLedEnable = 6,

            }

            public class ModuleVersion
            {
                public byte Major;
                public byte Minor;
                public UInt16 Release;

                public ModuleVersion(byte[] dataArray)
                {
                    Major = dataArray[0];
                    Minor = dataArray[1];
                    Release = BitConverter.ToUInt16(dataArray, 2);
                }

                public override string ToString()
                {
                    return Major.ToString() + "." + Minor.ToString() + "." + Release.ToString();
                }
            }

        }

        private enum eModulesSetup
        {
            Main,
            KK_2PO_System,
            MB200,
            MM100_MEMS100,
            Chinrest,
        }
        eModulesSetup setup = eModulesSetup.Main;

        private Usb2.Usb2 _modUsb;
        private Usb2.Usb2 _chinrestUsb;

        //private Usb3 _usb3;
        GenericQueue<UsbModuleCommand> _modQueue = null;

        Mems100Base _mems100Fet1;
        Mems100Base _mems100Fet2;
        Fet100Base _fet1;
        Fet100Base _fet2;
        Slo100Base _slo;
        Oct100Base _oct;
        Mb200Base _mb200;
        Md100Base _md100;
        Mems200Base _mems200Fet1;
        Phd100Base _phd100;
        Hslo100Base _hslo100;
        Led200Base _led200;
        Ld200Base _ld200;
        LTR100Base _ltr100;
        Dac100Base _dac100;
        Gm100Base _gm100;
        Adc100Base _adc100;
        Smd100Base _smd100;
        Mm100Base _mm100;

        Ld100 _ld100_fet;
        Ld100 _ld100_slo;
        Oled100Base _oled100;
        Voa100Base _voa100;
        Pb100a _pb100a;
        Pb100b _pb100b;
        Led100 _led100;
        Led201Base _led201;

        Md100Base SMD100_1_KK;
        Md100Base SMD100_2_KK;
        Md100Base SMD100_3_KK;
        Md100Base SMD100_4_KK;
        Md100Base SMD100_5_KK;
        Md100Base MD100_KK;
        Led200Base LED200_KK;

        Md100Base _md100_ChinrestX;
        Md100Base _md100_ChinrestY;
        Md100Base _md100_ChinrestZ;


        public FetModules()
        {
            InitializeComponent();

            _modUsb = new Usb2.Usb2(_modQueue);

            switch (setup)
            {
                case eModulesSetup.Main:
                    // usunięcie niepotrzebnych zakładek
                    //tabControlModules.TabPages.Remove(tabPageMain);
                    tabControlModules.TabPages.Remove(tabPageOLED100);
                    tabControlModules.TabPages.Remove(tabPagePB100);
                    tabControlModules.TabPages.Remove(tabPageVOA100);
                    tabControlModules.TabPages.Remove(tabPageMD100);
                    tabControlModules.TabPages.Remove(tabPageLED100);
                    tabControlModules.TabPages.Remove(tabPageFet1);
                    tabControlModules.TabPages.Remove(tabPageFet2);
                    tabControlModules.TabPages.Remove(tabPageLD200_883nm);
                    tabControlModules.TabPages.Remove(tabPageLD200_923nm);
                    tabControlModules.TabPages.Remove(tabPageLTR100);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_1);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_2);
                    tabControlModules.TabPages.Remove(tabPageOct);
                    tabControlModules.TabPages.Remove(tabPagePHD100);
                    tabControlModules.TabPages.Remove(tabPageSlo);
                    tabControlModules.TabPages.Remove(tabPageMD100_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_1_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_2_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_3_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_4_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_5_KK);
                    tabControlModules.TabPages.Remove(tabPageLED200_KK);
                    tabControlModules.TabPages.Remove(tabPageGM100);
                    tabControlModules.TabPages.Remove(tabPageAdc100);

                    tabControlModules.SelectTab(tabPageMain);

                    if (_modUsb != null)
                    {
                        _modUsb.ConnectUsb("FET Modules", "000001", 115200, 100, 100);
                        //_modUsb.ConnectUsb("angioSLO", "000001", 115200, 100, 100);
                        //_modUsb.ConnectUsb("FET Modules", "000001", 921600, 100, 100);
                        //_modUsb.ConnectUsb("LD200_LTR100", "000001", 115200, 100, 100);
                    }

                    // dodanie kontrolki DAC100
                    //_dac100 = new Dac100Base(_modUsb, "DAC100", "000001", 1);
                    //tabPageMain.Controls.Add(_dac100);

                    //// dodanie kontrolek MEMS200
                    //_mems200Fet1 = new Mems200Base(_modUsb, "FET1", "000001", 3);
                    //_mems200Fet1.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._mems200Fet1);

                    ////// dodanie kontrolek MEMS100
                    //_mems100Fet1 = new Mems100Base(_modUsb, "FET1", "000001", 3);
                    //_mems100Fet1.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._mems100Fet1);

                    //_mems100Fet2 = new Mems100Base(_modUsb, "FET2", "000001", 2);
                    //_mems100Fet2.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._mems100Fet2);

                    //// dodanie kontrolek FET
                    //_fet1 = new Fet100Base(_modUsb, null, "FET1 Acquisition", "000001", 1);
                    //_fet1.Location = new Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._fet1);

                    //_fet2 = new Fet100Base(_modUsb, null, "FET2 Acquisition", "000002", 2);
                    //_fet2.Location = new Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._fet2);

                    //// dodanie kontrolki SLO
                    //_slo = new Slo100Base(_modUsb, _usb3, "SLO", "000001", 1);
                    //_slo.Location = new Point(0, 0);
                    //this.tabPageSlo.Controls.Add(this._slo);

                    //// dodanie kontrolki OCT
                    //_oct = new Oct100Base(_modUsb, "OCT", "000001", 1);
                    //_oct.Location = new Point(0, 0);
                    //this.tabPageOct.Controls.Add(this._oct);


                    //// dodanie kontrolki OLED100
                    //_oled100 = new Oled100Base(_modUsb, "Fixation", "000001", 1);
                    ////_modules[(int)eModulesList.OLED100_Fixation] = _oled100;
                    //tabPageOLED100.Controls.Add(_oled100);

                    //// dodanie kontrolki LD100-FET
                    //_ld100_fet = new Ld100("FET", "000001", 1);
                    //_ld100_fet.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageLD100.Controls.Add(this._ld100_fet);

                    //// dodanie kontrolki LD100-SLO
                    //_ld100_slo = new Ld100("SLO", "000002", 2);
                    //_ld100_slo.Location = new System.Drawing.Point(595, 0);
                    //this.tabPageLD100.Controls.Add(this._ld100_slo);

                    //// dodanie kontrolki OLED100
                    //_oled100 = new Oled100("", "000001", 1);
                    //_oled100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageOLED100.Controls.Add(this._oled100);

                    //// dodanie kontrolki VOA100
                    //_voa100 = new Voa100Base(_modUsb, "VOA100", "000001", 1);
                    //_voa100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageVOA100.Controls.Add(this._voa100);

                    // dodanie kontrolki MD100
                    //_md100 = new Md100Base(_modUsb, "MD100", "000001", 5);
                    //_md100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._md100);


                    //// dodanie kontrolki MD100
                    //_hslo100 = new Hslo100Base(_modUsb, null, "HSLO100", "000001", 1);
                    //_hslo100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMB200.Controls.Add(this._hslo100);

                    //// dodanie kontrolki PB100A
                    //_pb100a = new Pb100a("", "000001", 1);
                    //_pb100a.Location = new System.Drawing.Point(0, 0);
                    //this.tabPagePB100.Controls.Add(this._pb100a);

                    //// dodanie kontrolki PB100B
                    //_pb100b = new Pb100b("", "000001", 1);
                    //_pb100b.Location = new System.Drawing.Point(390, 0);
                    //this.tabPagePB100.Controls.Add(this._pb100b);

                    //// dodanie kontrolki PB100B
                    //_led100 = new Led100("", "000001", 1);
                    //_led100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageLED100.Controls.Add(this._led100);

                    //// dodanie kontrolki PHD100
                    //_phd100 = new Phd100Base(_modUsb, "PHD100", "000001", 1);
                    //_phd100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPagePHD100.Controls.Add(this._phd100);

                    //_led200 = new Led200Base(_modUsb, "LED200", "000001", 1);
                    //_led200.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._led200);

                    // LD200
                    //_ld200 = new Ld200Base(_modUsb, "LD200", "000001", 2);
                    //_ld200.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageLD200_883nm.Controls.Add(this._ld200);


                    //// LD200
                    //_ld200 = new Ld200Base(_modUsb, "LD200", "000001", 1);
                    //_ld200.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageLD200_923nm.Controls.Add(this._ld200);


                    ////// LTR100
                    //_ltr100 = new LTR100Base(_modUsb, "LTR100", "000001", 1);
                    //_ltr100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageLTR100.Controls.Add(this._ltr100);

                    ////GM100
                    //_gm100 = new Gm100Base(_modUsb, "GM100", "000001", 1);
                    //_gm100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageGM100.Controls.Add(this._gm100);

                    //ADC100
                    //_adc100 = new Adc100Base(_modUsb, "ADC100", "000001", 1);
                    //_adc100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageAdc100.Controls.Add(this._adc100);

                    //SMD100
                    _smd100 = new Smd100Base(_modUsb, "SMD100", "000001", 12);
                    _smd100.Location = new System.Drawing.Point(0, 0);
                    this.tabPageMain.Controls.Add(this._smd100);

                    //MM100
                    //_mm100 = new Mm100Base(_modUsb, "MM100", "000001", 1);
                    //_mm100.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._mm100);

                    //LED201
                    //_led201 = new Led201Base(_modUsb, "LED200", "000001", 1);
                    //_led201.Location = new System.Drawing.Point(0, 0);
                    //this.tabPageMain.Controls.Add(this._led201);

                    break;
                case eModulesSetup.KK_2PO_System:
                    // usunięcie niepotrzebnych zakładek
                    tabControlModules.TabPages.Remove(tabPageMain);
                    tabControlModules.TabPages.Remove(tabPageOLED100);
                    tabControlModules.TabPages.Remove(tabPagePB100);
                    tabControlModules.TabPages.Remove(tabPageVOA100);
                    tabControlModules.TabPages.Remove(tabPageMD100);
                    tabControlModules.TabPages.Remove(tabPageLED100);
                    tabControlModules.TabPages.Remove(tabPageFet1);
                    tabControlModules.TabPages.Remove(tabPageFet2);
                    tabControlModules.TabPages.Remove(tabPageLD200_883nm);
                    tabControlModules.TabPages.Remove(tabPageLD200_923nm);
                    tabControlModules.TabPages.Remove(tabPageLTR100);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_1);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_2);
                    tabControlModules.TabPages.Remove(tabPageOct);
                    tabControlModules.TabPages.Remove(tabPagePHD100);
                    tabControlModules.TabPages.Remove(tabPageSlo);
                    tabControlModules.TabPages.Remove(tabPageGM100);
                    tabControlModules.TabPages.Remove(tabPageAdc100);

                    tabControlModules.SelectTab(tabPageSMD100_1_KK);

                    this.Text = "2PO Controlling System ";

                    if (_modUsb != null)
                    {
                        _modUsb.ConnectUsb("2PO System", "000001", 115200, 100, 100);
                        //_modUsb.ConnectUsb("FET Modules", "000001", 115200, 100, 100);
                    }

                    SMD100_1_KK = new Md100Base(_modUsb, "SMD100-1 - Filter 1", "000001", 1);
                    SMD100_1_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageSMD100_1_KK.Controls.Add(this.SMD100_1_KK);

                    SMD100_2_KK = new Md100Base(_modUsb, "SMD100-2 - Filter 2", "000001", 2);
                    SMD100_2_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageSMD100_2_KK.Controls.Add(this.SMD100_2_KK);

                    SMD100_3_KK = new Md100Base(_modUsb, "SMD100-Stage 1", "000001", 3);
                    SMD100_3_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageSMD100_3_KK.Controls.Add(this.SMD100_3_KK);

                    SMD100_4_KK = new Md100Base(_modUsb, "SMD100-Stage 2", "000001", 4);
                    SMD100_4_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageSMD100_4_KK.Controls.Add(this.SMD100_4_KK);

                    SMD100_5_KK = new Md100Base(_modUsb, "SMD100-Stage 3", "000001", 5);
                    SMD100_5_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageSMD100_5_KK.Controls.Add(this.SMD100_5_KK);

                    MD100_KK = new Md100Base(_modUsb, "MD100-Shutters", "000001", 6);
                    MD100_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageMD100_KK.Controls.Add(this.MD100_KK);

                    LED200_KK = new Led200Base(_modUsb, "LED200-LEDS", "000001", 1);
                    LED200_KK.Location = new System.Drawing.Point(0, 0);
                    this.tabPageLED200_KK.Controls.Add(this.LED200_KK);
                    break;

                case eModulesSetup.MB200:
                    // usunięcie niepotrzebnych zakładek
                    tabControlModules.TabPages.Remove(tabPageOLED100);
                    tabControlModules.TabPages.Remove(tabPagePB100);
                    tabControlModules.TabPages.Remove(tabPageVOA100);
                    tabControlModules.TabPages.Remove(tabPageMD100);
                    tabControlModules.TabPages.Remove(tabPageLED100);
                    tabControlModules.TabPages.Remove(tabPageFet1);
                    tabControlModules.TabPages.Remove(tabPageFet2);
                    tabControlModules.TabPages.Remove(tabPageLD200_883nm);
                    tabControlModules.TabPages.Remove(tabPageLD200_923nm);
                    tabControlModules.TabPages.Remove(tabPageLTR100);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_1);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_2);
                    tabControlModules.TabPages.Remove(tabPageOct);
                    tabControlModules.TabPages.Remove(tabPagePHD100);
                    tabControlModules.TabPages.Remove(tabPageSlo);
                    tabControlModules.TabPages.Remove(tabPageMD100_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_1_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_2_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_3_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_4_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_5_KK);
                    tabControlModules.TabPages.Remove(tabPageLED200_KK);
                    tabControlModules.TabPages.Remove(tabPageGM100);
                    tabControlModules.TabPages.Remove(tabPageAdc100);

                    tabControlModules.SelectTab(tabPageMain);


                    // wersja dla Kujawińskiej
                    if (_modUsb != null)
                    {
                        if (_modUsb.ConnectUsb("MB200", "000002", 115200, 100, 100) != Usb2.ErrorStatus.OK)
                        {
                            if (_modUsb.ConnectUsb("MB200", "000003", 115200, 100, 100) != Usb2.ErrorStatus.OK)
                                _modUsb.ConnectUsb("MB200", "000001", 115200, 100, 100);
                        }
                    }

                    //if (_modUsb != null)
                    //{
                    //    _modUsb.ConnectUsb("MB200", "000001", 115200, 100, 100);
                    //}


                    // dodanie kontrolki MB200
                    _mb200 = new Mb200Base(_modUsb, "Mainboard", "000001", 1);
                    tabPageMain.Controls.Add(_mb200);

                    break;
                case eModulesSetup.MM100_MEMS100:
                    // usunięcie niepotrzebnych zakładek
                    tabControlModules.TabPages.Remove(tabPageOLED100);
                    tabControlModules.TabPages.Remove(tabPagePB100);
                    tabControlModules.TabPages.Remove(tabPageVOA100);
                    tabControlModules.TabPages.Remove(tabPageMD100);
                    tabControlModules.TabPages.Remove(tabPageLED100);
                    tabControlModules.TabPages.Remove(tabPageFet1);
                    tabControlModules.TabPages.Remove(tabPageFet2);
                    tabControlModules.TabPages.Remove(tabPageLD200_883nm);
                    tabControlModules.TabPages.Remove(tabPageLD200_923nm);
                    tabControlModules.TabPages.Remove(tabPageLTR100);
                    //tabControlModules.TabPages.Remove(tabPageMEMS100_1);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_2);
                    tabControlModules.TabPages.Remove(tabPageOct);
                    tabControlModules.TabPages.Remove(tabPagePHD100);
                    tabControlModules.TabPages.Remove(tabPageSlo);
                    tabControlModules.TabPages.Remove(tabPageMD100_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_1_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_2_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_3_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_4_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_5_KK);
                    tabControlModules.TabPages.Remove(tabPageLED200_KK);
                    tabControlModules.TabPages.Remove(tabPageGM100);
                    tabControlModules.TabPages.Remove(tabPageAdc100);

                    tabControlModules.SelectTab(tabPageMain);

                    // dodanie kontrolek MEMS100
                    _mems100Fet1 = new Mems100Base(_modUsb, "FET1", 1, "000001", 2);
                    _mems100Fet1.Location = new System.Drawing.Point(0, 0);
                    this.tabPageMEMS100_1.Controls.Add(this._mems100Fet1);

                    //MM100
                    _mm100 = new Mm100Base(_modUsb, "MM100", "000001", 2);
                    _mm100.Location = new System.Drawing.Point(0, 0);
                    this.tabPageMain.Controls.Add(this._mm100);

                    if (_modUsb != null)
                    {
                        _modUsb.ConnectUsb("FET Modules", "000001", 115200, 100, 100);
                    }
                    break;

                case eModulesSetup.Chinrest:
                    tabControlModules.TabPages.Remove(tabPageMain);
                    tabControlModules.TabPages.Remove(tabPageOLED100);
                    tabControlModules.TabPages.Remove(tabPagePB100);
                    tabControlModules.TabPages.Remove(tabPageVOA100);
                    tabControlModules.TabPages.Remove(tabPageMD100);
                    tabControlModules.TabPages.Remove(tabPageLED100);
                    tabControlModules.TabPages.Remove(tabPageFet1);
                    tabControlModules.TabPages.Remove(tabPageFet2);
                    tabControlModules.TabPages.Remove(tabPageLD200_883nm);
                    tabControlModules.TabPages.Remove(tabPageLD200_923nm);
                    tabControlModules.TabPages.Remove(tabPageLTR100);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_1);
                    tabControlModules.TabPages.Remove(tabPageMEMS100_2);
                    tabControlModules.TabPages.Remove(tabPageOct);
                    tabControlModules.TabPages.Remove(tabPagePHD100);
                    tabControlModules.TabPages.Remove(tabPageSlo);
                    tabControlModules.TabPages.Remove(tabPageMD100_KK);
                    //tabControlModules.TabPages.Remove(tabPageSMD100_1_KK);
                    //tabControlModules.TabPages.Remove(tabPageSMD100_2_KK);
                    //tabControlModules.TabPages.Remove(tabPageSMD100_3_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_4_KK);
                    tabControlModules.TabPages.Remove(tabPageSMD100_5_KK);
                    tabControlModules.TabPages.Remove(tabPageLED200_KK);
                    tabControlModules.TabPages.Remove(tabPageGM100);
                    tabControlModules.TabPages.Remove(tabPageAdc100);

                    _chinrestUsb = new Usb2.Usb2(_modQueue);
                    _chinrestUsb.ConnectUsb("2POforKK", "000001", 115200, 100, 100);

                    {
                        _md100_ChinrestX = new Md100Base(_chinrestUsb, "Chinrest X axis", "000012", 1);   // adres zafiksowyn
                        //_modules[(int)eModulesList.MD100_ChinrestX] = _md100_ChinrestX;
                        tabPageSMD100_1_KK.Controls.Add(_md100_ChinrestX);
                    }

                    {
                        _md100_ChinrestY = new Md100Base(_chinrestUsb, "Chinrest Y axis", "000014", 2);   // adres zafiksowyn
                        //_modules[(int)eModulesList.MD100_ChinrestY] = _md100_ChinrestY;
                        tabPageSMD100_2_KK.Controls.Add(_md100_ChinrestY);
                    }

                    {
                        _md100_ChinrestZ = new Md100Base(_chinrestUsb, "Chinrest Z axis", "000013", 3);   // adres zafiksowyn
                        //_modules[(int)eModulesList.MD100_ChinrestZ] = _md100_ChinrestZ;
                        tabPageSMD100_3_KK.Controls.Add(_md100_ChinrestZ);
                    }

                    break;
                default:
                    break;
            }

            

            timer1.Enabled = true;
            //_usb3.fetTakeBitmapsToView = true;
            //_usb3.fetBitmapsToViewQSelector = 100;
        }

        private void FetModules_FormClosing(object sender, FormClosingEventArgs e)
        {
            //_usb3.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //   labelTest.Text = _usb3.fet1BitmapsToViewQ.Count.ToString(); ;
        }
    }
}
