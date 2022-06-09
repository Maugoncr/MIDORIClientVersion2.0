﻿/// <summary>
/// Midori valve software
/// </summary>
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
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Management;
using System.Runtime.CompilerServices;

namespace MidoriValveTest
{

    
    public partial class Midori_PV : Form
    {
        //------------------- VARIABLES DE TRABAJO GENERAL DE CODIGO-------------
        System.IO.Ports.SerialPort Arduino;         // Objeto de tipo "serial" port que permite lectoescritura con el puesto seteado. 
        bool record=false;                          // variable que permite determinar si la lectura actual del puerto serial se esta grabando para un archivo.
        public int precision_aperture= 0;           // variable volatil temporal para almacenar la apertura de la valvula
        int base_value = 0;                         // Almacena valores de 10 en 10 hasta 90, incluyendo 0. Esta variable impide el movimiento del trackbar de posicion, fuera del rango inmediato superior de esta base. 
        double tiempo=0;                            // Contador que determina el tiempo de recorrido desde el inicio de la toma de datos
        bool connect = false;                       // Refleja la conexion con el puerto serial. 
        
        DateTime star_record = new DateTime();
        DateTime end_record = new DateTime();

        //--------------- Arreglos de lista (temporales para almacenar el orden de datos a guardar en los archivos de grabacion) -----------------
        private List<string> times = new List<string>();        
        private List<string> apertures = new List<string>();
        private List<string> pressures = new List<string>();
        private List<string> datetimes = new List<string>();

        // funcion de cosntruccion de clase (inicio automatico), inicializa los componentes visuales, (no es recomendado agregar mas funcionamiento a este)
         public Midori_PV() 
                {
                    InitializeComponent();

                }


        // Funcion de carga de procedimientos iniciales (inicio automatico). 
        private void Form1_Load(object sender, EventArgs e)
        {
            button3.Enabled = false;
            string[] ports = SerialPort.GetPortNames();                         // En este arreglo se almacena todos los puertos seriales "COM" registados por la computadora.
            comboBox1.Items.AddRange(ports);                                    // Volcamos el contenido de este arreglo dentro del COMBOBOX de seleccion de puerto
            
            if(ports.Length>0)                                                  // Determina existencia de puertos, y seleccionamos el primero de ellos.
            {
                comboBox1.SelectedIndex = 0;
                button3.Enabled = true;
            }
            lbl_estado.ForeColor = Color.Red;                                   // Establece color rojo al lbl de estado de posicion de valvula. 
            ChartArea CA = chart1.ChartAreas[0];                                //
            CA.CursorX.AutoScroll = true;                                       // Activamos autoescala en la grafica.
                                                                                // 
            btn_90.Enabled = false;
            btn_80.Enabled = false;
            btn_70.Enabled = false;
            btn_60.Enabled = false;
            btn_50.Enabled = false;
            btn_40.Enabled = false;
            btn_30.Enabled = false;
            btn_20.Enabled = false;
            btn_10.Enabled = false;
            btn_0.Enabled = false;

        }

