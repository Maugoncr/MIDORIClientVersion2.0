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

        private void ObtenerPendienteMaxList()
        {
            List<double> tiempoX = times.ConvertAll(double.Parse);
            List<double> presionY = pressures.ConvertAll(double.Parse);
            List<double> Pendientes = new List<double>();
            // Ambas tienen el mismo lenght

            for (int i = 0; i < presionY.Count; i++)
            {
                if (i < presionY.Count-1)
                {
                    double m = ObtenerMpendiente(tiempoX[i], tiempoX[i + 1],
                                            presionY[i], presionY[i + 1]);
                    Pendientes.Add(m);
                }
            }

            txtM.Text = ObtenerMaxPendiente(Pendientes,1).ToString();
            txtMSinRound.Text = ObtenerMaxPendiente(Pendientes).ToString();

        }

        public static List<string> RegistroPendientes = new List<string>();

        private double ObtenerMpendiente(double x1, double x2, double y1, double y2)
        {
            double m = 0;

            m = (y2 - y1) / (x2 - x1);

            string Registro = m + "," + x1 + "," + x2 + "," + y1 + "," + y2;

            RegistroPendientes.Add(Registro);

            return m;
        }

        private double ObtenerMaxPendiente(List<double> M, int opcion = 0)
        {
            double mayorPendiente = 0;
            for (int i = 0; i < M.Count; i++)
            {
                if (mayorPendiente < M[i])
                {
                    mayorPendiente = M[i];
                }
            }
            //Conversion
            if (opcion == 1)
            {
                decimal R = Convert.ToDecimal(mayorPendiente);
                R = decimal.Round(R, 2);
                mayorPendiente = Convert.ToDouble(R);
            }
            

            return mayorPendiente;
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

        private void btnCalcular_Click(object sender, EventArgs e)
        {
            ObtenerPendienteMaxList();
        }
    }
}
