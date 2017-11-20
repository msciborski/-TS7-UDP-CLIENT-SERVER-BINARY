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
        private const int timeSenderPort = 4000;
        private static UdpClient _udpClient;
        private static UdpClient _timeClient;
        static void Main(string[] args){
            SetupClient();

            //while (true){
                var msg = Console.ReadLine();
                byte[] bufferMsg = Encoding.ASCII.GetBytes(msg);
                _udpClient.Send(bufferMsg, bufferMsg.Length);
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 6100);
                byte[] recvMsg = _udpClient.Receive(ref remoteEndPoint);
                Console.WriteLine("Message from server: {0}", Encoding.ASCII.GetString(recvMsg));
            //}
            Thread thread = new Thread(DataIN);
            thread.Start();
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
            //while (true){
            //    var msg = Console.ReadLine();
            //    byte[] bytes = Encoding.ASCII.GetBytes(msg);
            //    IPEndPoint ep = new IPEndPoint(broadcast, 11000);
            //    EndPoint remoteEndPoint = (EndPoint) ep;
            //    s.SendTo(bytes, ep);
            //    Console.WriteLine("Message sent!");
            //    byte[] recvBytes = new byte[128];
            //    try{
            //        s.ReceiveFrom(recvBytes, SocketFlags.None, ref remoteEndPoint);
            //        Console.WriteLine(Encoding.ASCII.GetString(recvBytes));
            //    }
            //    catch (Exception e){
            //        Console.WriteLine("System disconnected.");
            //    }

            //}
        }

        private static void SetupClient() {
            Console.WriteLine("Podaj adres ip:");
            var ipAddress = Console.ReadLine();
            IPAddress endpointIPAddress = IPAddress.Parse(ipAddress);

            _udpClient = new UdpClient();
            _udpClient.Connect(endpointIPAddress, listenPort);
            _timeClient = new UdpClient(timeSenderPort);
            _timeClient.Connect(endpointIPAddress, timeSenderPort);



            //_timeClient.Connect(endpointIPAddress, timeSenderPort);
        }

        private static void DataIN(){
            while (true){
                Console.WriteLine("Odbieram wiadomość...");
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast,timeSenderPort);
                byte[] recBuff = _timeClient.Receive(ref remoteEndPoint);
                var recMsg = Encoding.ASCII.GetString(recBuff);
                Console.WriteLine("Czas z serwera: {0}", recMsg);
            }
        }
    }
}
