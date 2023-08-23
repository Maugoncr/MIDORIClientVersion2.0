using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidoriValveTest
{
    public partial class Terminal : Form
    {
        public Terminal()
        {
            InitializeComponent();
        }

        public string command;
        public System.IO.Ports.SerialPort Arduino;
        public  Midori_PV mvt;
        private void textBox1_Enter(object sender, EventArgs e)
        {
          
        }

        private void txt_command_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                command = txt_command.Text;
                txt_command.Text = null;
                command_response();

            }
        }


        void command_response()
        {

            txt_response.Text = null;
            switch(command)
            {
               
                case "getPressure":
                   
                   // MessageBox.Show(mvt.pressure_get.ToString());
                    txt_response.Text = "Current presure: " + mvt.pressure_get.ToString();
                    RTXT_history.AppendText("Current presure: " + mvt.pressure_get.ToString() +", ["+ DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "]"+ "\r\n");
                    break;


                case "getCom":
                    txt_response.Text = "Current Aperture: " + Arduino.PortName;
                    RTXT_history.AppendText("Com port active: " + Arduino.PortName + ", [" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "]" + "\r\n");
                    break;


                case "getAperture":
                   // MessageBox.Show(mvt.precision_aperture.ToString());
                    txt_response.Text = "Current Aperture: " + mvt.precision_aperture.ToString();
                    RTXT_history.AppendText("Current Aperture: " + mvt.precision_aperture.ToString() + ", [" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "]" + "\r\n");
                    break;


                case "getSoftv":
                    txt_response.Text = "Software version: 1.3.2" ;
                    RTXT_history.AppendText("Software version: 1.3.2"+ ", [" + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt") + "]" + "\r\n");
                    break;
                case "setAperture":
                    break;

                 default:
                   
                    txt_response.Text = "Error: unidentified command";
                
                    break;
                    //GFBFGSRTHB
                  // HOLAAAA
               
            }
        }

        private void Terminal_Load(object sender, EventArgs e)
        {
            lblPuerto.Text = "Connected";
        }
    }
}
