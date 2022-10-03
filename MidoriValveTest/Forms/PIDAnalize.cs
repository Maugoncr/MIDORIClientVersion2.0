using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MidoriValveTest.Forms
{
    public partial class PIDAnalize : Form
    {
        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        public string archivo;

        public string ini_range;
        public string end_range;
        public List<string> times = new List<string>();
        public List<string> apertures = new List<string>();
        public List<string> pressures = new List<string>();
        public List<string> datetimes = new List<string>();
        public List<string> alldata = new List<string>();

        public PIDAnalize()
        {
            InitializeComponent();
        }

        private void PIDAnalize_Load(object sender, EventArgs e)
        {
            lbTime.Text = "Analysis captured at: " + end_range;
            //lbl_archive.Text = archivo;

            try
            {
                for (int i = 0; i < times.Count; i++)
                {
                    chart1.Series[1].Points.AddXY(times[i], apertures[i]);
                    chart1.Series[0].Points.AddXY(times[i], pressures[i]);
                    // MessageBox.Show(i.ToString() + "   " + times[i] + "    " + apertures[i] + "   " + pressures[i]);
                }
            }
            catch (Exception)
            {

            }

            ChartArea CA = chart1.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panelTop_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelTop_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
