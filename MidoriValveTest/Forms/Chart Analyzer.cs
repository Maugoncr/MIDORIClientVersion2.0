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
    
    public partial class Chart_Analyzer : Form
    {
        private List<string> times = new List<string>();
        private List<string> apertures = new List<string>();
        private List<string> pressures = new List<string>();
        private List<string> datetimes = new List<string>();

        public double final_time;
        public  DateTime date;
        public DateTime date_var;
        DateTime d1 = new DateTime();
        public Chart_Analyzer()
        {
            
            InitializeComponent();
        
           
        }

        private void Chart_Analiser_FormClosing(object sender, FormClosingEventArgs e)
        {
            chart1.Series.Clear();
        }
     
        ToolTip tooltip = new ToolTip();
        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);
            chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true);

           // chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, true);

            
            // label1.Text =( chart1.ChartAreas[0].CursorX.Position).ToString();
            int pos  = Convert.ToInt32(chart1.ChartAreas[0].CursorX.Position);
            //inictial_time = chart1.Series["Aperture value"].Points[1].XValue;
        

            try
            {
             
                string time = (decimal.Round((decimal)(final_time - ((chart1.Series[0].Points.Count- pos)*0.040 )),2)).ToString();
                //string time = (chart1.Series["Aperture value"].Points[pos - 1].XValue).ToString();
                string aperture   = (decimal.Round((decimal)chart1.Series["Aperture value"].Points[pos-1].YValues[0],2)).ToString();
                string pressure   = (chart1.Series["Pressure"].Points[pos-1].YValues[0]).ToString();
                date_var = date;
                string datetimestring = date_var.AddMilliseconds(-(40 * ((chart1.Series[0].Points.Count + 1) - (pos)))).ToString("hh:mm:ss:ff tt");
                toolTip1.SetToolTip(chart1, "Time:      " + time +"s | ("+datetimestring+")"+
                                         "\nPosition: " + aperture +
                                         "°\nPressure: " + pressure + "psi"); ;

               


            }
            catch (Exception)
            {
                toolTip1.SetToolTip(chart1, "Time: -" + "\nPosition: -" + "\nPressure: -")  ;

           }

           
        }

        private void chart1_GetToolTipText(object sender, ToolTipEventArgs e)
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
        int Point;
        private void Chart_Analiser_Load(object sender, EventArgs e)
        {
            Point = chart1.Series["Aperture value"].Points.Count();
            d1 = date;
           // MessageBox.Show(chart1.Series[0].Points.Count.ToString());
            //d1.AddMilliseconds(-(40* chart1.Series[0].Points.Count));
           // date.AddMilliseconds(-40);
            lbl_time.Text  = "Analysis captured at: " + date.AddMilliseconds(-40).ToString("yyyy/MM/dd - hh:mm:ss:ff tt") + "| Time range[" + date.AddMilliseconds(-40* chart1.Series[0].Points.Count).ToString(" hh:mm:ss:ff tt") +" - " +date.AddMilliseconds(-40).ToString(" hh:mm:ss:ff tt")+ "]";

            //MessageBox.Show( Convert.ToDouble( chart1.Series["Aperture value"].Points[0].xva).ToString(), "", MessageBoxButtons.OK);
          
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void btn_save_Click(object sender, EventArgs e)
        {

          
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;
            saveFileDialog1.InitialDirectory = @"C:\";
            saveFileDialog1.FileName = "VALVE_CHART_CAPTURE_"+ date.AddMilliseconds(-40).ToString("yyyy_MM_dd-hh_mm_ss");
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                for(int j = 0; j < Point; j++)
                {
                    times.Add((decimal.Round((decimal)(final_time - ((chart1.Series[0].Points.Count - j) * 0.040)+0.040), 2)).ToString());
                    apertures.Add((decimal.Round((decimal)chart1.Series["Aperture value"].Points[j].YValues[0], 2)).ToString());
                    pressures.Add((chart1.Series["Pressure"].Points[j].YValues[0]).ToString());
                    datetimes.Add(date_var.AddMilliseconds(-(40 * ((chart1.Series[0].Points.Count + 1) - (j+1)))).ToString("hh:mm:ss:ff tt"));
               
                }

                // Saves the Image via a FileStream created by the OpenFile method.
              
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"" + saveFileDialog1.FileName + ".txt"))
                {
                    file.WriteLine("** MIDORI VALVE **");
                    file.WriteLine("#------------------------------------------------------------------");
                    file.WriteLine("#Datetime: " + date.AddMilliseconds(-40).ToString("yyyy/MM/dd - hh:mm:ss:ff tt"));
                    file.WriteLine("#Data Time range: [" + date.AddMilliseconds(-40 * chart1.Series[0].Points.Count).ToString(" hh:mm:ss:ff tt") + " - " + date.AddMilliseconds(-40).ToString(" hh:mm:ss:ff tt") + "]");
                    file.WriteLine("#Data |Time,seconds,[s],ChartAxisX ");
                    file.WriteLine("#Data |Apperture,grades,[°],ChartAxisY1 ");
                    file.WriteLine("#Data |Pressure,pounds per square inch,[psi],ChartAxisY2 ");
                    file.WriteLine("#------------------------------------------------------------------");
                    file.WriteLine("#PARAMETER    |Chart Type = valve chart capture");
                    file.WriteLine("#PARAMETER    |Valve serie =");
                    file.WriteLine("#PARAMETER    |Valve Software Version =");
                    file.WriteLine("#PARAMETER    |Valve Firmware Version =");
                    file.WriteLine("#PARAMETER    |Position Unit = 0 - 90 =");

                    file.WriteLine("#------------------------------------------------------------------");
                    file.WriteLine("-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-");

                    file.WriteLine("#------------------------------------------------------------------");
                    for (int i = 0; i < times.Count; i++)
                    {

                        file.WriteLine(times[i] + " | " + apertures[i] + " | " + pressures[i] + " | " + datetimes[i]);

                    }
                    file.WriteLine("#------------------------------------------------------------------");
                }
            }
           
        }
    }
}
