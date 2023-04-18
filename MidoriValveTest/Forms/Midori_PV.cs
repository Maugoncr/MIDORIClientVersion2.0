/// <summary>
/// Midori valve software
/// </summary>
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using CustomMessageBox;
using MidoriValveTest.Forms;

namespace MidoriValveTest
{

    
    public partial class Midori_PV : Form
    {
        //------------------- Work variable ------------
        bool record=false;                          // flag for record data
        public int precision_aperture= 0;           // apperture
        int base_value = 0;                         // ranges for apperture 
        bool InicioStartPID = true;                 // Flag for btnStarPID sent P or T
        bool connect = false;                       // flag for connect status
        public static bool EnviarPID = false;       // flag for Sent the PID
        double rt = 0;                              // Time X from chart
        double temp = 0;                            // Time in ms
        bool MostrarSetPoint = false;

        DateTime star_record = new DateTime();
        DateTime end_record = new DateTime();

        //--------------- (Temp LIST for record data) -----------------
        private List<string> times = new List<string>();        
        private List<string> apertures = new List<string>();
        private List<string> pressures = new List<string>();
        private List<string> datetimes = new List<string>();

        //var for menu
        private const int widthSlide = 200;
        private const int widthSlideIcon = 46;
        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        // contructor
         public Midori_PV() 
         {
                    InitializeComponent();
                    // Load inicial settings
                    InitializeSetting();
                    // For avoid problems with delay in the chart
                    Control.CheckForIllegalCrossThreadCalls = false;
         }

        public void InitializeSetting() {

            // For Torr Units
            s_inicial = 755;
            s_final = 760;
            trackBar2A.Maximum = 760;
            lbl_T_0.Text = "0";
            lbl_T_1.Text = "84.44";
            lbl_T_2.Text = "168.88";
            lbl_T_3.Text = "253.32";
            lbl_T_4.Text = "337.76";
            lbl_T_5.Text = "422.2";
            lbl_T_6.Text = "506.64";
            lbl_T_7.Text = "591.08";
            lbl_T_8.Text = "675.52";
            lbl_T_9.Text = "760";
            lbl_units_track.Text = "Torr";
            lbl_P_unit_top.Text = "Torr";
            lbl_presure_chart.Text = "[Torr]";

        }

        //Load the main Form
        private void Form1_Load(object sender, EventArgs e)
        {
            OffEverything();

            timer1.Enabled = true;
            if (cbSelectionCOM.Items.Count > 0)     // exist ports com
            {
                cbSelectionCOM.SelectedIndex = 0;
                EnableBtn(btnConnect);
            }
        }

        private void OffEverything()
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }

            //Timers
            timerForData.Stop();

            // Return all the variables from ObjetosGlobales to default
            ObjetosGlobales.P = "x";
            ObjetosGlobales.I = "x";
            ObjetosGlobales.D = "x";
            ObjetosGlobales.flagPID = false;
            ObjetosGlobales.ApperCali = 90;

            //Return all variables to default from MIDORI_PV

            record = false;
            InicioStartPID = true;
            i = false;
            AutocalibracionPrendida = false;
            base_value = 0;
            precision_aperture = 0;
            times = new List<string>();
            apertures = new List<string>();
            pressures = new List<string>();
            datetimes = new List<string>();
            EnviarPID = false;
            MostrarSetPoint = false;
            Manual = true;
            Auto = false;
            AxisY2Maximo= 1000;

            // Return all labels to default text

            LblEstado.Text = "Disconnected *";
            LblEstado.ForeColor = Color.FromArgb(15,60,89);
            lblPuerto.ForeColor = Color.FromArgb(15,60,89);
            lblPuerto.Text = "Disconnected *";
            lbl_estado.Text = "OFF";
            lbl_record.Text = "OFF";
            Current_aperture.Text = "0°";
            lb_Temperature.Text = " 0 °C";
            lbl_pressure.Text = "0";
            lbSetPointPressure.Text = "---";

            //Return texts btn to default

            btnSetApertura.Text = "Set Apperture";
            btnSetPresion.Text = "Set Target Pressure";
            btnStartPID.Text = "Start PID";
            btnAutoCalibrate.Text = "Autocalibration";
            txtSetPresion.Clear();

            // Load COM
            cbSelectionCOM.Enabled = true;
            string[] ports = SerialPort.GetPortNames();
            cbSelectionCOM.Items.Clear();
            cbSelectionCOM.Items.AddRange(ports);

            //Enable Buttons

            //Disable Buttons
            DisableBtn(btnOpenGate);
            DisableBtn(btnCloseGate);
            DisableBtn(btnSetApertura);
            DisableBtn(btnSetPresion);
            DisableBtn(btnStartPID);
            DisableBtn(btnStartRecord);
            DisableBtn(btnStopRecord);
            DisableBtn(btnClear);
            DisableBtn(btnChartArchiveAnalyzer);
            DisableBtn(btnAnalyze);
            DisableBtn(btnPIDAnalisis);
            DisableBtn(btnAutoCalibrate);
            DisableBtn(btnOEM);
            DisableBtn(btnConnect);
            iconPID.Enabled = false;
            txtSetPresion.Enabled = false;

            //Buttons for Degrees
            DisableBtn(btn_90);
            DisableBtn(btn_80);
            DisableBtn(btn_70);
            DisableBtn(btn_60);
            DisableBtn(btn_50);
            DisableBtn(btn_40);
            DisableBtn(btn_30);
            DisableBtn(btn_20);
            DisableBtn(btn_10);
            DisableBtn(btn_0);

            //Disable Trackbars
            trackBar1A.Enabled = false;
            trackBar2A.Enabled = false;
            trackBar2A.Value = 0;
            trackBar1A.Value = 0;

            // Visual Valve IMG's
            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front0;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti0B;

            //Led status
            com_led.Image.Dispose();
            com_led.Image = MidoriValveTest.Properties.Resources.led_off;

