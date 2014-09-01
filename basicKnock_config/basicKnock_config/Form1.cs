using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using System.IO;
using System.Diagnostics;

namespace basicKnock_config
{
    public partial class Form1 : Form
    {
        static string dir = Environment.GetEnvironmentVariable("ProgramFiles") + "\\fe80Grau\\basicKnock Server";
        static string IP_LOCAL = null;
        static string REGLA_FIREWALL = null;
        static int PUERTO_BLOQUEO = 0;
        static int PUERTO_PERMISO = 0;
        static int ID_TARJETA_RED = 0;
        static string PUERTOS;
        static string[] PUERTOS_PERMISO;
        static string estado = "Detenido";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("basicKnock_trafico.exe");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            config_Lectura();
            textBox1.Text = IP_LOCAL;
            textBox2.Text = REGLA_FIREWALL;
            textBox3.Text = PUERTOS;
            textBox4.Text = PUERTO_BLOQUEO.ToString();
            textBox5.Text = ID_TARJETA_RED.ToString();
        }

        static int config_Lectura()
        {
            int result = 0;
            string line;
            StreamReader file = new StreamReader(dir+"\\basicKnock.conf");
            if (!File.Exists(dir + "\\basicKnock.conf")) file = new StreamReader(dir + "\\basicKnock.conf");
            string txt = null;

            while ((line = file.ReadLine()) != null) txt += line;
            file.Close();

            if (txt != null)
            {
                String[] config = new String[5];
                String[] cnf = txt.Split(';');
                for (int i = 0; i < 5; i++) config[i] = cnf[i].ToString().Split('=')[1];

                IP_LOCAL = config[4].Replace(" ", "");
                REGLA_FIREWALL = config[1].Replace(" ", "");
                PUERTO_BLOQUEO = Convert.ToInt16(config[2]);
                PUERTOS = config[3].Replace(" ","");
                PUERTOS_PERMISO = config[3].Split(',');
                PUERTO_PERMISO = Convert.ToInt16(Convert.ToInt16(PUERTOS_PERMISO[0]));
                ID_TARJETA_RED = Convert.ToInt16(config[0]);
                result = 1;
            }

            return result;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("basicKnock");
            estado = sc.Status.ToString();
            button3.Enabled = true;
            button4.Enabled = true;
            if (estado != "Stopped")
            {
                button3.Text = "Detener servicio";
                label1.ForeColor = Color.Black;
                label1.BackColor = Color.GreenYellow;
            }
            else
            {
                button3.Text = "Iniciar servicio";
                label1.ForeColor = Color.White;
                label1.BackColor = Color.OrangeRed;
            }
            label1.Text = "Estado del servicio: "+estado;
        }

        private string construir_config()
        {
            string configura="";
            configura =
                "[ID_TARJETA_RED] = " + textBox5.Text + ";" + String.Empty +
                "[REGLA_FIREWALL] = " + textBox2.Text + ";" + String.Empty +
                "[PUERTO_BLOQUEO] = " + textBox4.Text + ";" + String.Empty +
                "[PUERTO_PERMISO] = " + textBox3.Text + ";" + String.Empty +
                "[IP_LOCAL] = " + textBox1.Text + ";";
            return configura;
        }

        private void guardar_config(string config)
        {
            StreamWriter file = new StreamWriter(dir + "\\basicKnock.conf");
            if (!File.Exists(dir + "\\basicKnock.conf")) file = new StreamWriter(dir + "\\basicKnock.conf");
            try
            {
                file.Write(config);
                MessageBox.Show("Se ha guardado la configuración, reinicia el servicio para aplicar los cambios.");
            }catch(Exception e){
                MessageBox.Show("Se ha detectado un error producido por la siguiente excepción: "+e.ToString());
            }
            file.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            guardar_config(construir_config());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("basicKnock");
            Button bt = sender as Button;
            if (bt.Text == "Iniciar servicio")
            {
                sc.Start();
            }
            else
            {
                sc.Stop();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ServiceController sc = new ServiceController("basicKnock");
            if (sc.Status.ToString() != "Stopped")
            {
                sc.Stop();
                MessageBox.Show("Se ha reiniciado el servicio");
                sc.Start();
            }
            else
            {
                sc.Start();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            ProcessStartInfo info = new ProcessStartInfo("CMD.EXE", "/C eventvwr %computername% /l:\"%SystemRoot%\\System32\\Winevt\\Logs\\Application.evtx\"");
            info.Verb = "open";
            Process.Start(info);
        }
    }
}
