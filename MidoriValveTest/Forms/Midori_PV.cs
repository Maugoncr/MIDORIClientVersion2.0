/// <summary>
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
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace MidoriValveTest
{

    
    public partial class Midori_PV : Form
    {
        Stopwatch oSW = new Stopwatch();

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

        //var for menu

        private const int widthSlide = 200;
        private const int widthSlideIcon = 46;
        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);


        // funcion de cosntruccion de clase (inicio automatico), inicializa los componentes visuales, (no es recomendado agregar mas funcionamiento a este)
         public Midori_PV() 
                {
                    InitializeComponent();
                    InitializeSetting();
                }

        public void InitializeSetting() {
            this.FormBorderStyle = FormBorderStyle.None;
           
        }

       


        // Funcion de carga de procedimientos iniciales (inicio automatico). 
        private void Form1_Load(object sender, EventArgs e)
        {
            
            iconTerminal.Enabled = false;
            iconPID.Enabled = false;
            IconSensor.Enabled = false;
            IconTrace.Enabled = false;
            IconReport.Enabled = false;
            DisableBtn(button3);
            DisableBtn(btn_valveTest);
            DisableBtn(btn_set);
            DisableBtn(btn_S_pressure);
            DisableBtn(btn_encender);
            DisableBtn(btn_apagar);
            string[] ports = SerialPort.GetPortNames();                         // En este arreglo se almacena todos los puertos seriales "COM" registados por la computadora.
            comboBox1.Items.AddRange(ports);                                    // Volcamos el contenido de este arreglo dentro del COMBOBOX de seleccion de puerto

            timer1.Enabled = true;
            //TimerForData.Enabled = true;

            if(ports.Length>0)                                                  // Determina existencia de puertos, y seleccionamos el primero de ellos.
            {
                comboBox1.SelectedIndex = 0;
                EnableBtn(button3);
                //button3.Enabled = true;
            }
            lbl_estado.ForeColor = Color.SteelBlue;                                   // Establece color rojo al lbl de estado de posicion de valvula. 
            ChartArea CA = chart1.ChartAreas[0];                                //
            CA.CursorX.AutoScroll = true;                                       // Activamos autoescala en la grafica.
                                                                                // 
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
            trackBar1A.Enabled = false;
            trackBar2A.Enabled = false;
            DisableBtn(button7);
            DisableBtn(button6);
            DisableBtn(button5);
            DisableBtn(button4);
            DisableBtn(button2);
            DisableBtn(button1);
        }

        // Metodo para cambiar para remplazar el enable disable button y usar uno general

        private void DisableBtn(Button btn) { 
            
            btn.BackgroundImage.Dispose();
            btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
            btn.Enabled = false;
            btn.ForeColor = Color.White;

        }

        private void EnableBtn(Button btn)
        {

            btn.BackgroundImage.Dispose();
            btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnNBlue3;
            btn.Enabled = true;

        }


        //Maugoncr// Nos permite comprobar que en caso de que al iniciar la carga del form no habia ningun com para reconocer, en caso de reconocerse luego de esta
        // ser capaces de activar el boton Connect


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                EnableBtn(button3);
            }
            else 
            {
                DisableBtn(button3);
            }
        }



        //Maugoncr// 
        // Reboot the whole system as when it started up

        private void btnRestart_Click(object sender, EventArgs e)
        {
            
            //Disable SideMenu since you connect again
            iconTerminal.Enabled = false;
            iconPID.Enabled = false;
            IconSensor.Enabled = false;
            IconTrace.Enabled = false;
            IconReport.Enabled = false;
            DisableBtn(button3);
            // En este arreglo se almacena todos los puertos seriales "COM" registados por la computadora.
            // Boton 3 es el boton de Connect
            // button3.Enabled = false;
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

            //Restart the chart timer
            tiempo = 0;
            //Stop the Chart
            timer_Chart.Stop();
            comboBox1.Enabled = true;
            // hEEEEY
            DisableBtn(btn_set);
            //btn_set.Enabled = false;
            btn_set.Text = "Set Apperture";

            //Maugoncr//because if there is nothing connected and we use the Arduino object we would get an error because it would be null.
            //Close the port and wait for 2s
            if (Arduino != null)
            {
                //Arduino.Write("0");
                Arduino.Close();
                Thread.Sleep(2000);
            }

            if (lbl_estado.Text == "Open")
            {

                trackBar1A.Enabled = false;
                trackBar2A.Enabled = false;
                trackBar1A.Value = 0;
                precision_aperture = 0;
                Current_aperture.Text =  precision_aperture + "°";
                picture_frontal.Image.Dispose();
                picture_frontal.Image = MidoriValveTest.Properties.Resources._0_2;
                picture_plane.Image.Dispose();
                picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS2;
                precision_aperture = 0;
                lbl_estado.ForeColor = Color.SteelBlue;
                lbl_estado.Text = "Close";
                DisableBtn(btn_apagar);
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
                
               
            }

            if (DateStartedTest.Text != "-/-/-")
            {
                TestCicles.greenlight = false;
                TestCicles.counter = 0;
                DateStartedTest.Text = "-/-/-";
                DateEndedTest.Text = "-/-/-";
                green_off.Image.Dispose();
                green_off.Image = MidoriValveTest.Properties.Resources.led_off_green;
                yellow_off.Image.Dispose();
                yellow_off.Image = MidoriValveTest.Properties.Resources.led_off_yellow;
                red_off.Image.Dispose();
                red_off.Image = MidoriValveTest.Properties.Resources.led_off_red;
                lb_CounterTest.Text = "0";
               


            }

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
            //Maugoncr// Turn off the led and the same for labels, disable the button of Open Gate
            com_led.Image.Dispose();
            com_led.Image = MidoriValveTest.Properties.Resources.led_off;
            LblEstado.Text = "Disconnected *";
            lblPuerto.Text = "Disconnected *";
           // btn_encender.Enabled = false;
            DisableBtn(btn_encender);
            lbl_pressure.Text = " 0 ";
            //btn_valveTest.Enabled = false;
            DisableBtn(btn_valveTest);

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
                    com_led.Image = MidoriValveTest.Properties.Resources.led_on_green;
                    //btn_encender.Enabled = true;
                    EnableBtn(btn_encender);
                    btn_P_conf.Enabled = true;
                    EnableBtn(btn_valveTest);
                    //btn_valveTest.Enabled = true;
                    comboBox1.Enabled = false;
                    //button3.Enabled = false;
                    DisableBtn(button3);

                    // Menu settings
                    btn_menu.Enabled = true;
                    iconTerminal.Enabled = true;
                    iconPID.Enabled = true;
                    IconSensor.Enabled = true;
                    IconTrace.Enabled = true;
                    IconReport.Enabled = true;               
                    //button3.Enabled = true;
                    //EnableBtn(button3);
                    //apertura
                    trackBar1A.Enabled = true;
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
                Arduino.ReadTimeout = 500;
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
            //Arduino.Write("90");
            //Thread.Sleep(50);


            //esperamos la señal de movimeinto de partura
            //while (respuesta != "A")
            //{
            //    respuesta = Arduino.ReadExisting(); //MessageBox.Show(respuesta);
            //    Thread.Sleep(100);
            //}

            trackBar1A.Enabled = true;
            trackBar2A.Enabled = true;

            //Maugoncr// Enviamos los grados que deseamos se abra la valvula desde el open gate.
            precision_aperture = trackBar1.Value;
            Arduino.Write(precision_aperture.ToString());
            Thread.Sleep(50);

            Current_aperture.Text =  precision_aperture + "°";

            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front90;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources._90_GRADOS;
            //precision_aperture = 90;

            lbl_estado.ForeColor = Color.SteelBlue;
            lbl_estado.Text = "Open";
            //btn_encender.Enabled = false;
            DisableBtn(btn_encender);

           // btn_apagar.Enabled = true;
            EnableBtn(btn_apagar);

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

            //btn_set.Enabled = true;
            EnableBtn(btn_set);
            EnableBtn(btnInfo);
            EnableBtn(button7);
            EnableBtn(button6);
            EnableBtn(button5);
            EnableBtn(button4);
            EnableBtn(button2);
            EnableBtn(button1);

        }

        private void btn_apagar_Click(object sender, EventArgs e)
        {
            
                Arduino.Write("0");
                Thread.Sleep(50);
          

            //esperamos la señal de movimeinto de partura
            //while (respuesta != "B")
            //{
            //    respuesta = Arduino.ReadExisting(); //MessageBox.Show(respuesta);
            //    Thread.Sleep(50);
            //}
            //trackBar1.Enabled = false;
            trackBar2A.Enabled = false;

            trackBar1A.Value = 0;
            precision_aperture = 0;
            Current_aperture.Text =  precision_aperture + "°";
            picture_frontal.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front0;
            picture_plane.Image.Dispose();
            picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS;
            precision_aperture = 0;
            lbl_estado.ForeColor = Color.SteelBlue;
            lbl_estado.Text = "Close";

            //btn_encender.Enabled = true;
            EnableBtn(btn_encender);
            
            //btn_apagar.Enabled = false;
            DisableBtn(btn_apagar);   

            //btn_90.Enabled = false;
            //btn_80.Enabled = false;
            //btn_70.Enabled = false;
            //btn_60.Enabled = false;
            //btn_50.Enabled = false;
            //btn_40.Enabled = false;
            //btn_30.Enabled = false;
            //btn_20.Enabled = false;
            //btn_10.Enabled = false;
            //btn_0.Enabled = false;

            //btn_set.Enabled=false;
            DisableBtn(btn_set);

        }

        private void btn_valveTest_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is TestCicles);
            if (frm == null)
            {
                TestCicles TEST = new TestCicles();
                TEST.menssager = this;
                TEST.Arduino = Arduino;
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
            picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS2;
            base_value = 0;
            trackBar1A.Value = 0;
           // precision_aperture = 0;
            Current_aperture.Text =  trackBar1.Value+"°";
            btn_set.Text = "Set Aperture";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Red;
            //lbl_estado.Text = "Close";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }

        }

        private void btn_10_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front10;
            picture_plane.Image = MidoriValveTest.Properties.Resources._10_GRADOS;
            base_value = 10;
            trackBar1A.Value = 10;
           // precision_aperture = 10;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 10";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }
        }

        private void btn_20_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front20;
            picture_plane.Image = MidoriValveTest.Properties.Resources._20_GRADOS;
            base_value = 20;
            trackBar1A.Value = 20;
           // precision_aperture = 20;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 20";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
             
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }
        }

        private void btn_30_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front30;
            picture_plane.Image = MidoriValveTest.Properties.Resources._30_GRADOS;
            base_value = 30;
            trackBar1A.Value = 30;
            //precision_aperture = 30;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 30";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);

            }
        }

        private void btn_40_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front40;
            picture_plane.Image = MidoriValveTest.Properties.Resources._40_GRADOS;
            base_value = 40;
            trackBar1A.Value = 40;
            //precision_aperture = 40;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 40";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }
        }

        private void btn_50_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front50;
            picture_plane.Image = MidoriValveTest.Properties.Resources._50_GRADOS;
            base_value = 50;
            trackBar1A.Value = 50;
            //precision_aperture = 50;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 50";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }

        }

        private void btn_60_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front60;
            picture_plane.Image = MidoriValveTest.Properties.Resources._60_GRADOS;
            base_value = 60;
            trackBar1A.Value = 60;
            //precision_aperture = 60;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 60";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";

            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }
        }

        private void btn_70_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front70;
            picture_plane.Image = MidoriValveTest.Properties.Resources._70_GRADOS;
            base_value = 70;
            trackBar1A.Value = 70;
            //precision_aperture = 70;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 70";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";

            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }
        }

        private void btn_80_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front80;
            picture_plane.Image = MidoriValveTest.Properties.Resources._80_GRADOS;
            base_value = 80;
            trackBar1A.Value = 80;
            //precision_aperture = 80;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 80";
            //btn_set.Enabled = true;
            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            if (lbl_estado.Text == "Open")
            {
                //btn_set.Enabled = true;
                EnableBtn(btn_set);
            }

        }

        private void btn_90_Click(object sender, EventArgs e)
        {
            picture_frontal.Image.Dispose();
            picture_plane.Image.Dispose();
            picture_frontal.Image = MidoriValveTest.Properties.Resources.Front90;
            picture_plane.Image = MidoriValveTest.Properties.Resources._90_GRADOS;
            base_value = 90;
            trackBar1A.Value = 90;
            //precision_aperture = 90;
            Current_aperture.Text =  trackBar1A.Value+"°";
            btn_set.Text = "Set Aperture in 90";

            //lbl_estado.ForeColor = Color.Green;
            //lbl_estado.Text = "Open";
            //btn_set.Enabled = true;

            if (lbl_estado.Text == "Open")
            {
                btn_set.Enabled = true;
            }

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


            //btn_set.Enabled = true;
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
            tiempo = tiempo + 40;
            double t = tiempo / 1000;
            final = t;
            //MAUGONCR// En esta variable double se define la presion de manera ramdon con parametros maximos dentro de s_final y s_inicial
            // esta es la causa de los picos
            double rd = _random.NextDouble() * (s_final - s_inicial) + s_inicial;
            n = DateTime.Now;

            if (lbl_estado.Text == "Open")
            {
                //if (Arduino != null && Arduino.IsOpen)
                //{
                    //string hola = Arduino.ReadLine();
                    //hola = hola.Replace("$", "");
                    //hola = hola.Replace("s", "");
                    //chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
                    //chart1.Series["Pressure"].Points.AddXY(t.ToString(), hola.ToString());
                    //lbl_pressure.Text = hola.ToString();
                    //chart1.ChartAreas[0].RecalculateAxesScale();
                //}
                //else
               // {
                    chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
                    chart1.Series["Pressure"].Points.AddXY(t.ToString(), rd.ToString());
                    decimal rr = Convert.ToDecimal(rd);
                    pressure_get = decimal.Round(rr, 3);
                    lbl_pressure.Text = pressure_get.ToString();
                    chart1.ChartAreas[0].RecalculateAxesScale();
                //}
            }
            else
            {

                chart1.Series["Aperture value"].Points.AddXY(t.ToString(), precision_aperture.ToString());
                chart1.Series["Pressure"].Points.AddXY(t.ToString(), 8.ToString());
                lbl_pressure.Text = "8";
                chart1.ChartAreas[0].RecalculateAxesScale();

            }

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
                //button1.Enabled = false;
                DisableBtn(button1);

                //button2.Enabled = true;
                EnableBtn(button2);
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
                //button2.Enabled = false;
                DisableBtn(button2);

                //button1.Enabled = true;
                EnableBtn(button1);

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
            //btn_S_pressure.Enabled = true;
            EnableBtn(btn_S_pressure);
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

            precision_aperture = trackBar1A.Value;
            Current_aperture.Text =  precision_aperture + "°";
            btn_set.Text = "Set Aperture";
            //btn_set.Enabled = false;
            DisableBtn(btn_set);

            lbl_estado.ForeColor = Color.Green;
            lbl_estado.Text = "Open";
            Arduino.Write(precision_aperture.ToString());

        }


        //Maugoncr// Set clic de la presión VERDE
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
                   

                    if (presion <= 760 && presion > 675.52)
                    {
                        s_inicial = 675.52;
                        s_final = 760;
                    }
                    else if (presion <= 675.52 && presion > 591.08)
                    {
                        s_inicial = 591.08;
                        s_final = 675.52;
                    }
                    else if (presion <= 591.08 && presion > 506.64)
                    {
                        s_inicial = 506.64;
                        s_final = 591.08;
                    }
                    else if (presion <= 506.64 && presion > 422.2)
                    {
                        s_inicial = 422.2;
                        s_final = 506.64;
                    }
                    else if (presion <= 422.2 && presion > 337.76)
                    {
                        s_inicial = 422.2;
                        s_final = 337.76;
                    }
                    else if (presion <= 337.76 && presion > 253.32)
                    {
                        s_inicial = 253.32;
                        s_final = 337.76;
                    }
                    else if (presion <= 253.32 && presion > 168.88)
                    {
                        s_inicial = 168.88;
                        s_final = 253.32;
                    }
                    else if (presion <= 168.88 && presion > 84.44)
                    {
                        s_inicial = 84.44;
                        s_final = 168.88;
                    }
                    else if (presion <= 84.44 && presion >= 0)
                    {
                        s_inicial = 0;
                        s_final = 84.44;
                    };

                    break;
            }


            //pressure_get = trackBar2.Value;
            //lbl_pressure.Text = "Current Pressure:" + pressure_get + "°";
            //btn_S_pressure.Text = "Set Pressure";
            //btn_S_pressure.Enabled = false;
            DisableBtn(btn_S_pressure);
            //// lbl_estado.ForeColor = Color.Green;
            //// lbl_estado.Text = "Open";
            //Arduino.Write(pressure_get.ToString());


        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            string fecha = DateTime.Now.ToString("dddd, MM/dd/yyyy");
            lblhora.Text = DateTime.Now.ToString("hh:mm:ss tt");
            lblfecha.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fecha);
        }

        private void lblfecha_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void IconClose_Click(object sender, EventArgs e)
        {
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

        private void iconTerminal_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Terminal);

            if (frm == null)
            {
                Terminal nt = new Terminal();
                nt.lblPuerto.Text = "Connected";
                nt.mvt = this;
                nt.Arduino = Arduino;
                nt.Show();
            }
            else {
                frm.BringToFront();
                return;
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

        private void label9_Click(object sender, EventArgs e)
        {

        }

        //Maugoncr//
        //Evento que nos permitira mover el form
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

        private void button10_Click(object sender, EventArgs e)
        {
            StartCrono();
            DisableBtn(btnPlay);
        }
        public void StartCrono() { 
            oSW.Start();
            timer2.Enabled = true;
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)oSW.ElapsedMilliseconds);
            txthoras.Text = ts.Hours.ToString().Length < 2 ? "0" + ts.Hours.ToString() : ts.Hours.ToString();
            txtminutos.Text = ts.Minutes.ToString().Length < 2 ? "0" + ts.Minutes.ToString() : ts.Minutes.ToString();
            txtsegundos.Text = ts.Seconds.ToString().Length<2 ? "0" + ts.Seconds.ToString() : ts.Seconds.ToString();
            txtmilisegundos.Text = ts.Milliseconds.ToString();

            lb_CounterTest.Text = TestCicles.counter.ToString();

            if (TestCicles.greenlight == true)
            {
                green_off.Image.Dispose();
                green_off.Image = MidoriValveTest.Properties.Resources.led_on_green;
                if (DateStartedTest.Text == "-/-/-")
                {
                    DateStartedTest.Text = DateTime.Now.ToString("MM/dd/yy      hh:mm:ss");
                }
            }
            else if (TestCicles.greenlight == false)
            {
                green_off.Image.Dispose();
                green_off.Image = MidoriValveTest.Properties.Resources.led_off_green;
            }

            if (TestCicles.yellowlight == true)
            {
                yellow_off.Image.Dispose();
                yellow_off.Image = MidoriValveTest.Properties.Resources.led_on_yellow;
                green_off.Image.Dispose();
                green_off.Image = MidoriValveTest.Properties.Resources.led_off_green;
                

            }
            else if (TestCicles.yellowlight == false)
            {
                yellow_off.Image.Dispose();
                yellow_off.Image = MidoriValveTest.Properties.Resources.led_off_yellow;

            }

        }

        public void Clear() {
            DateEndedTest.Text = "-/-/-";
            DateStartedTest.Text = "-/-/-";
            green_off.Image.Dispose();
            green_off.Image = MidoriValveTest.Properties.Resources.led_off_green;
            yellow_off.Image.Dispose();
            yellow_off.Image = MidoriValveTest.Properties.Resources.led_off_yellow;
            ResetCrono();
            lb_CounterTest.Text = "0";
            label11.Text = "Finished:";
        }

        public void resetCycles() {
            lb_CounterTest.Text = "0";

        }

        public void TestFinished() {
            StopCrono();
            DateEndedTest.Text = DateTime.Now.ToString("MM / dd / yy      hh: mm:ss");
            label11.Text = "Finished:";

        }

        private void button11_Click(object sender, EventArgs e)
        {
            ResetCrono();
            DisableBtn(btnStop);
        }

        public void ResetCrono() {

            oSW.Reset();
            txthoras.Text = "00";
            txtminutos.Text = "00";
            txtsegundos.Text = "00";
            txtmilisegundos.Text = "000";
            timer2.Enabled = false;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            StopCrono();
            DisableBtn(btn_Pause);
        }

        public void StopCrono() {
            oSW.Stop();
            DateEndedTest.Text = DateTime.Now.ToString("MM / dd / yy      hh: mm:ss");
            label11.Text = "Stopped:";
        }

        private void IconTrace_Click(object sender, EventArgs e)
        {

            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Trace_Log);

            if (frm == null)
            {
                Trace_Log nt = new Trace_Log();

                nt.Show();
            }
            else
            {
                frm.BringToFront();
                return;
            }

        }

        private void IconSensor_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Pressure_Sensor);

            if (frm == null)
            {
                Pressure_Sensor nt = new Pressure_Sensor();

                nt.Show();
            }
            else
            {
                frm.BringToFront();
                return;
            }
            
        }

        private void IconReport_Click(object sender, EventArgs e)
        {
            Form frm = Application.OpenForms.Cast<Form>().FirstOrDefault(x => x is Report);

            if (frm == null)
            {
                Report nt = new Report();
                nt.Show();

            }
            else
            {
                frm.BringToFront();
                return;
            }

        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            
            //Disable SideMenu since you connect again
            iconTerminal.Enabled = false;
            iconPID.Enabled = false;
            IconSensor.Enabled = false;
            IconTrace.Enabled = false;
            IconReport.Enabled = false;
            //button3.Enabled = false;
            DisableBtn(button3);
            // En este arreglo se almacena todos los puertos seriales "COM" registados por la computadora.
            //Boton 3 es el boton de Connect
            //button3.Enabled = false;
            DisableBtn(button3);
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

            //Restart the chart timer
            tiempo = 0;
            //Stop the Chart
            timer_Chart.Stop();
            comboBox1.Enabled = true;

            //Maugoncr//because if there is nothing connected and we use the Arduino object we would get an error because it would be null.
            //Close the port and wait for 2s
            if (Arduino != null)
            {
                //Arduino.Write("0");
                Arduino.Close();
                Thread.Sleep(2000);
            }

            if (lbl_estado.Text == "Open")
            {

                trackBar1A.Enabled = false;
                trackBar2A.Enabled = false;
                trackBar1A.Value = 0;
                precision_aperture = 0;
                Current_aperture.Text =  precision_aperture + "°";
                picture_frontal.Image.Dispose();
                picture_frontal.Image = MidoriValveTest.Properties.Resources._0_2;
                picture_plane.Image.Dispose();
                picture_plane.Image = MidoriValveTest.Properties.Resources._0_GRADOS2;
                precision_aperture = 0;
                lbl_estado.ForeColor = Color.Red;
                lbl_estado.Text = "Close";

                DisableBtn(btn_apagar);
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


            }

            if (DateStartedTest.Text != "-/-/-")
            {
                TestCicles.greenlight = false;
                TestCicles.counter = 0;
                DateStartedTest.Text = "-/-/-";
                DateEndedTest.Text = "-/-/-";
                green_off.Image.Dispose();
                green_off.Image = MidoriValveTest.Properties.Resources.led_off_green;
                yellow_off.Image.Dispose();
                yellow_off.Image = MidoriValveTest.Properties.Resources.led_off_yellow;
                red_off.Image.Dispose();
                red_off.Image = MidoriValveTest.Properties.Resources.led_off_red;
                lb_CounterTest.Text = "0";
            }

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

            //Maugoncr// Turn off the led and the same for labels, disable the button of Open Gate
            com_led.Image.Dispose();
            com_led.Image = MidoriValveTest.Properties.Resources.led_on_red;
            LblEstado.Text = "Disconnected *";
            lblPuerto.Text = "Disconnected *";
            //btn_encender.Enabled = false;
            DisableBtn(btn_encender);
            lbl_pressure.Text = " 0 ";
            //btn_valveTest.Enabled = false;
            DisableBtn(btn_valveTest);

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
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnNBlue2;

            }

        }

        private void LeftBtn(Button btn) {
            if (btn.Enabled == true)
            {
                btn.BackgroundImage.Dispose();
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnNBlue3;
            }
            else
            {
                btn.BackgroundImage.Dispose();
                btn.BackgroundImage = MidoriValveTest.Properties.Resources.btnDisa2;
            }
        }

        private void button3_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button3);
        }

        private void button3_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button3);
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
            EnterBtn(btn_encender);
        }

        private void btn_encender_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_encender);
        }

        private void btn_apagar_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_apagar);
        }

        private void btn_apagar_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_apagar);
        }

        private void btn_valveTest_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_valveTest);
        }

        private void btn_valveTest_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_valveTest);
        }

        private void btnInfo_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btnInfo);
        }

        private void btnInfo_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btnInfo);
        }

        private void btn_set_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_set);
        }

        private void btn_set_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_set);
        }

        private void btn_S_pressure_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(btn_S_pressure);
        }

        private void btn_S_pressure_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(btn_S_pressure);
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
            EnterBtn(button1);
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button1);
        }

        private void button2_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button2);
        }

        private void button2_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button2);
        }

        private void button4_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button4);
        }

        private void button4_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button4);
        }

        private void button7_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button7);
        }

        private void button7_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button7);
        }

        private void button6_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button6);
        }

        private void button6_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button6);
        }

        private void button5_MouseEnter(object sender, EventArgs e)
        {
            EnterBtn(button5);
        }

        private void button5_MouseLeave(object sender, EventArgs e)
        {
            LeftBtn(button5);
        }

        private void TimerForData_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Arduino != null)
                {
                    if (Arduino.IsOpen != false)
                    {
                        string Temperatura = null;
                        char[] delimitador = { '$' };
                        Temperatura = Arduino.ReadExisting();
                        Temperatura = Temperatura.Replace("\r", string.Empty);
                        Temperatura = Temperatura.Replace("\n", string.Empty);

                        string[] temps = Temperatura.Split(delimitador);

                        lb_Temperature.Text = temps[0] + " ° C";
                        // es muy posible que ponga también los $
                        //TimerForData.Interval = 5000; actualmente cada 5 s
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
           
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

        private void PanelNav_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }

        private void btn_90_BackColorChanged(object sender, EventArgs e)
        {

        }

        private void groupBox6_Enter(object sender, EventArgs e)
        {

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
            btn_set.Text = "Set Aperture in " + trackBar1A.Value + "°";
            //precision_aperture = trackBar1.Value;
        }


        private void trackBar2A_Scroll(object sender, EventArgs e)
        {
            EnableBtn(btn_S_pressure);
            float nivel = trackBar2A.Value;


            switch (lbl_P_unit_top.Text)
            {
                case "PSI":
                    btn_S_pressure.Text = "Set target pressure in " + nivel / 100;
                    break;
                case "ATM":
                    btn_S_pressure.Text = "Set target pressure in " + nivel / 1000;
                    break;
                case "mbar":
                    btn_S_pressure.Text = "Set target pressure in " + nivel;
                    break;
                case "Torr":
                    btn_S_pressure.Text = "Set target pressure in " + nivel;
                    break;
            }
        }

        
    }
}
