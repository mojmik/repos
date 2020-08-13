using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Trida1 mTrida;


        public Form1()
        {
            this.mTrida = new Trida1();
            InitializeComponent();
            
        }

        private void label1_Click(object sender, EventArgs e)
        {
            mTrida.Add();
            this.label1.Text = "cau " + mTrida.getCount();
        }

       
    }
}
