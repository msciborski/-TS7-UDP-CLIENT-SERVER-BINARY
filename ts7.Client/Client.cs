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
        private static Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private static IPEndPoint ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 11000);
        private static EndPoint remoEndPoint;

        static void Main(string[] args){
            remoEndPoint = (EndPoint) ep;
            s.Bind(remoEndPoint);
            Thread thread = new Thread(DataIN);
            thread.Start();
            while (true){
                try{
                    string input = Console.ReadLine();
                    byte[] bytes = Encoding.ASCII.GetBytes(input);
                    s.SendTo(bytes, ep);
                }
                catch (Exception e){
                    Console.WriteLine(e.Message);
                }
            }

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

        private static void DataIN(){
            byte[] recvBytes = new byte[128];

            while (true){
                try{
                    s.ReceiveFrom(recvBytes, SocketFlags.None, ref remoEndPoint);
                    Console.WriteLine("Message from server: {0}", Encoding.ASCII.GetString(recvBytes));
                }
                catch (SocketException e){
                    Console.WriteLine("Server closed!");
                }
            }
        }
    }
}