            // Chart
            chart1.Series["Aperture value"].Points.Clear();
            chart1.Series["Pressure"].Points.Clear();
            ChartArea CA = chart1.ChartAreas[0];
            CA.CursorX.AutoScroll = true;

        }


        // General function for disable buttoms

        private void DisableBtn(Button btn) { 
            btn.BackgroundImage.Dispose();
            btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
            btn.Enabled = false;
            btn.ForeColor = Color.Black;
        }

        private void EnableBtn(Button btn)
        {

            btn.BackgroundImage.Dispose();
            btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
            btn.Enabled = true;
            btn.ForeColor = Color.Black;

        }


        //Select COM
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbSelectionCOM.SelectedIndex >= 0)
            {
                EnableBtn(btnConnect);
                
            }
            else 
            {
                DisableBtn(btnConnect);
            }

        }



        //Maugoncr// 
        // Reboot the whole system as when it started up

        private void btnRestart_Click(object sender, EventArgs e)
        {
            OffEverything();
            this.Alert("Successfully restarted", Form_Alert.enmType.Success);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            OffEverything();
            this.Alert("Successfully stoped", Form_Alert.enmType.Success);
        }

        // Connecting
        private void button3_Click(object sender, EventArgs e)
        {
          try
            {
                if (reconocer_arduino(cbSelectionCOM.SelectedItem.ToString()))// Funcion para establecer conexion COM con la valvula. 
                {
                    // Start running
                    timerForData.Start();
                    txtSetPresion.Enabled = true;
                    com_led.Image.Dispose();
                    com_led.Image = MidoriValveTest.Properties.Resources.led_on_green;
                    EnableBtn(btnOpenGate);
                    btn_P_conf.Enabled = true;
                   // EnableBtn(btn_valveTest);
                    cbSelectionCOM.Enabled = false;
                    DisableBtn(btnConnect);
                    EnableBtn(btnStartPID);
                   // EnableBtn(btnOnMANValve);


                    // Menu settings
                    btnMenu.Enabled = true;
                    iconTerminal.Enabled = true;
                    iconPID.Enabled = true;
                    IconSensor.Enabled = true;
                    IconTrace.Enabled = true;
                    IconReport.Enabled = true;

                    trackBar1A.Enabled = true;
                    trackBar2A.Enabled = true;
                    
                    btnAutoCalibrate.Enabled = true;
                    btnPIDAnalisis.Enabled = true;
                    
                    EnableBtn(btn_90);
                    EnableBtn(btn_80);
                    EnableBtn(btn_70);
                    EnableBtn(btn_60);
                    EnableBtn(btn_50);
                    EnableBtn(btn_40);
                    EnableBtn(btn_30);
                    EnableBtn(btn_20);
                    EnableBtn(btn_10);
                    EnableBtn(btn_0);
                    EnableBtn(btnStartRecord);
                    EnableBtn(btnChartArchiveAnalyzer);
                    EnableBtn(btnAnalyze);
                }              
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        public bool reconocer_arduino(string COMM)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    return false;
                }
                // Remember Baud Rate
                serialPort1.PortName = COMM;
                serialPort1.Open();
                LblEstado.Text = "Connected";
                lblPuerto.Text = COMM;
                connect = true;

                string validarData = serialPort1.ReadExisting();

                if (validarData == null || validarData == "") {
                    LblEstado.Text = "Disconnected *";
                    lblPuerto.Text = "Disconnected *";
                    serialPort1.Close();

                    MessageBoxMaugoncr.Show("Data is not being received correctly. The program will not start until this is fixed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);


                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                LblEstado.Text = "Disconnected *";
                lblPuerto.Text = "Disconnected *";
                return false;
            }
        }
        private void btn_encender_Click(object sender, EventArgs e)
        {
            trackBar1A.Enabled = true;
            trackBar2A.Enabled = true;

            //Maugoncr// Valide default degrees
            if (trackBar1A.Value != 0)
            {
                precision_aperture = trackBar1A.Value;
            }
            else
            {
                precision_aperture = 90;
                picture_frontal.Image.Dispose();
                picture_frontal.Image = MidoriValveTest.Properties.Resources.Front90;
                picture_plane.Image.Dispose();
                picture_plane.Image = MidoriValveTest.Properties.Resources.Verti90B;
            }

            serialPort1.Write(precision_aperture.ToString());
            Thread.Sleep(50);

            Current_aperture.Text =  precision_aperture + "°";
            lbl_estado.ForeColor = Color.Red;
            lbl_estado.Text = "Open";
            DisableBtn(btnOpenGate);
            EnableBtn(btnCloseGate);

            EnableBtn(btn_90);
            EnableBtn(btn_80);
            EnableBtn(btn_70);
            EnableBtn(btn_60);
            EnableBtn(btn_50);
            EnableBtn(btn_40);
            EnableBtn(btn_30);
            EnableBtn(btn_20);
            EnableBtn(btn_10);
            EnableBtn(btn_0);

            
            EnableBtn(btnSetApertura);
           // EnableBtn(btnInfo);
            EnableBtn(btnChartArchiveAnalyzer);
            EnableBtn(btnOEM);
            EnableBtn(btnAnalyze);
            //clear
            EnableBtn(btnClear);
            //stop
            EnableBtn(btnStopRecord);
            // grabar
            EnableBtn(btnStartRecord);

        }

        private void btn_apagar_Click(object sender, EventArgs e)
        {
            
            serialPort1.Write("0");
            Thread.Sleep(50);
            trackBar1A.Value = 0;
            precision_aperture = 0;
            Current_aperture.Text =  precision_aperture + "°";
            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front0;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti0B;
            lbl_estado.ForeColor = Color.Red;
            lbl_estado.Text = "Close";
            btnSetPresion.Text = "Set Target Pressure";
            btnSetApertura.Text = "Set Apperture";
            EnableBtn(btnOpenGate);
            DisableBtn(btnCloseGate);   
            DisableBtn(btnSetApertura);

        }

        private void btn_valveTest_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is TestCicles);
            if (frm == null)
            {
                TestCicles TEST = new TestCicles();
                TEST.menssager = this;
                TEST.Arduino = serialPort1;
                TEST.Show();
            }
            else
            {
                frm.BringToFront();
                return;
            }
        }

        private void btn_0_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front0;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti0B;
            base_value = 0;
            trackBar1A.Value = 0;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btnSetApertura.Text = "Set Aperture";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
            }

        }

        private void btn_10_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front10;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti10B;
            base_value = 10;
            trackBar1A.Value = 10;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 10";
            }
        }

        private void btn_20_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front20;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti20B;
            base_value = 20;
            trackBar1A.Value = 20;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 20";
            }
        }

        private void btn_30_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front30;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti30B;
            base_value = 30;
            trackBar1A.Value = 30;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 30";

            }
        }

        private void btn_40_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front40;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti40B;
            base_value = 40;
            trackBar1A.Value = 40;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 40";
            }
        }

        private void btn_50_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front50;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti50B;
            base_value = 50;
            trackBar1A.Value = 50;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 50";
            }

        }

        private void btn_60_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front60;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti60B;
            base_value = 60;
            trackBar1A.Value = 60;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 60";
            }
        }

        private void btn_70_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front70;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti70B;
            base_value = 70;
            trackBar1A.Value = 70;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 70";
            }
        }

        private void btn_80_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front80;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti80B;
            base_value = 80;
            trackBar1A.Value = 80;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
               
                EnableBtn(btnSetApertura);
                btnSetApertura.Text = "Set Aperture in 80";
            }

        }

        private void btn_90_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front90;
            picture_plane.Image = MidoriValveTest.Properties.Resources.Verti90B;
            base_value = 90;
            trackBar1A.Value = 90;
            Current_aperture.Text =  trackBar1A.Value+"°";
            if (lbl_estado.Text == "Open")
            {
                btnSetApertura.Enabled = true;
                btnSetApertura.Text = "Set Aperture in 90";
            }

        }

        
       



        private readonly Random _random = new Random();
        double final = 0.0;
        public decimal pressure_get;
        DateTime n = new DateTime();
        public double s_inicial = 13.5555;
        public double s_final = 14.6959;
        //Maugoncr// Aqui es donde se algoritman las lineas de manera random 
        private void timer_Chart_Tick(object sender, EventArgs e)
        {
            //tiempo = tiempo + 100;
            //double t = tiempo / 1000;
            //final = t;
            ////MAUGONCR// En esta variable double se define la presion de manera ramdon con parametros maximos dentro de s_final y s_inicial
            //// esta es la causa de los picos
            //// ya no se usa
            //double rd = _random.NextDouble() * (s_final - s_inicial) + s_inicial;
            //n = DateTime.Now;
            ////if (lbl_estado.Text == "Open")
            ////{
            //try
            //{
            //    if (Arduino != null && Arduino.IsOpen)
            //    {
            //        string test = Arduino.ReadLine();
            //        if (!test.Equals(null))
            //        {
            //            chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
            //            chart1.Series["Pressure"].Points.AddXY(t.ToString(), ObtenerData(test, 2).ToString());
            //            lbl_pressure.Text = ObtenerData(test, 2);
            //            lb_Temperature.Text = ObtenerData(test, 1) + " °C";
            //            chart1.ChartAreas[0].RecalculateAxesScale();
            //        }
            //        else {
            //            MessageBox.Show("No se recibe datos");
            //        }
            //    }
            //    else
            //    {
            //        chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
            //        chart1.Series["Pressure"].Points.AddXY(t.ToString(), 8.ToString());
            //        lbl_pressure.Text = "8";
            //        chart1.ChartAreas[0].RecalculateAxesScale();
            //    }
            //}
            //catch (Exception)
            //{

            //    //throw;
            //}
            //if (chart1.Series["Aperture value"].Points.Count == 349)
            //{
            //    chart1.Series["Aperture value"].Points.RemoveAt(0);
            //    chart1.Series["Pressure"].Points.RemoveAt(0);
            //}
            //if (record==true)
            //{
            //    times.Add(t.ToString());
            //    apertures.Add(precision_aperture.ToString());
            //    pressures.Add(rd.ToString());
            //    datetimes.Add(DateTime.Now.ToString("hh:mm:ss:ff tt"));
            //    lbl_record.Text = "Recording. "+"["+t.ToString()+"]";
            //}

        }

        //Maugoncr// Start Record
        private void button1_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to start recording?, The real time graph will be reset to start recording.", "Midori Valve",MessageBoxButtons.OKCancel)==DialogResult.OK)
            {
                chart1.Series["Aperture value"].Points.Clear();
                chart1.Series["Pressure"].Points.Clear();
                record = true;
                rt = 0;
                star_record = DateTime.Now;
                DisableBtn(btnStartRecord);
                EnableBtn(btnStopRecord);
                lbl_record.Text = "Recording...";
            }
        
        }

        //Maugoncr// Stop Record
        private void button2_Click(object sender, EventArgs e)
        {
            if (record == true)
            {
                record = false;
                end_record = DateTime.Now;
                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog1.FilterIndex = 2;
                saveFileDialog1.RestoreDirectory = true;
                saveFileDialog1.InitialDirectory = @"C:\";
                saveFileDialog1.FileName = "VALVE_RECORD_" + end_record.AddMilliseconds(-40).ToString("yyyy_MM_dd-hh_mm_ss");
                saveFileDialog1.ShowDialog();
            
                if (saveFileDialog1.FileName != "")
                {
                   

                    // Saves the Image via a FileStream created by the OpenFile method.

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"" + saveFileDialog1.FileName + ".txt"))
                    {
                        file.WriteLine("** MIDORI VALVE **");
                        file.WriteLine("#------------------------------------------------------------------");
                        file.WriteLine("#Datetime: " + star_record.ToString("yyyy/MM/dd - hh:mm:ss:ff tt"));
                        file.WriteLine("#Data Time range: [" + star_record.ToString(" hh:mm:ss:ff tt") + " - " +end_record.ToString(" hh:mm:ss:ff tt") + "]");
                        file.WriteLine("#Data |Time,seconds,[s],ChartAxisX ");
                        file.WriteLine("#Data |Apperture,grades,[°],ChartAxisY1 ");
                        file.WriteLine("#Data |Pressure,pounds per square inch,[psi],ChartAxisY2 ");
                        file.WriteLine("#------------------------------------------------------------------");
                        file.WriteLine("#PARAMETER    |Chart Type = valve record");
                        file.WriteLine("#PARAMETER    |Valve serie =");
                        file.WriteLine("#PARAMETER    |Valve Software Version =");
                        file.WriteLine("#PARAMETER    |Valve Firmware Version =");
                        file.WriteLine("#PARAMETER    |Position Unit = 0 - 90 =");

                        file.WriteLine("#------------------------------------------------------------------");
                        file.WriteLine("-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-");

                        file.WriteLine("#------------------------------------------------------------------");
                        for (int i = 0; i < times.Count; i++)
                        {

                            file.WriteLine(times[i] + " , " + apertures[i] + " , " + pressures[i] + " , " + datetimes[i] );

                        }
                        file.WriteLine("#------------------------------------------------------------------");
                    }
                }
                //button2.Enabled = false;
                DisableBtn(btnStopRecord);
                //button1.Enabled = true;
                EnableBtn(btnStartRecord);
                times.Clear();
                apertures.Clear();
                pressures.Clear();
                datetimes.Clear();

                lbl_record.Text = "OFF";
            }
            else
            {
                MessageBox.Show("The recording has not started", "Midori Valve", MessageBoxButtons.OK);
            }
        }


        private void button5_Click(object sender, EventArgs e)
        {
           
            Chart_Analyzer ca = new Chart_Analyzer();
            ca.final_time =final;
            ca.date = n;
            for ( int i = 0; i< chart1.Series["Aperture value"].Points.Count;i++)
            {
                ca.chart1.Series["Aperture value"].Points.Add(chart1.Series["Aperture value"].Points[i]);
                ca.chart1.Series["Pressure"].Points.Add(chart1.Series["Pressure"].Points[i]);
            }
           
            ca.ShowDialog();

        }

        
        //Chart Analicer
        private void button7_Click(object sender, EventArgs e)
        {

            Chart_Analyzer_File cd = new Chart_Analyzer_File();
            string[] line_in_depure;
         List<string> times_1 = new List<string>();
         List<string> apertures_1 = new List<string>();
         List<string> pressures_1 = new List<string>();
         List<string> datetimes_1 = new List<string>();

        OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "Texto | *.txt";

        int initial_line = 0;
            string range = "";
            string[] times;
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                string FileToRead = OpenFile.FileName;
                cd.archivo = FileToRead;
                using (StreamReader sr = new StreamReader(FileToRead))
                {
                    if (System.IO.Path.GetExtension(FileToRead).ToLower() == ".txt")
                    {
                        if (System.IO.File.Exists(FileToRead))
                        {
                            string[] lines = File.ReadAllLines(FileToRead);
                            for (int i = 0; i < lines.Length; i++)
                            {
                               

                                using (StreamReader tr = new StreamReader(FileToRead))
                                {
                                    cd.richTextBox1.Text = tr.ReadToEnd();
                                }

                                if (lines[i].Contains("#Data Time range: ["))
                                {
                                    range=  lines[i].Replace("#Data Time range: [", string.Empty);
                                    MessageBox.Show(range);
                                    range = range.Remove(range.Length - 1);
                                    times = range.Split('-');
                                    cd.ini_range = times[0];
                                    cd.end_range = times[1];


                                }

                                    if (lines[i] == "-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-" && lines[i+1]== "#------------------------------------------------------------------") 
                                {
                                    initial_line = i + 2;
                                }

                                
                            }
                            for (int y = initial_line; y < lines.Length - 1;y++)
                            {
                                line_in_depure = lines[y].Split(',');
                                times_1.Add( line_in_depure[0]);
                                apertures_1.Add(line_in_depure[1]);
                                pressures_1.Add(line_in_depure[2]);
                                datetimes_1.Add(line_in_depure[3]);
                                Console.WriteLine(String.Join(Environment.NewLine, line_in_depure[0] + " " + line_in_depure[1] + " " + line_in_depure[2] + " " + line_in_depure[3]));

                            }

                            cd.times = times_1;
                            cd.apertures = apertures_1;
                            cd.pressures = pressures_1;
                            cd.datetimes = datetimes_1;
                            cd.Show();
                        }
                        else
                        {
                            MessageBox.Show("Doesnt exist.");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalided format");
                    }
                }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            LateralNav.Size = new Size(0, 1019);
         
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Terminal nt = new Terminal();
            LateralNav.Size = new Size(0, 1019);
            nt.lblPuerto.Text = "Connected";
            nt.mvt = this;
            nt.Arduino = serialPort1;
                nt.ShowDialog();
            
        }

        private void Midori_PV_MouseClick(object sender, MouseEventArgs e)
        {
            if (LateralNav.Width!=0)
            {
                LateralNav.Size = new Size(0, 1019);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PID_Config nt = new PID_Config();
            
            nt.ShowDialog();


        }

        private void btn_P_conf_Click(object sender, EventArgs e)
        {
            unit_form un = new unit_form();
            un.ob = this;
            un.ShowDialog();
        }


        //Maugoncr// Set clic de la apertura AZUL ESTE SIRVE
        private void btn_set_Click(object sender, EventArgs e)
        {
            precision_aperture = trackBar1A.Value;
            Current_aperture.Text =  precision_aperture + "°";
            btnSetApertura.Text = "Set Aperture";
            DisableBtn(btnSetApertura);
            lbl_estado.ForeColor = Color.Red;
            lbl_estado.Text = "Open";
            serialPort1.Write(precision_aperture.ToString());

        }
        
        private void btn_S_pressure_Click_1(object sender, EventArgs e)
        {
            double presion = trackBar2A.Value;


            switch (lbl_P_unit_top.Text)
            {
                case "PSI":
                    if (presion <= 1469 && presion > 1306)
                    {
                        s_inicial = 130624 / 10000;
                        s_final = 146959 / 10000;
                    }
                    else if (presion <= 1306 && presion > 1142)
                    {
                        s_inicial = 114296 / 10000;
                        s_final = 130624 / 10000;
                    }
                    else if (presion <= 1142 && presion > 979)
                    {
                        s_inicial = 97968 / 10000;
                        s_final = 114296 / 10000;
                    }
                    else if (presion <= 979 && presion > 816)
                    {
                        s_inicial = 81640 / 10000;
                        s_final = 97968 / 10000;
                    }
                    else if (presion <= 816 && presion > 653)
                    {
                        s_inicial = 65312 / 10000;
                        s_final = 81640 / 10000;
                    }
                    else if (presion <= 653 && presion > 489)
                    {
                        s_inicial = 48984 / 10000;
                        s_final = 65312 / 10000;
                    }
                    else if (presion <= 489 && presion > 326)
                    {
                        s_inicial = 32656 / 10000;
                        s_final = 48954 / 10000;
                    }
                    else if (presion <= 326 && presion > 163)
                    {
                        s_inicial = 16328 / 10000;
                        s_final = 32656 / 10000;
                    }
                    else if (presion <= 163 && presion > 0)
                    {
                        s_inicial = 0;
                        s_final = 16328 / 10000;
                    };


                    break;

                case "ATM":

                    presion = presion / 1000;

                    if (presion <= 1 && presion > 0.88)
                    {
                        s_inicial = 0.88 ;
                        s_final = 1 ;
                    }
                    else if (presion <= 0.88 && presion > 0.77)
                    {
                        s_inicial = 0.77 ;
                        s_final = 0.88 ;
                    }
                    else if (presion <= 0.77 && presion > 0.66)
                    {
                        s_inicial = 0.66 ;
                        s_final = 0.77 ;
                    }
                    else if (presion <= 0.66 && presion > 0.55)
                    {
                        s_inicial = 0.55 ;
                        s_final = 0.66 ;
                    }
                    else if (presion <= 0.55 && presion > 0.44)
                    {
                        s_inicial = 0.44 ;
                        s_final = 0.55 ;
                    }
                    else if (presion <= 0.44 && presion > 0.33)
                    {
                        s_inicial = 0.33 ;
                        s_final = 0.44 ;
                    }
                    else if (presion <= 0.33 && presion > 0.22)
                    {
                        s_inicial = 0.22 / 1000;
                        s_final = 0.33 / 1000;
                    }
                    else if (presion <= 0.22 && presion > 0.11)
                    {
                        s_inicial = 0.11 ;
                        s_final = 0.22 ;
                    }
                    else if (presion <= 0.11 && presion > 0)
                    {
                        s_inicial = 0;
                        s_final = 0.11 ;
                    };



                    break;
                case "mbar":

                    

                    if (presion <= 1013 && presion > 900)
                    {
                        s_inicial = 900.6664;
                        s_final = 1013.25;
                    }
                    else if (presion <= 900 && presion > 788)
                    {
                        s_inicial = 788.0831;
                        s_final = 900.6664;
                    }
                    else if (presion <= 788 && presion > 675)
                    {
                        s_inicial = 675.4998;
                        s_final = 788.0831;
                    }
                    else if (presion <= 675 && presion > 562)
                    {
                        s_inicial = 562.9165;
                        s_final = 675.4998;
                    }
                    else if (presion <= 562 && presion > 450)
                    {
                        s_inicial = 450.3332;
                        s_final = 562.9165;
                    }
                    else if (presion <= 450 && presion > 337)
                    {
                        s_inicial = 337.7499;
                        s_final = 450.3332;
                    }
                    else if (presion <= 337 && presion > 225)
                    {
                        s_inicial = 225.1666;
                        s_final = 337.7499;
                    }
                    else if (presion <= 225 && presion > 112)
                    {
                        s_inicial = 112.5833;
                        s_final = 225.1666;
                    }
                    else if (presion <= 112 && presion > 0)
                    {
                        s_inicial = 0;
                        s_final = 112.5833;
                    };



                    break;


                case "Torr":

                    //Send to arduino
                    // S120,x,x,x

                    string envioConFormato = "S" + presion.ToString() + ",x,x,x";
                    lbSendPID.Text = envioConFormato;
                    serialPort1.Write(envioConFormato);
                    lbSetPointPressure.Text = presion.ToString();

                    break;
            }
        }
        

        private void timer1_Tick(object sender, EventArgs e)
        {
            string fecha = DateTime.Now.ToString("dddd, MM/dd/yyyy");
            lblhora.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblfecha.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fecha);

            if (EnviarPID)
            {
                EnviarPID = false;
                string PIDFormat = "Sx," + ObjetosGlobales.P + "," + ObjetosGlobales.I + "," + ObjetosGlobales.D;
                lbPIDSent.Text = PIDFormat;
                //ENVIAR
                serialPort1.Write(PIDFormat);

            }

        }

        private void IconClose_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            Application.Exit();

        }

        private void iconBar_Click(object sender, EventArgs e)
        {
            if (PanelSideNav.Width != widthSlideIcon)
            {
                PanelSideNav.Width = widthSlideIcon;
            }
            else {
                PanelSideNav.Width = widthSlide;
            }

        }


        private void iconPID_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is PID_Config);

            if (frm == null)
            {
                PID_Config nt = new PID_Config();
               
                nt.Show();
            }
            else
            {
                frm.BringToFront();
                return;
            }

        }

        //Maugoncr//
        // move form
        private void PanelNav_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void IconMaxin_Click(object sender, EventArgs e)
        {
            if (WindowState==FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
            else if (WindowState==FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }

        }

        private void IconMinima_Click(object sender, EventArgs e)
        {
            if (WindowState==FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
            else if (WindowState==FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Minimized;
            }
        }
        
      

        private void btnInfo_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is ChooseDWG);

            if (frm == null)
            {
                ChooseDWG nt = new ChooseDWG();
                nt.ShowDialog();
            }
            else
            {
                frm.BringToFront();
                return;
            }
        }

        private void IconInfo_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Information);

            if (frm == null)
            {
                Information nt = new Information();
                nt.Show();
            }
            else
            {
                frm.BringToFront();
                return;
            }
        }

        private void EnterBtn(Button btn) {
            if (btn.Enabled == true)
            {
                btn.BackgroundImage.Dispose();
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnNblue;
                btn.ForeColor = Color.White;
            }
        }

        private void LeftBtn(Button btn) {
            if (btn.Enabled == true)
            {
                btn.BackgroundImage.Dispose();
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
                btn.ForeColor = Color.Black;
                if (btn.Name == "btnOnMANValve")
                {
                    //btnOnMANValve.IconColor = Color.Black;
                }
                if (btn.Name == "btnOffMANValve")
                {
                   // btnOffMANValve.IconColor = Color.Black;
                }
            }
            else
            {
                btn.BackgroundImage.Dispose();
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
                btn.ForeColor = Color.Black;
            }
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnConnect);
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnConnect);
        }

        private void btnRestart_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnRestart);
        }

        private void btnRestart_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnRestart);
        }

        private void btnStop_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnStop);
        }

        private void btnStop_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnStop);
        }

        private void btn_encender_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnOpenGate);
        }

        private void btn_encender_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnOpenGate);
        }

        private void btn_apagar_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnCloseGate);
        }

        private void btn_apagar_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnCloseGate);
        }

        private void btn_valveTest_MouseEnter(object sender, EventArgs e)
        {
          //  EnterBtn(btn_valveTest);
        }

        private void btn_valveTest_MouseLeave(object sender, EventArgs e)
        {
          //  LeftBtn(btn_valveTest);
        }

        private void btnInfo_MouseEnter(object sender, EventArgs e)
        {
          //  EnterBtn(btnInfo);
        }

        private void btnInfo_MouseLeave(object sender, EventArgs e)
        {
          //  LeftBtn(btnInfo);
        }

        private void btn_set_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnSetApertura);
        }

        private void btn_set_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnSetApertura);
        }

        private void btn_S_pressure_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnSetPresion);
        }

        private void btn_S_pressure_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnSetPresion);
        }

        private void btn_90_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_90);
        }

        private void btn_90_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_90);
        }

        private void btn_80_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_80);
        }

        private void btn_80_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_80);
        }

        private void btn_70_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_70);
        }

        private void btn_70_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_70);
        }

        private void btn_60_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_60);
        }

        private void btn_60_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_60);
        }

        private void btn_50_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_50);
        }

        private void btn_50_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_50);
        }

        private void btn_40_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_40);
        }

        private void btn_40_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_40);
        }

        private void btn_30_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_30);
        }

        private void btn_30_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_30);
        }

        private void btn_20_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_20);
        }

        private void btn_20_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_20);
        }

        private void btn_10_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_10);
        }

        private void btn_10_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_10);
        }

        private void btn_0_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_0);
        }

        private void btn_0_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_0);
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnStartRecord);
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnStartRecord);
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnStopRecord);
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnStopRecord);
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnClear);
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnClear);
        }

        private void button7_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnChartArchiveAnalyzer);
        }

        private void button7_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnChartArchiveAnalyzer);
        }

        private void button6_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnOEM);
        }

        private void button6_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnOEM);
        }

        private void button5_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnAnalyze);
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnAnalyze);
        }
        private void iconCamera_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Forms.Camara);

            if (frm == null)
            {
                Forms.Camara nt = new Forms.Camara();
                nt.Show();

            }
            else
            {
                frm.BringToFront();
                return;
            }
        }

        private void trackBar1A_Scroll(object sender, EventArgs e)
        {
            int pos = trackBar1A.Value;

            switch (base_value)
            {
                case 0:
                    if (pos > 9)
                    {
                        trackBar1A.Value = 9;

                    }
                    break;
                case 10:
                    if (pos < 10)
                    {
                        trackBar1A.Value = 10;
                    }
                    else if (pos > 19)
                    {
                        trackBar1A.Value = 19;
                    }
                    break;
                case 20:
                    if (pos < 20)
                    {
                        trackBar1A.Value = 20;
                    }
                    else if (pos > 29)
                    {
                        trackBar1A.Value = 29;
                    }
                    break;
                case 30:
                    if (pos < 30)
                    {
                        trackBar1A.Value = 30;
                    }
                    else if (pos > 39)
                    {
                        trackBar1A.Value = 39;
                    }
                    break;
                case 40:
                    if (pos < 40)
                    {
                        trackBar1A.Value = 40;
                    }
                    else if (pos > 49)
                    {
                        trackBar1A.Value = 49;
                    }
                    break;
                case 50:
                    if (pos < 50)
                    {
                        trackBar1A.Value = 50;
                    }
                    else if (pos > 59)
                    {
                        trackBar1A.Value = 59;
                    }
                    break;
                case 60:
                    if (pos < 60)
                    {
                        trackBar1A.Value = 60;
                    }
                    else if (pos > 69)
                    {
                        trackBar1A.Value = 69;
                    }
                    break;
                case 70:
                    if (pos < 70)
                    {
                        trackBar1A.Value = 70;
                    }
                    else if (pos > 79)
                    {
                        trackBar1A.Value = 79;
                    }
                    break;
                case 80:
                    if (pos < 80)
                    {
                        trackBar1A.Value = 80;
                    }
                    else if (pos > 89)
                    {
                        trackBar1A.Value = 89;
                    }
                    break;
                case 90:
                    if (pos < 90)
                    {
                        trackBar1A.Value = 90;
                    }
                    break;


            }


            //btn_set.Enabled = true;
            btnSetApertura.Text = "Set Aperture in " + trackBar1A.Value + "°";
            //precision_aperture = trackBar1.Value;
        }


        private void trackBar2A_Scroll(object sender, EventArgs e)
        {
            EnableBtn(btnSetPresion);
            float nivel = trackBar2A.Value;
            
            switch (lbl_P_unit_top.Text)
            {
                case "PSI":
                    btnSetPresion.Text = "Set target pressure in " + nivel / 100;
                    break;
                case "ATM":
                    btnSetPresion.Text = "Set target pressure in " + nivel / 1000;
                    break;
                case "mbar":
                    btnSetPresion.Text = "Set target pressure in " + nivel;
                    break;
                case "Torr":
                    btnSetPresion.Text = "Set target pressure in " + nivel;
                    txtSetPresion.Text = trackBar2A.Value.ToString();
                    break;
            }
        }

        //Reset this flag
        Boolean i = false;
        string capturadatos;
        public string presionChart;
        public string temperaturaLabel;
        public string presionSetPoint;

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (i==false)
            {
                rt = 0;
                i = true;
            }

            try
            {
                if (!serialPort1.ReadLine().Contains("-"))
                {
                    if (serialPort1.ReadLine().Contains("$"))
                    {
                        //lbl_Test.Invoke(new Action(() => lbl_Test.Text = serialPort1.ReadLine().ToString()));
                        lbl_Test.Text = serialPort1.ReadLine();
                        capturadatos = serialPort1.ReadLine();
                        ObtenerData(capturadatos);
                        serialPort1.DiscardInBuffer();
                    }
                }
            }
            catch (Exception)
            {

                
            }
        }

        private void ObtenerData(string full)
        {

            string test = full;
            test.Trim();
            bool firtIn = false;
            bool secondIn = false;
            bool thirdIn = false;
            string Temp = "";
            string Pressure = "";
            string PressureSetPoint = "";
            // A120,250J180$

            for (int i = 0; i < test.Length; i++)
            {
                if (test.Substring(i, 1).Equals("$"))
                {
                    break;
                }
                if (thirdIn == true)
                {
                    PressureSetPoint += test.Substring(i, 1);
                }
                if (test.Substring(i, 1).Equals("J"))
                {
                    secondIn = false;
                    thirdIn = true;
                }
                if (secondIn == true)
                {
                    Pressure += test.Substring(i, 1);
                }
                if (test.Substring(i, 1).Equals(","))
                {
                    firtIn = false;
                    secondIn = true;
                }
                if (firtIn == true)
                {
                    Temp += test.Substring(i, 1);
                }
                if (test.Substring(i, 1).Equals("A"))
                {
                    firtIn = true;
                }
            }
            Temp.Replace("A", "");
            Pressure.Replace("$", "");
            PressureSetPoint.Replace("J", "");

           
            temperaturaLabel = Temp;
            presionSetPoint = PressureSetPoint;

            try
            {
                switch (lbl_P_unit_top.Text)
                {
                    case "PSI":
                        if (Pressure != "")
                        {
                            double presionPSI = Math.Round(Convert.ToDouble(Pressure) / 51.715, 4);
                            presionChart = presionPSI.ToString();
                        }
                        break;
                    case "mbar":
                        if (Pressure != "")
                        {
                            double presionMBAR = Math.Round(Convert.ToDouble(Pressure) * 1.33322, 4);
                            presionChart = presionMBAR.ToString();
                        }
                        break;
                    case "ATM":
                        if (Pressure != "")
                        {
                            double presionATM = Math.Round(Convert.ToDouble(Pressure) / 760, 4);
                            presionChart = presionATM.ToString();
                        }
                        break;
                    case "Torr":
                        presionChart = Pressure;
                        break;
                }
            }
            catch (Exception)
            {

            }
          
        }

        private void timerForData_Tick(object sender, EventArgs e)
        {
            rt = rt + 100;
            temp = rt / 1000;


            if (serialPort1.IsOpen && i == true && presionChart != null && temperaturaLabel != null && presionSetPoint != null)
            {
                chart1.Series["Aperture value"].Points.AddXY(temp.ToString(), precision_aperture.ToString());
                chart1.Series["Pressure"].Points.AddXY(temp.ToString(), presionChart.ToString());

                if (MostrarSetPoint)
                {
                    lbSetPointPressure.Text = presionSetPoint;
                }
                lbl_pressure.Text = (presionChart);
                lb_Temperature.Text = temperaturaLabel + " °C";

                if (Auto)
                {
                    chart1.ChartAreas[0].AxisY2.Maximum = Double.NaN;
                    chart1.ChartAreas[0].AxisY2.Minimum = Double.NaN;
                    chart1.ChartAreas[0].RecalculateAxesScale();
                }
                if (Manual)
                {
                    chart1.ChartAreas[0].AxisY.Minimum = 0;
                    chart1.ChartAreas[0].AxisY.Maximum = 100;
                    chart1.ChartAreas[0].AxisY2.Minimum = 0;
                    chart1.ChartAreas[0].AxisY2.Maximum = AxisY2Maximo;
                }
               

            }

            if (chart1.Series["Aperture value"].Points.Count == 349)
            {

                chart1.Series["Aperture value"].Points.RemoveAt(0);
                chart1.Series["Pressure"].Points.RemoveAt(0);
            }

            if (record == true)
            {
                if (AutocalibracionPrendida == true)
                {
                    times.Add(temp.ToString());
                    apertures.Add(precision_aperture.ToString());
                    pressures.Add(presionChart.ToString());
                    datetimes.Add(DateTime.Now.ToString("hh:mm:ss:ff tt"));
                    lbl_record.Text = "Calibrating. " + "[" + temp.ToString() + "]";
                }
                else
                {
                    times.Add(temp.ToString());
                    apertures.Add(precision_aperture.ToString());
                    pressures.Add(presionChart.ToString());
                    datetimes.Add(DateTime.Now.ToString("hh:mm:ss:ff tt"));
                    lbl_record.Text = "Recording. " + "[" + temp.ToString() + "]";
                }
            }

        }

        private void btnPIDAnalisis_Click(object sender, EventArgs e)
        {
            PIDAnalize MiPidAnalisis = new PIDAnalize();
            string[] line_in_depure;
            List<string> times_1 = new List<string>();
            List<string> apertures_1 = new List<string>();
            List<string> pressures_1 = new List<string>();
            List<string> datetimes_1 = new List<string>();

            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "Texto | *.txt";

            int initial_line = 0;
            string range = "";
            string[] times;
            if (OpenFile.ShowDialog() == DialogResult.OK)
            {
                string FileToRead = OpenFile.FileName;
                MiPidAnalisis.archivo = FileToRead;
                using (StreamReader sr = new StreamReader(FileToRead))
                {
                    if (Path.GetExtension(FileToRead).ToLower() == ".txt")
                    {
                        if (File.Exists(FileToRead))
                        {
                            // Creating string array  
                            string[] lines = File.ReadAllLines(FileToRead);
                            for (int i = 0; i < lines.Length; i++)
                            {
                                if (lines[i].Contains("#Data Time range: ["))
                                {
                                    range = lines[i].Replace("#Data Time range: [", string.Empty);
                                    range = range.Remove(range.Length - 1);
                                    times = range.Split('-');
                                    MiPidAnalisis.ini_range = times[0];
                                    MiPidAnalisis.end_range = times[1];


                                }

                                if (lines[i] == "-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-" && lines[i + 1] == "#------------------------------------------------------------------")
                                {
                                    initial_line = i + 2;
                                }
                            }
                            for (int y = initial_line; y < lines.Length - 1; y++)
                            {
                                line_in_depure = lines[y].Split(',');
                                times_1.Add(line_in_depure[0]);
                                apertures_1.Add(line_in_depure[1]);
                                pressures_1.Add(line_in_depure[2]);
                                datetimes_1.Add(line_in_depure[3]);
                                Console.WriteLine(String.Join(Environment.NewLine, line_in_depure[0] + " " + line_in_depure[1] + " " + line_in_depure[2] + " " + line_in_depure[3]));

                            }
                            MiPidAnalisis.times = times_1;
                            MiPidAnalisis.apertures = apertures_1;
                            MiPidAnalisis.pressures = pressures_1;
                            MiPidAnalisis.datetimes = datetimes_1;
                            MiPidAnalisis.Show();
                        }
                        else
                        {
                            MessageBoxMaugoncr.Show("File doesn't exist.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBoxMaugoncr.Show("Invalid Format.", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnPIDAnalisis_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnPIDAnalisis);
        }

        private void btnPIDAnalisis_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnPIDAnalisis);
        }

        public void AutoCalibrarANDRecord()
        {
            if (AutocalibracionPrendida == false)
            {
                if (MessageBoxMaugoncr.Show("Do you want to start autocalibration?, The real time graph will be reset to start.", "!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    chart1.Series["Aperture value"].Points.Clear();
                    chart1.Series["Pressure"].Points.Clear();
                    record = true;
                    rt = 0;
                    star_record = DateTime.Now;
                    precision_aperture = 0;
                    serialPort1.Write(precision_aperture.ToString());
                    base_value = 0;
                    trackBar1A.Value = 0;
                    Current_aperture.Text = trackBar1A.Value + "°";
                    lbl_record.Text = "Calibrating...";
                    AutocalibracionPrendida = true;
                    btnAutoCalibrate.Text = "Stop";
                    picture_frontal.Image.Dispose();
                    picture_plane.Image.Dispose();
                    picture_frontal.Image = Properties.Resources.Front0;
                    picture_plane.Image = Properties.Resources.Verti0B;

                    if (MessageBoxMaugoncr.Show("PRESS OK TO START", "!", MessageBoxButtons.OK, MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        precision_aperture = ObjetosGlobales.ApperCali;
                        //Send apper Electronic
                        serialPort1.Write(ObjetosGlobales.ApperCali.ToString());
                        base_value = ObjetosGlobales.ApperCali;
                        trackBar1A.Value = ObjetosGlobales.ApperCali;
                        Current_aperture.Text = trackBar1A.Value + "°";
                        lbl_estado.ForeColor = Color.Red;
                        lbl_estado.Text = "Open";
                    }


                }
            }
            else
            {
                precision_aperture = 0;
                Current_aperture.Text = precision_aperture + "°";
                serialPort1.Write("0");
                trackBar2A.Enabled = false;
                trackBar2A.Value = 0;
                trackBar1A.Value = 0;
                picture_frontal.Image.Dispose();
                picture_frontal.Image = Properties.Resources.Front0;
                picture_plane.Image.Dispose();
                picture_plane.Image = Properties.Resources.Verti0B;
                lbl_estado.ForeColor = Color.Red;
                lbl_estado.Text = "Close";
                btnSetPresion.Text = "Set Target Pressure";
                btnSetApertura.Text = "Set Apperture";
                EnableBtn(btnOpenGate);
                DisableBtn(btnCloseGate);
                DisableBtn(btnSetApertura);

                if (record == true)
                {
                    record = false;
                    end_record = DateTime.Now;
                    saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog1.FilterIndex = 2;
                    saveFileDialog1.RestoreDirectory = true;
                    saveFileDialog1.InitialDirectory = @"C:\";
                    saveFileDialog1.FileName = "VALVE_CALIBRATION_" + end_record.AddMilliseconds(-40).ToString("yyyy_MM_dd-hh_mm_ss");
                    saveFileDialog1.ShowDialog();

                    if (saveFileDialog1.FileName != "")
                    {
                        // Saves the Image via a FileStream created by the OpenFile method.

                        using (StreamWriter file = new StreamWriter(@"" + saveFileDialog1.FileName + ".txt"))
                        {
                            file.WriteLine("** MIDORI VALVE **");
                            file.WriteLine("#------------------------------------------------------------------");
                            file.WriteLine("#Datetime: " + star_record.ToString("yyyy/MM/dd - hh:mm:ss:ff tt"));
                            file.WriteLine("#Data Time range: [" + star_record.ToString(" hh:mm:ss:ff tt") + " - " + end_record.ToString(" hh:mm:ss:ff tt") + "]");
                            file.WriteLine("#Data |Time,seconds,[s],ChartAxisX ");
                            file.WriteLine("#Data |Apperture,grades,[°],ChartAxisY1 ");
                            file.WriteLine("#Data |Pressure,pounds per square inch,[psi],ChartAxisY2 ");
                            file.WriteLine("#------------------------------------------------------------------");
                            file.WriteLine("#PARAMETER    |Chart Type = valve record");
                            file.WriteLine("#PARAMETER    |Valve serie =");
                            file.WriteLine("#PARAMETER    |Valve Software Version =");
                            file.WriteLine("#PARAMETER    |Valve Firmware Version =");
                            file.WriteLine("#PARAMETER    |Position Unit = 0 - 90 =");

                            file.WriteLine("#------------------------------------------------------------------");
                            file.WriteLine("-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-");

                            file.WriteLine("#------------------------------------------------------------------");
                            for (int i = 0; i < times.Count; i++)
                            {

                                file.WriteLine(times[i] + " , " + apertures[i] + " , " + pressures[i] + " , " + datetimes[i]);

                            }
                            file.WriteLine("#------------------------------------------------------------------");
                        }

                        MessageBoxMaugoncr.Show("Autocalibration data successfully saved", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    lbl_record.Text = "OFF";
                    AutocalibracionPrendida = false;
                    btnAutoCalibrate.Text = "Autocalibration";
                    times.Clear();
                    apertures.Clear();
                    pressures.Clear();
                    datetimes.Clear();
                }
            }
        }
        public static bool AutocalibracionPrendida = false;
        private void btnAutoCalibrate_Click(object sender, EventArgs e)
        {
            if (trackBar1A.Value != 0)
            {
                ObjetosGlobales.ApperCali = trackBar1A.Value;
                AutoCalibrarANDRecord();
            }
            else
            {
                AutoCalibrarANDRecord();
            }
        }
        private void btnAutoCalibrate_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnAutoCalibrate);
        }

        private void btnAutoCalibrate_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnAutoCalibrate);
        }

        
        private void btnStartPID_Click(object sender, EventArgs e)
        {
            if (InicioStartPID)
            {
                serialPort1.Write("P");
                btnStartPID.Text = "Stop PID";
                InicioStartPID = false;
                MostrarSetPoint = true;
            }
            else
            {
                serialPort1.Write("T");
                btnStartPID.Text = "Start PID";
                InicioStartPID = true;
                MostrarSetPoint = false;
                lbSetPointPressure.Text = "---";
            }


        }
        private void btnStartPID_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnStartPID);
        }

        private void btnStartPID_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnStartPID);
        }

           
      
       
        //private void ReadExistTimer_Tick(object sender, EventArgs e)
        //{
        //    txtRead.Text += serialPort1.ReadExisting();
        //    txtRead.Select(txtRead.TextLength + 1, 0);
        //    txtRead.ScrollToCaret();
        //}

      
      
        public void Alert(string msg, Form_Alert.enmType type)
        {
            Form_Alert frm = new Form_Alert();
            frm.showAlert(msg, type);
        }
     
        private void EncenderBTN(Button btn)
        {
            if (btn.Enabled == true)
            {
                if (btn.Name == "btnOnMANValve")
                {
                    btn.ForeColor = Color.White;
                   // btnOnMANValve.IconColor = Color.White;
                    btn.BackgroundImage.Dispose();
                    btn.BackgroundImage = Properties.Resources.btnOn;
                }
                else
                {
                    btn.ForeColor = Color.White;
                    //btnOffMANValve.IconColor = Color.White;
                    btn.BackgroundImage.Dispose();
                    btn.BackgroundImage = Properties.Resources.btnOff;
                }
            }
        }

        private void txtSetPresion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (lbl_P_unit_top.Text == "Torr")
            {
                if ((e.KeyChar >= 32 && e.KeyChar <= 47) || (e.KeyChar >= 58 && e.KeyChar <= 255))
                {
                    MessageBoxMaugoncr.Show("Only numbers are allowed", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                if ((e.KeyChar >= 32 && e.KeyChar <= 45) || (e.KeyChar >= 58 && e.KeyChar <= 255) || e.KeyChar == 47)
                {
                    MessageBoxMaugoncr.Show("Only numbers are allowed", "Important", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtSetPresion_TextChanged(object sender, EventArgs e)
        {
           
            switch (lbl_P_unit_top.Text)
            {
                case "PSI":
                  //
                    break;
                case "ATM":
                   //
                    break;
                case "mbar":
                  //
                    break;
                case "Torr":
                    if (!string.IsNullOrEmpty(txtSetPresion.Text.Trim()))
                    {
                        int txtTorrUnit = Convert.ToInt32(txtSetPresion.Text.Trim());
                        if (txtTorrUnit <= 760)
                        {
                            trackBar2A.Value = txtTorrUnit;
                            btnSetPresion.Text = "Set target pressure in " + txtTorrUnit;
                            EnableBtn(btnSetPresion);
                        }
                        else
                        {
                            MessageBoxMaugoncr.Show("Invalide Number", "Important", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            txtSetPresion.Clear();
                            trackBar2A.Value = 0;
                            btnSetPresion.Text = "Set Target Pressure";
                            DisableBtn(btnSetPresion);
                        }
                    }
                    break;
            }
        }


        int AxisY2Maximo = 1000;
        bool Auto = false;
        bool Manual = false;

        //0-1000
        private void torrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Auto = false;
            Manual = true;
            AxisY2Maximo = 1000;
        }
        //0-500
        private void torrToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Auto = false;
            Manual = true;
            AxisY2Maximo = 500;
        }

        // 0-100
        private void torrToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Auto = false;
            Manual = true;
            AxisY2Maximo = 100;
        }

        //Auto
        private void scaleAutoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Auto = true;
            Manual = false;
            AxisY2Maximo = 1000;
        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void btnOEM_Click(object sender, EventArgs e)
        {

        }
    }
}


