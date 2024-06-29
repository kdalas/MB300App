using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel.Design;
using static Usb2.Usb2;
using Usb2;
using _2po_app;
using static _2po_app.Usb3;

namespace FetComDriver
{
    public partial class FetMmb100Base : BaseControl
    {
        public FetMmb100 moduleControl;

        public FetMmb100Base(Usb2.Usb2 usb, Usb3 usb3, string moduleName, string serialNum, UInt16 modAddr) : base (usb, serialNum, modAddr)
        {
            InitializeComponent();
            moduleControl = new FetMmb100(moduleName, modAddr, ComModRegsExe, usb3);
            moduleControl.Location = new System.Drawing.Point(4, 8);
            groupBoxModule.Controls.Add(moduleControl);

            // ustalenie wielkości okna
            int groupBoxModulResizeX = 6;
            int groupBoxModulResizeY = 8 + 4;
            groupBoxModule.Size = new Size(moduleControl.Size.Width + groupBoxModulResizeX, moduleControl.Size.Height + groupBoxModulResizeY);

            int sizeY = (groupBoxModule.Size.Height + groupBoxModulResizeY) > groupBoxMain.Size.Height ? groupBoxModule.Size.Height + groupBoxModulResizeY + 2: groupBoxMain.Size.Height;
            groupBoxMain.Size = new Size(groupBoxModule.Location.X + groupBoxModule.Size.Width + groupBoxModulResizeX, sizeY);
            this.Size = new Size(groupBoxMain.Size.Width + 5, groupBoxMain.Size.Height + 5);
            SetModuleName(moduleControl.Name + " - " + moduleName);
        }

        public void SetUsb3(Usb3 usb3)
        {
            moduleControl.SetUsb3(usb3);
        }
        public GenericQueue<FetFrameBitmap> Fet1AcqQueue()
        {
            return moduleControl.usb3.fet1FramesToSaveQ;
        }
        public GenericQueue<FetFrameBitmap> Fet2AcqQueue()
        {
            return moduleControl.usb3.fet2FramesToSaveQ;
        }

        protected override ErrorStatus ModuleComExe(UsbModuleCommand com)
        {
            return moduleControl.ModuleComExe(com);
        }
    }
}
