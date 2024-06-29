using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FetComDriver
{
    public class ThreadSafeCalls
    {
        private delegate void SetDataSourceDelegate(DataGridView dataGrid, BindingSource source);

        public static void SetDataSource(DataGridView dataGrid, BindingSource source)
        {
            try
            {
                if (dataGrid.InvokeRequired)
                {
                    dataGrid.Invoke(new SetDataSourceDelegate(SetDataSource), new object[] { dataGrid, source });
                }
                else
                {
                    dataGrid.DataSource = source;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void ChangeControlBackColorDelegate(System.Windows.Forms.Control control, Color color);

        public static void ChangeControlBackColor(System.Windows.Forms.Control control, Color color)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new ChangeControlBackColorDelegate(ChangeControlBackColor), new object[] { control, color });
                }
                else
                {
                    control.BackColor = color;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void ChangeLabelForeColorDelegate(System.Windows.Forms.Label label, Color color);

        public static void ChangeLabelForeColor(System.Windows.Forms.Label label, Color color)
        {
            try
            {
                if (label.InvokeRequired)
                {
                    label.Invoke(new ChangeLabelForeColorDelegate(ChangeLabelForeColor), new object[] { label, color });
                }
                else
                {
                    label.ForeColor = color;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void ControlEnableDelegate(System.Windows.Forms.Control control, bool enable);

        public static void ControlEnable(System.Windows.Forms.Control control, bool enable)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new ControlEnableDelegate(ControlEnable), new object[] { control, enable });
                }
                else
                {
                    control.Enabled = enable;
                }
            }
            catch (Exception)
            { }
        }


        private delegate void RefreshDataGridDelegate(System.Windows.Forms.DataGridView grid);

        public static void RefreshDataGridView(System.Windows.Forms.DataGridView grid)
        {
            try
            {
                if (grid.InvokeRequired)
                {
                    grid.Invoke(new RefreshDataGridDelegate(RefreshDataGridView), new object[] { grid });
                }
                else
                {
                    //grid.SelectedObject = item;
                    grid.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void RefreshPropertyGridDelegate(System.Windows.Forms.PropertyGrid grid, object item);

        public static void RefreshPropertyGrid(System.Windows.Forms.PropertyGrid grid, object item)
        {
            try
            {
                if (grid.InvokeRequired)
                {
                    grid.Invoke(new RefreshPropertyGridDelegate(RefreshPropertyGrid), new object[] { grid, item });
                }
                else
                {
                    if (item != null)
                        grid.SelectedObject = item;
                    grid.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetControlTextDelegate(System.Windows.Forms.Control control, string text);

        public static void SetControlText(System.Windows.Forms.Control control, string text)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new SetControlTextDelegate(SetControlText), new object[] { control, text });
                }
                else
                {
                    control.Text = text;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void AddControlTextDelegate(System.Windows.Forms.Control control, string text);

        public static void AddControlText(System.Windows.Forms.Control control, string text)
        {
            try
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(new AddControlTextDelegate(AddControlText), new object[] { control, text });
                }
                else
                {
                    control.Text += text;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void AppendTexttDelegate(System.Windows.Forms.RichTextBox richTextBox, string text);

        public static void AppendText(System.Windows.Forms.RichTextBox richTextBox, string text)
        {
            try
            {
                if (richTextBox.InvokeRequired)
                {
                    richTextBox.Invoke(new AppendTexttDelegate(AppendText), new object[] { richTextBox, text });
                }
                else
                {
                    richTextBox.AppendText(text);
                }
            }
            catch (Exception)
            { }
        }

        private delegate void AddItemToListBoxDelegate(System.Windows.Forms.ListBox listBox, object item);

        public static void AddItemToListBox(System.Windows.Forms.ListBox listBox, object item)
        {
            try
            {
                if (listBox.InvokeRequired)
                {
                    listBox.Invoke(new AddItemToListBoxDelegate(AddItemToListBox), new object[] { listBox, item });
                }
                else
                {
                    listBox.Items.Add(item);
                    listBox.SelectedIndex = listBox.Items.Count - 1;
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetPropertyGridObjectDelegate(System.Windows.Forms.PropertyGrid propertyGrid, object item);

        public static void SetPropertyGridObject(System.Windows.Forms.PropertyGrid propertyGrid, object item)
        {
            try
            {
                if (propertyGrid.InvokeRequired)
                {
                    propertyGrid.BeginInvoke(new SetPropertyGridObjectDelegate(SetPropertyGridObject), new object[] { propertyGrid, item });
                }
                else
                {
                    propertyGrid.SelectedObject = item;
                    propertyGrid.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetPictureBoxImageDelegate(System.Windows.Forms.PictureBox pictureBox, Bitmap bitmap);

        public static void SetPictureBoxImage(System.Windows.Forms.PictureBox pictureBox, Bitmap bitmap)
        {
            try
            {
                if (pictureBox.InvokeRequired)
                {
                    pictureBox.BeginInvoke(new SetPictureBoxImageDelegate(SetPictureBoxImage), new object[] { pictureBox, bitmap });
                }
                else
                {
                    pictureBox.Image = bitmap;
                    pictureBox.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetTrackBarValueDelegate(System.Windows.Forms.TrackBar trackBar, int value);

        public static void SetTrackBarValue(System.Windows.Forms.TrackBar trackBar, int value)
        {
            try
            {
                if (trackBar.InvokeRequired)
                {
                    trackBar.BeginInvoke(new SetTrackBarValueDelegate(SetTrackBarValue), new object[] { trackBar, value });
                }
                else
                {
                    trackBar.Value = value;
                    trackBar.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetNumericUpDownValueDelegate(System.Windows.Forms.NumericUpDown numericUpDown, object value);

        public static void SetNumericUpDownValue(System.Windows.Forms.NumericUpDown numericUpDown, object value)
        {
            try
            {
                if (numericUpDown.InvokeRequired)
                {
                    numericUpDown.BeginInvoke(new SetNumericUpDownValueDelegate(SetNumericUpDownValue), new object[] { numericUpDown, value });
                }
                else
                {
                    numericUpDown.Value = (decimal)Convert.ChangeType(value, TypeCode.Decimal);
                    numericUpDown.Refresh();
                }
            }
            catch (Exception e)
            { }
        }

        private delegate void SetCheckBoxCheckedDelegate(System.Windows.Forms.CheckBox checkbox, bool value);

        public static void SetCheckBoxChecked(System.Windows.Forms.CheckBox checkbox, bool value)
        {
            try
            {
                if (checkbox.InvokeRequired)
                {
                    checkbox.BeginInvoke(new SetCheckBoxCheckedDelegate(SetCheckBoxChecked), new object[] { checkbox, value });
                }
                else
                {
                    checkbox.Checked = value;
                    checkbox.Refresh();
                }
            }
            catch (Exception)
            { }
        }

        private delegate void SetRadioButtonDelegate(System.Windows.Forms.RadioButton radioButton, bool value);

        public static void SetRadioButton(System.Windows.Forms.RadioButton radioButton, bool value)
        {
            try
            {
                if (radioButton.InvokeRequired)
                {
                    radioButton.BeginInvoke(new SetRadioButtonDelegate(SetRadioButton), new object[] { radioButton, value});
                }
                else
                {
                    radioButton.Checked = value;
                    radioButton.Refresh();
                }
            }
            catch (Exception)
            { }
        }
    }
}
