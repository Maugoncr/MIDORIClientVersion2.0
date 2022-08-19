using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace MidoriValveTest
{
    public partial class Chart_Analyzer_File : Form
    {
        public string archivo;
        
        public string ini_range;
        public string end_range;
        public List<string> times = new List<string>();
        public List<string> apertures = new List<string>();
        public List<string> pressures = new List<string>();
        public List<string> datetimes = new List<string>();
        public List<string> alldata = new List<string>();

        public Chart_Analyzer_File()
        {
            InitializeComponent();
        }

        private void Chart_Analyzer_File_Load(object sender, EventArgs e)
        {
            lbl_time.Text = "Analysis captured at: " + end_range + "| Time range[" + ini_range + " - " + end_range + "]";
            lbl_archive.Text = archivo;

            try
            {
                for (int i = 0; i < times.Count; i++)
                {
                    chart1.Series[1].Points.AddXY(times[i],apertures[i]);
                    chart1.Series[0].Points.AddXY(times[i], pressures[i]);
                   // MessageBox.Show(i.ToString() + "   " + times[i] + "    " + apertures[i] + "   " + pressures[i]);
                }
            }
            catch (Exception )
            {

            }

            ChartArea CA = chart1.ChartAreas[0];  // quick reference
            CA.AxisX.ScaleView.Zoomable = true;
            CA.CursorX.AutoScroll = true;
            CA.CursorX.IsUserSelectionEnabled = true;
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true);

            //chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, true);


           
            int pos = Convert.ToInt32(chart1.ChartAreas[0].CursorX.Position);
     


            try
            {

               
                toolTip1.SetToolTip(chart1, "Time:      " + times[pos-1] + "s | (" + datetimes[pos-1] + ")" +
                                         "\nPosition: " + apertures[pos-1 ] +
                                         "°\nPressure: " + pressures[pos -1] + "psi"); ;




            }
            catch (Exception ex)
            {
                toolTip1.SetToolTip(chart1, "Time: -" + "\nPosition: -" + "\nPressure: -");

            }
        }
        ToolTip tooltip = new ToolTip();
        private void chart1_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            HitTestResult hitTestResult = chart1.HitTest(e.X, e.Y);

            if (hitTestResult.PointIndex >= 0)
                if (hitTestResult.ChartElementType == ChartElementType.DataPoint)
                {
                    tooltip.RemoveAll();

                    var results = chart1.HitTest(e.X, e.Y, false,
                                                       ChartElementType.DataPoint);
                    foreach (var result in results)
                    {
                        if (result.ChartElementType == ChartElementType.DataPoint)
                        {
                            var prop = result.Object as DataPoint;
                            if (prop != null)
                            {
                                var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                                var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                                // check if the cursor is really close to the point (2 pixels around the point)
                                if (Math.Abs(e.X - pointXPixel) < 2 &&
                                    Math.Abs(e.Y - pointYPixel) < 2)
                                {
                                    tooltip.Show(prop.XValue +
                                        "," + prop.YValues[0], chart1,
                                        e.X, e.Y - 15);
                                }
                            }
                        }
                    }
                }
        }
    }
}