        //Maugoncr// Nos permite comprobar que en caso de que al iniciar la carga del form no habia ningun com para reconocer, en caso de reconocerse luego de esta
        // ser capaces de activar el boton Connect
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                button3.Enabled = true;
            }
            else 
            { 
                button3.Enabled=false;
            }
        }
        


        //Maugoncr// 
        // Reinicia el programa al momento de cuando se arranca por primera vez
        
        private void btnRestart_Click(object sender, EventArgs e)
        {
            // En este arreglo se almacena todos los puertos seriales "COM" registados por la computadora.
            //Boton 3 es el boton de Connect
            button3.Enabled = false;
            string[] ports = SerialPort.GetPortNames();
            //Maugoncr//Validar que no metamos el mismo Puerto COM repetido
            //string[] portsNoRep = ports.Distinct().ToArray();
            //Limpia el combobox y añade el array de los nombres de los puertos
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(ports);

            chart1.Series["Aperture value"].Points.Clear();
            chart1.Series["Pressure"].Points.Clear();

            ChartArea CA = chart1.ChartAreas[0];
            CA.CursorX.AutoScroll = true;

            //Reinicia el tiempo del chart
            tiempo = 0;
            //Detiene la grafica
            timer_Chart.Stop();
            comboBox1.Enabled = true;

            //Maugoncr// Cerramos el puerto y damos 2 segundos al sistema
            Arduino.Close();
            Thread.Sleep(2000);

            //Maugoncr// Cambia el led de encendido a apagado y igual con las etiquetas y desactiva el boton de abrir GATE
            com_led.Image.Dispose();
            com_led.Image = MidoriValveTest.Properties.Resources.led_off;
            LblEstado.Text = "Disconnected *";
            lblPuerto.Text = "Disconnected *";
            btn_encender.Enabled = false;

        }

        // Accion en boton "CONNECT" en la seccion "COM SELECT" 
        private void button3_Click(object sender, EventArgs e)
        {
          try
            {
                if (reconocer_arduino(comboBox1.SelectedItem.ToString()))// Funcion para establecer conexion COM con la valvula. 
                {
                    timer_Chart.Start();
                    com_led.Image.Dispose();
                    com_led.Image = MidoriValveTest.Properties.Resources.led_on;
                    btn_encender.Enabled = true;
                    btn_P_conf.Enabled = true;
                    btn_valveTest.Enabled = true;
                    comboBox1.Enabled = false;
                    button3.Enabled = false;
                    btn_menu.Enabled = true;
                    trackBar2.Enabled = true;
                    trackBar1.Enabled = true;
                    //apertura

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
                Arduino = new System.IO.Ports.SerialPort();
                if (Arduino.IsOpen)
                { Arduino.Close();
                    return false;
                }


                Arduino.PortName = COMM;
                Arduino.BaudRate = 9600;  //se estima para test existen distintas datos 115 200  POSIBLE INCOMPATIBILIDAD POR ESTE DATO
                Arduino.DtrEnable = true;
                Arduino.RtsEnable = true;
                Arduino.Parity = System.IO.Ports.Parity.None;
                Arduino.DataBits = 8;
                Arduino.StopBits = System.IO.Ports.StopBits.One;

                Arduino.Open();
                Thread.Sleep(4000);

                LblEstado.Text = "Connected";
                lblPuerto.Text = COMM;
                connect = true;
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
            Arduino.Write("O");
            Thread.Sleep(50);
    

            //esperamos la señal de movimeinto de partura
            //while (respuesta != "A")
            //{
            //    respuesta = Arduino.ReadExisting(); //MessageBox.Show(respuesta);
            //    Thread.Sleep(100);
            //}
            trackBar1.Value = 90;
            precision_aperture = 90;
            Current_aperture.Text = "Current Aperture:" + precision_aperture + "°";
          
            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._90_2;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources._90_GRADOS2;
            precision_aperture = 90;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
            btn_encender.Enabled = false;
            btn_apagar.Enabled = true;


            btn_90.Enabled = true;
            btn_80.Enabled = true;
            btn_70.Enabled = true;
            btn_60.Enabled = true;
            btn_50.Enabled = true;
            btn_40.Enabled = true;
            btn_30.Enabled = true;
            btn_20.Enabled = true;
            btn_10.Enabled = true;
            btn_0.Enabled = true;




        }

        private void btn_apagar_Click(object sender, EventArgs e)
        {
            
                Arduino.Write("C");
                Thread.Sleep(50);
          

            //esperamos la señal de movimeinto de partura
            //while (respuesta != "B")
            //{
            //    respuesta = Arduino.ReadExisting(); //MessageBox.Show(respuesta);
            //    Thread.Sleep(50);
            //}
            trackBar1.Value = 0;
            precision_aperture = 0;
            Current_aperture.Text = "Current Aperture:" + precision_aperture + "°";
            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._0_2;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS2;
            precision_aperture = 0;
            lbl_estado.ForeColor = Color.Red;
            lbl_estado.Text = "Close";

            btn_encender.Enabled = true;
            btn_apagar.Enabled = false;
            btn_90.Enabled = false;
            btn_80.Enabled = false;
            btn_70.Enabled = false;
            btn_60.Enabled = false;
            btn_50.Enabled = false;
            btn_40.Enabled = false;
            btn_30.Enabled = false;
            btn_20.Enabled = false;
            btn_10.Enabled = false;
            btn_0.Enabled = false;

        }

        private void btn_valveTest_Click(object sender, EventArgs e)
        {
            TestCicles TEST = new TestCicles();
            TEST.Arduino = Arduino;
            TEST.ShowDialog();
        }

        private void btn_0_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._0_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS2;
            base_value = 0;
            trackBar1.Value = 0;
           // precision_aperture = 0;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Red;
            lbl_estado.Text = "Close";

        }

        private void btn_10_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._10_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._10_GRADOS2;
            base_value = 10;
            trackBar1.Value = 10;
           // precision_aperture = 10;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 10";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_20_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._20_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._20_GRADOS2;
            base_value = 20;
            trackBar1.Value = 20;
           // precision_aperture = 20;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 20";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_30_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._30_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._30_GRADOS2;
            base_value = 30;
            trackBar1.Value = 30;
            //precision_aperture = 30;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 30";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_40_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._40_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._40_GRADOS2;
            base_value = 40;
            trackBar1.Value = 40;
            //precision_aperture = 40;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 40";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_50_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._50_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._50_GRADOS2;
            base_value = 50;
            trackBar1.Value = 50;
            //precision_aperture = 50;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 50";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_60_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._60_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._60_GRADOS2;
            base_value = 60;
            trackBar1.Value = 60;
            //precision_aperture = 60;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 60";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_70_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._70_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._70_GRADOS2;
            base_value = 70;
            trackBar1.Value = 70;
            //precision_aperture = 70;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 70";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_80_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._80_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._80_GRADOS2;
            base_value = 80;
            trackBar1.Value = 80;
            //precision_aperture = 80;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 80";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void btn_90_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources._90_2;
            picture_plane.Image = MidoriValveTest.Properties.Resources._90_GRADOS2;
            base_value = 90;
            trackBar1.Value = 90;
            //precision_aperture = 90;
            Current_aperture.Text = "Current Aperture:" + trackBar1.Value+"°";
            btn_set.Text = "Set Aperture in 90";
            btn_set.Enabled = true;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            int pos = trackBar1.Value;


            switch (base_value)
            {
                case 0:
                    if (pos > 9)
                    {
                        trackBar1.Value = 9;

                    }
                    break;
                case 10:
                    if (pos < 10)
                    {
                        trackBar1.Value = 10;
                    }
                    else if (pos > 19)
                    {
                        trackBar1.Value = 19;
                    }
                    break;
                case 20:
                    if (pos < 20)
                    {
                        trackBar1.Value = 20;
                    }
                    else if (pos > 29)
                    {
                        trackBar1.Value = 29;
                    }
                    break;
                case 30:
                    if (pos < 30)
                    {
                        trackBar1.Value = 30;
                    }
                    else if (pos > 39)
                    {
                        trackBar1.Value = 39;
                    }
                    break;
                case 40:
                    if (pos < 40)
                    {
                        trackBar1.Value = 40;
                    }
                    else if (pos > 49)
                    {
                        trackBar1.Value = 49;
                    }
                    break;
                case 50:
                    if (pos < 50)
                    {
                        trackBar1.Value = 50;
                    }
                    else if (pos > 59)
                    {
                        trackBar1.Value = 59;
                    }
                    break;
                case 60:
                    if (pos < 60)
                    {
                        trackBar1.Value = 60;
                    }
                    else if (pos > 69)
                    {
                        trackBar1.Value = 69;
                    }
                    break;
                case 70:
                    if (pos < 70)
                    {
                        trackBar1.Value = 70;
                    }
                    else if (pos > 79)
                    {
                        trackBar1.Value = 79;
                    }
                    break;
                case 80:
                    if (pos < 80)
                    {
                        trackBar1.Value = 80;
                    }
                    else if (pos > 89)
                    {
                        trackBar1.Value = 89;
                    }
                    break;
                case 90:
                    if (pos < 90)
                    {
                        trackBar1.Value = 90;
                    }
                    break;

            }


            btn_set.Enabled = true;
            btn_set.Text = "Set Aperture in " + trackBar1.Value+"°";
            //precision_aperture = trackBar1.Value;


        }


       
        private readonly Random _random = new Random();
        double final = 0.0;
        public decimal pressure_get;
        DateTime n = new DateTime();

        public double s_inicial =13.5555;
        public double s_final =14.6959;

        //Maugoncr// Aqui es donde se algoritman las lineas de manera random 
        private void timer_Chart_Tick(object sender, EventArgs e)
        {
            tiempo = tiempo+40;
            double t = tiempo / 1000;
            final = t;
            //MAUGONCR// En esta variable double se define la presion de manera ramdon con parametros maximos dentro de s_final y s_inicial
            // esta es la causa de los picos
            double rd = _random.NextDouble() * (s_final - s_inicial) + s_inicial; 
            n = DateTime.Now;
            
            chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
            chart1.Series["Pressure"].Points.AddXY(t .ToString(), rd.ToString());
            
            decimal rr = Convert.ToDecimal( rd);
            pressure_get =  decimal.Round(rr, 3);
            lbl_pressure.Text = "Current Pressure: " + pressure_get;
            chart1.ChartAreas[0].RecalculateAxesScale();

            if (chart1.Series["Aperture value"].Points.Count == 349)
            {
            
                chart1.Series["Aperture value"].Points.RemoveAt(0);
                chart1.Series["Pressure"].Points.RemoveAt(0);      



            }


            if (record==true)
            {
                times.Add(t.ToString());
                apertures.Add(precision_aperture.ToString());
                pressures.Add(rd.ToString());
                datetimes.Add(DateTime.Now.ToString("hh:mm:ss:ff tt"));
                lbl_record.Text = "Recording. "+"["+t.ToString()+"]";
            }


        }

        //Maugoncr// Boton de iniciar la grabacion del chart
        private void button1_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Do you want to start recording?, The real time graph will be reset to start recording.", "Midori Valve",MessageBoxButtons.OKCancel)==DialogResult.OK)
            {
                chart1.Series["Aperture value"].Points.Clear();
                chart1.Series["Pressure"].Points.Clear();
                record = true;
                tiempo = 0;
                star_record = DateTime.Now;
                button1.Enabled = false;
                button2.Enabled = true;
                lbl_record.Text = "Recording...";
             


            }
        
        }

        //Maugoncr// Boton de stop para la grabación
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

                            file.WriteLine(times[i] + " | " + apertures[i] + " | " + pressures[i] + " | " + datetimes[i] );

                        }
                        file.WriteLine("#------------------------------------------------------------------");
                    }
                }
                button2.Enabled = false;
                button1.Enabled = true;
                lbl_record.Text = "OFF";
            }
            else
            {
                MessageBox.Show("The recording has not started", "Midori Valve", MessageBoxButtons.OK);
            }
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox11_Enter(object sender, EventArgs e)
        {

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
            //MessageBox.Show((chart1.Series[0].Points[0].XValue).ToString(), "", MessageBoxButtons.OK);
            
           
            ca.ShowDialog();

        }

        private void lbl_record_Click(object sender, EventArgs e)
        {
            
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
                        //Dado el caso, verifico que exista el archivo..
                        if (System.IO.File.Exists(FileToRead))
                        {
                            //Lo ejecuto.
                            //System.Diagnostics.Process.Start(FileToRead);
                            // Creating string array  
                            string[] lines = File.ReadAllLines(FileToRead);
                            for (int i = 0; i < lines.Length; i++)
                            {
                               

                                using (StreamReader tr = new StreamReader(FileToRead))
                                {
                                    cd.richTextBox1.Text = tr.ReadToEnd();
                                }

                                if (lines[i].Contains("#Data Time range: ["))
                                {
                                    //MessageBox.Show(lines[i]);
                                    
                                    range=  lines[i].Replace("#Data Time range: [", string.Empty);
                                    MessageBox.Show(range);
                                    //MessageBox.Show(lines[i].Replace("#Data Time range:", string.Empty));
                                    range = range.Remove(range.Length - 1);
                                    //MessageBox.Show(range);
                                    // range = range.Remove(0);
                                    // MessageBox.Show(range);
                                    times = range.Split('-');
                                    //MessageBox.Show(times[0]);

                                    //MessageBox.Show(times[1]);
                                    cd.ini_range = times[0];
                                    cd.end_range = times[1];


                                }

                                    if (lines[i] == "-|-  Time  -|-  Apperture  -|-  Pressure  -|-  DateTime  -|-" && lines[i+1]== "#------------------------------------------------------------------") 
                                {
                                    initial_line = i + 2;

                                    // MessageBox.Show((initial_line).ToString());
                                    //break;
                                }

                                
                            }
                            for (int y = initial_line; y < lines.Length - 1;y++)
                            {
                                line_in_depure = lines[y].Split('|');
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
                            //Caso que la ruta tenga la extensión correcta, pero el archivo
                            //no exista en el disco
                            MessageBox.Show("El archivo no existe.");
                        }
                    }
                    else
                    {
                        //Caso de que la extensión sea incorrecta.
                        MessageBox.Show("El formato del archivo no es correcto.");
                    }
                }
            }







          
        
            
           
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if(connect==true)
            {
                LateralNav.Size = new Size(419, 1019);
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
            nt.Arduino = Arduino;
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

        private void label39_Click(object sender, EventArgs e)
        {

        }

        private void groupBox11_Enter_1(object sender, EventArgs e)
        {

        }

        private void btn_P_conf_Click(object sender, EventArgs e)
        {
            unit_form un = new unit_form();
            un.ob = this;
            un.ShowDialog();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            btn_S_pressure.Enabled = true;
            btn_S_pressure.Text = "Set target pressure in " + (float)trackBar2.Value/10000;

            switch (lbl_P_unit_top.Text)
            {
                case "PSI":
                    btn_S_pressure.Text = "Set target pressure in " + (float)trackBar2.Value / 10000;
                    break;
                case "ATM":
                    btn_S_pressure.Text = "Set target pressure in " + (float)trackBar2.Value / 1000;
                    break;
                case "mbar":
                    btn_S_pressure.Text = "Set target pressure in " + (float)trackBar2.Value / 100;
                    break;
                case "Torr":
                    btn_S_pressure.Text = "Set target pressure in " + (float)trackBar2.Value ;
                    break;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

      

        //Maugoncr// Set clic de la apertura AZUL ESTE SIRVE
        private void btn_set_Click(object sender, EventArgs e)
        {
            // 

            precision_aperture = trackBar1.Value;
            Current_aperture.Text = "Current Aperture:" + precision_aperture + "°";
            btn_set.Text = "Set Aperture";
            btn_set.Enabled = false;
            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
            Arduino.Write(precision_aperture.ToString());

        }


        //Maugoncr// Set clic de la presión VERDE
        private void btn_S_pressure_Click_1(object sender, EventArgs e)
        {
            //pressure_get = trackBar2.Value;
            //lbl_pressure.Text = "Current Pressure:" + pressure_get + "°";
            //btn_S_pressure.Text = "Set Pressure";
            //btn_S_pressure.Enabled = false;
            //// lbl_estado.ForeColor = Color.Green;
            //// lbl_estado.Text = "Open";
            //Arduino.Write(pressure_get.ToString());


        }
    }
}
