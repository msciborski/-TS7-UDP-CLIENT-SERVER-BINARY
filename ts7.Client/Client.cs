using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ts7.Client {
    class Program{
        private const int listenPort = 6100;
        private const int timeSenderPort = 6101;
        private static UdpClient _udpClient;
        private static UdpClient _timeClient;
        static void Main(string[] args){
            bool registered = false;
            SetupClient();
            while (!registered){
                var msg = Console.ReadLine();
                byte[] bufferMsg = Encoding.ASCII.GetBytes(msg);
                _udpClient.Send(bufferMsg, bufferMsg.Length);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 6100);
                byte[] recvMsg = _udpClient.Receive(ref remoteEndPoint);
                string recMsgString = Encoding.ASCII.GetString(recvMsg);
                Console.WriteLine("Message from server: {0}", recMsgString);
                if (recMsgString.Equals("Registered")){
                    registered = true;
                }

            }
            //Thread threadTime = new Thread(DataINTime);
            //threadTime.Start();
            Thread communicationThread = new Thread(DataIN);
            communicationThread.Start();
            //remoEndPoint = (EndPoint) ep;
            //s.Bind(ep);
            //Thread thread = new Thread(DataIN);
            //thread.Start();
            //while (true){
            //    try{
            //        string input = Console.ReadLine();
            //        byte[] bytes = Encoding.ASCII.GetBytes(input);
            //        s.SendTo(bytes, ep);
            //    }
            //    catch (Exception e){
            //        Console.WriteLine(e.Message);
            //    }
            //}

            //IPAddress broadcast = IPAddress.Parse("127.0.0.1");
            while (true) {
                Console.Write("Podaj wiadomosc:");
                var msg = Console.ReadLine();
                byte[] bytes = Encoding.ASCII.GetBytes(msg);
                try{
                    _udpClient.Send(bytes, bytes.Length);
                }
                catch (Exception e){
                    Console.WriteLine("Sever disconnected!");
                }

            }
        }

        private static void SetupClient() {
            Console.WriteLine("Podaj adres ip:");
            var ipAddress = Console.ReadLine();
            IPAddress endpointIPAddress = IPAddress.Parse(ipAddress);

            _udpClient = new UdpClient();
            _udpClient.Connect(endpointIPAddress, listenPort);

            _timeClient = new UdpClient();
            _timeClient.Connect(IPAddress.Parse("192.168.0.2"), timeSenderPort);
        }

        //private static void DataINTime(){
        //    while (true){
        //        Console.WriteLine("Odbieram wiadomość...");
        //        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"),0);
        //        byte[] recBuff = _udpClient.Receive(ref remoteEndPoint);
        //        var recMsg = Encoding.ASCII.GetString(recBuff);
        //        Console.WriteLine("Czas z serwera: {0}", recMsg);
        //    }
        //}

        private static void DataIN(){
            while (true){
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6100);
                byte[] recBuff = _udpClient.Receive(ref remoteEndPoint);
                var recMsg = Encoding.ASCII.GetString(recBuff);
                Console.WriteLine("Wiadomość z serwera: ", recMsg);
            }
        }
    }
}
