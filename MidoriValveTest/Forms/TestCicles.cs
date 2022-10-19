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
    public partial class TestCicles : Form
    {
       public System.IO.Ports.SerialPort Arduino;
        public static int counter;
        public static int limit;
        public static bool greenlight = false;
        public static bool yellowlight = false;
        public Midori_PV menssager;
       

        public TestCicles()
        {
            InitializeComponent();
           

        }

        private void TestCicles_Load(object sender, EventArgs e)
        {
             btnTestStart.Enabled = false;
             button1.Enabled = false;
        }



private void timer1_Tick(object sender, EventArgs e)
        {
            if (counter < limit)
            {
                Arduino.Write("90");
                Arduino.Write("0");
                counter++;
                txt_cycles.Text = counter.ToString();

                //TODO: verificar con los sensores si todo esta correcto, se mantenga en verde, amarillo, rojo

            }
            if (counter == limit)
            {
                
                //menssager.TestFinished();
                btnTestStart.Enabled=false;
                NumOfCycles.Enabled = false;
                timer1.Stop();

            }


        }

        // Cycles Reset
        private void button1_Click(object sender, EventArgs e)
        {
            counter = 0;            
            txt_cycles.Text = "0";
           // menssager.resetCycles();
            button1.Enabled=false;

        }

        private void btnTestStart_Click(object sender, EventArgs e)
        {
           
                limit = (int)NumOfCycles.Value;
                timer1.Interval = 2500;
                timer1.Start();
                greenlight = true;
                yellowlight = false;
              //  menssager.StartCrono();
            

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            yellowlight = true;
          //  menssager.StopCrono();
            button1.Enabled = true;

        }

        private void btnForClear_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            greenlight = false;
            yellowlight=false;
            counter = 0;
            limit = 0;
            NumOfCycles.Value = 0;
            txt_cycles.Text = "0";
           // menssager.Clear();
            btnTestStart.Enabled = false;
            NumOfCycles.Enabled = true;

        }

        private void NumOfCycles_ValueChanged(object sender, EventArgs e)
        {

            if (NumOfCycles.Value > 0)
            {
                btnTestStart.Enabled = true;
            }
           


        }
    }


    
}
