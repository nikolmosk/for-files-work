using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public delegate void deleg(object sender, IntensivityEventArgs e);
    public partial class Form2 : Form
    {
        public event deleg ev;
        int intens;
        public Form2()
        {
            InitializeComponent();
            intens = 5;
        }


        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            intens = trackBar1.Value;
        }
    }
    public class IntensivityEventArgs : EventArgs
    {
        public int inten { get; set; }
        public IntensivityEventArgs(int intens)
        { inten = intens;}
    }
}
