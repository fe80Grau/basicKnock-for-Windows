using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;
using NATUPNPLib;
using NETCONLib;
using NetFwTypeLib;

namespace basicKnock_trafico
{
    class Program
    {
        static string dir = Environment.GetEnvironmentVariable("ProgramFiles") + "\\fe80Grau\\basicKnock Server";
        static int PUERTO_BLOQUEO = 0;
        static int PUERTO_PERMISO = 0;
        static int ID_TARJETA_RED = 0;

        static void Main(string[] args)
        {

            if (config_Lectura() == 1)
            {
                /* Lista de dispositivos */
                var devices = CaptureDeviceList.Instance;

                /* Si no existen dispositivos devuelve error y sale de la aplicación*/
                if (devices.Count < 1)
                {
                    return;
                }


                int i = 0;
                foreach (var dev in devices)
                {
                    i++;
                }

                var device = devices[ID_TARJETA_RED];

                //Declaramos el hilo PacketArrivalEventHandler que actuará junto a la función device_OnPacketArrival local dentro de la propiedad OnPacketArrival del objeto device seleccionado
                device.OnPacketArrival +=
                    new PacketArrivalEventHandler(device_OnPacketArrival);

                // Ejecutamos la función Open del objeto device para empezar la configuración de la escucha
                int readTimeoutMilliseconds = 1000;
                device.Open(DeviceMode.Promiscuous, readTimeoutMilliseconds); //definimos que el dispositivo entre en modo promiscuo y que lea cada segundo

                //Filtro tcpdump para escuchar solo los paquetes TCP/IP 
                string filter = "tcp";
                device.Filter = filter;


                // Empezar la caputa de paquetes
                device.Capture();
                Console.WriteLine();
                device.Close();
            }
            else
            {
                return;
            }
        }

        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            var time = e.Packet.Timeval.Date;
            var len = e.Packet.Data.Length;

            var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            var tcpPacket = PacketDotNet.TcpPacket.GetEncapsulated(packet);
            if (tcpPacket != null)
            {
                var ipPacket = (PacketDotNet.IpPacket)tcpPacket.ParentPacket;
                System.Net.IPAddress srcIp = ipPacket.SourceAddress;
                System.Net.IPAddress dstIp = ipPacket.DestinationAddress;
                int srcPort = tcpPacket.SourcePort;
                int dstPort = tcpPacket.DestinationPort;
                string prefix = "";
                if (dstPort == PUERTO_PERMISO || dstPort == PUERTO_BLOQUEO)
                {
                    prefix = "Knock de ";
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    prefix = "Paquete de ";
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                Console.WriteLine(prefix + srcIp.ToString() + " " + time.Hour + ":" + time.Minute + ":" + time.Second + " Tamaño=" + len + " " + srcIp + ":" + srcPort + " -> " + dstIp + ":" + dstPort);
                if (prefix == "Knock de ")
                {
                    Console.WriteLine("Presione una tecla para seguir...");
                    Console.ReadKey();
                }
            }
        }

        static public int config_Lectura()
        {
            int result = 0;
            string line;
            string name = dir + "\\basicKnock.conf";
            string name32 = dir + "\\basicKnock.conf";
            if (!System.IO.File.Exists(name)) name = name32;
            System.IO.StreamReader file = new System.IO.StreamReader(name);

            string txt = null;
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
                PUERTO_BLOQUEO = Convert.ToInt16(config[2]);
                PUERTO_PERMISO = Convert.ToInt16(config[3].Split(',')[0]);
                ID_TARJETA_RED = Convert.ToInt16(config[0]);
                result = 1;
            }

            return result;
        }
    }
}
