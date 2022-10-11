using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MidoriValveTest.Forms
{
    public partial class FrmAnalizarReadExist : Form
    {
        public FrmAnalizarReadExist()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            ObjetosGlobales.ReadExist = new List<string>();
        }

        private void FrmAnalizarReadExist_Load(object sender, EventArgs e)
        {
            LimpiarCargar();
        }


        public void LimpiarCargar() 
        {
            txtRead.Clear();

            txtRead.Text = String.Join(Environment.NewLine, ObjetosGlobales.ReadExist);

        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
