using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using SharpPcap;
using NATUPNPLib;
using NETCONLib;
using NetFwTypeLib;
using System.Threading;
using System.IO;

namespace basicKnock_servicio
{
    public partial class basicKnock : ServiceBase
    {
        static string dir = Environment.GetEnvironmentVariable("ProgramFiles")+"\\fe80Grau\\basicKnock Server";
        string REGLA_FIREWALL = "";
        string fileName64 = dir+"\\BasicKnockPR.exe";
        string fileName32 = dir+"\\BasicKnockPR.exe";
        string fileName = "";

        Process process;
        ProcessStartInfo psi;
        Thread t, tt;
        public basicKnock()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("basicKnock")) 
                System.Diagnostics.EventLog.CreateEventSource("basicKnock", "");
            eventLog1.Source = "basicKnock";
            eventLog1.Log = "";
        }
        protected override void OnStart(string[] args)
        {
            t = new Thread(iniciarProceso);
            t.Start();
            tt = new Thread(comprobarProceso);
            tt.Start();
        }
        public void iniciarProceso()
        {
            try
            {
                if (System.IO.File.Exists(fileName32)) fileName = fileName32;
                else fileName = fileName64;
                psi = new ProcessStartInfo(fileName)
                {
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                process = Process.Start(psi);
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Excepción: " + e.ToString()+ " "+ dir);
            }
        }

        public void comprobarProceso()
        {
            while (true)
            {
                Thread.Sleep(5000);
                int cont = 0;
                Process[] procesos = Process.GetProcesses();
                foreach (Process ps in procesos)
                {
                    if (ps.ProcessName == "BasicKnockPR") cont++;
                }
                if (cont == 0) iniciarProceso();
            }
        }
        protected override void OnStop()
        {
            t.Abort();
            tt.Abort();
            config_Lectura();
            INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            INetFwRule firewallIPs = firewallPolicy.Rules.Item(REGLA_FIREWALL);
            firewallIPs.RemoteAddresses = "127.0.0.1";
            process.Kill();

        }
        public int config_Lectura()
        {
            int result = 0;
            string txt = String.Empty;
            string line = String.Empty;
            string name = dir+"\\basicKnock.conf";
            string name32 = dir+"\\basicKnock.conf";
            StreamReader file = new StreamReader(name);

            if (!File.Exists(name)) name = name32;
            eventLog1.WriteEntry("Fichero configuración: "+name);

            while ((line = file.ReadLine()) != null) txt += line;
            file.Close();

            if (txt != null)
            {
                String[] config = new String[5];
                String[] cnf = txt.Split(';');
                for (int i = 0; i < 5; i++)
                {
                    config[i] = cnf[i].ToString().Split('=')[1];
                }
                REGLA_FIREWALL = config[1].Replace(" ", "");
                result = 1;
            }
            eventLog1.WriteEntry("Regla Firewall: " + REGLA_FIREWALL);
            return result;
        }
    }
}
