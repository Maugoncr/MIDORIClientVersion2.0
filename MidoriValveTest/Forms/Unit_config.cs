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
    public partial class unit_form : Form
    {
        public unit_form()
        {
            InitializeComponent();
        }
        public Midori_PV ob = new Midori_PV();
        private void unit_form_Load(object sender, EventArgs e)
        {
            unit_scale.SelectedItem = Program.P_unit;
           
        }

        private void position_scale_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Save_Click(object sender, EventArgs e)
        {
            Program.P_unit = unit_scale.SelectedItem.ToString();
          

            ob.lbl_units_track.Text = Program.P_unit;
            ob.lbl_P_unit_top.Text = Program.P_unit;
            ob.lbl_presure_chart.Text = "[" + Program.P_unit + "]";

            switch(unit_scale.SelectedItem)
            {
                case "PSI":
                    ob.s_inicial =13.5555;
                    ob.s_final =14.6959;
                    ob.trackBar2A.Maximum = 146959;
                    ob.lbl_T_0.Text = "0";
                    ob.lbl_T_1.Text = "1.6328";
                    ob.lbl_T_2.Text = "3.2656";
                    ob.lbl_T_3.Text = "4.8984";
                    ob.lbl_T_4.Text = "6.5312";
                    ob.lbl_T_5.Text = "8.164";
                    ob.lbl_T_6.Text = "9.7968";
                    ob.lbl_T_7.Text = "11.4296";
                    ob.lbl_T_8.Text = "13.0624";
                    ob.lbl_T_9.Text = "14.6959";
                    break;
                case "ATM":
                    ob.s_inicial = 0.8895; //0.8895
                    ob.s_final = 1.0000;
                    ob.trackBar2A.Maximum = 1000;
                    ob.lbl_T_0.Text = "0";
                    ob.lbl_T_1.Text = "0.11";
                    ob.lbl_T_2.Text = "0.22";
                    ob.lbl_T_3.Text = "0.33";
                    ob.lbl_T_4.Text = "0.44";
                    ob.lbl_T_5.Text = "0.55";
                    ob.lbl_T_6.Text = "0.66";
                    ob.lbl_T_7.Text = "0.77";
                    ob.lbl_T_8.Text = "0.88";
                    ob.lbl_T_9.Text = "0.99";
                    break;
                case "mbar":
                    ob.s_inicial = 998.22;
                    ob.s_final = 1013.25;
                    ob.trackBar2A.Maximum = 101325;
                    ob.lbl_T_0.Text = "0";
                    ob.lbl_T_1.Text = "112.5833";
                    ob.lbl_T_2.Text = "225.1666";
                    ob.lbl_T_3.Text = "337.7499";
                    ob.lbl_T_4.Text = "450.3332";
                    ob.lbl_T_5.Text = "562.9165";
                    ob.lbl_T_6.Text = "675.4998";
                    ob.lbl_T_7.Text = "788.0831";
                    ob.lbl_T_8.Text = "900.6664";
                    ob.lbl_T_9.Text = "1013.25";
                    break;
                case "Torr":
                    ob.s_inicial = 755;
                    ob.s_final = 760;
                    ob.trackBar2A.Maximum = 760;
                    ob.lbl_T_0.Text = "0";
                    ob.lbl_T_1.Text = "84.44";
                    ob.lbl_T_2.Text = "168.88";
                    ob.lbl_T_3.Text = "253.32";
                    ob.lbl_T_4.Text = "337.76";
                    ob.lbl_T_5.Text = "422.2";
                    ob.lbl_T_6.Text = "506.64";
                    ob.lbl_T_7.Text = "591.08";
                    ob.lbl_T_8.Text = "675.52";
                    ob.lbl_T_9.Text = "760";
                    break;
            }

          

            foreach (var series in ob.chart1.Series)
            {
                series.Points.Clear();
            }

            this.Dispose();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void unit_scale_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Program.P_unit==unit_scale.SelectedItem.ToString())
            {
                Save.Enabled = false;
            }
            else
            {
                Save.Enabled = true;
            }
        }
    }
}
