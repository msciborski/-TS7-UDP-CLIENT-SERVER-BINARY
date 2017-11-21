using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ts7.Data;
using ts7.Packet;

namespace ts7.Client {
    class Program{
        private const int listenPort = 6100;
        private const int timeSenderPort = 4000;
        private static UdpClient _udpClient;
        private static UdpClient _timeClient;
        private static IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 6100);
        private static bool gameRunning = true;
        static void Main(string[] args){
            SetupClient();
            bool registered;
            do{
                Console.WriteLine("Wpisz register, aby się zarejestrować:");
                var msg = Console.ReadLine();
                if (msg.ToLower().Equals("register")){

                    Data.Packet packet = new Data.Packet(HelperData.RandomInt(0,255), 0, AnswerEnum.REQUEST, OperationEnum.REGISTER);
                    byte[] msgToSend = packet.Serialize();
                    _udpClient.Send(msgToSend, msgToSend.Length);

                    byte[] recvMessage = _udpClient.Receive(ref Program.remoteEndPoint);
                    Data.Packet recPacket = Data.Packet.Deserialize(recvMessage);
                    Console.WriteLine("Zarejestrowano, ID: {0}, data: {1}, answer: {2}, operation: {3}", recPacket.ID, recPacket.Data, recPacket.Answer, recPacket.Operation);
                    registered = true;

                }
                else{
                    Console.WriteLine("Niepoprawna komenda");
                    registered = false;
                }

            } while (!registered);
            Thread thread = new Thread(DataIN);
            thread.Start();
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
            IPAddress endpointIPAddress = IPAddress.Parse("127.0.0.1");
            _udpClient = new UdpClient();
            _udpClient.Connect(endpointIPAddress, listenPort);


            //_timeClient = new UdpClient(timeSenderPort);
            //_timeClient.Connect(endpointIPAddress, timeSenderPort);
            //_timeClient.Connect(endpointIPAddress, timeSenderPort);
        }

        private static void DataIN() {
            while (gameRunning){
                try{
                    byte[] recBuff = _udpClient.Receive(ref remoteEndPoint);
                    if (recBuff.Length > 0){
                        Data.Packet recvPacket = Data.Packet.Deserialize(recBuff);
                        Thread dataManagerThread = new Thread(DataManager);
                        dataManagerThread.Start(recvPacket);
                    }
                }
                catch (Exception e){
                    Console.WriteLine("Server disconnected");
                }
            }
        }

        private static void DataManager(object p){
            Data.Packet packet = (Data.Packet) p;

            if (packet.Operation == OperationEnum.START){
                Console.WriteLine("Gra wystartowała!");
                InputNumber(packet);
            }

            if (packet.Operation == OperationEnum.GUESS && packet.Answer == AnswerEnum.GUESSED){
                Console.WriteLine("Wygrałeś!");
                Console.WriteLine("ID: {0}, data: {1}, answer: {2}, operation: {3}", packet.ID, packet.Data, packet.Answer, packet.Operation);
                gameRunning = false;
                _udpClient.Close();
                Console.ReadLine();
            }
            if (packet.Operation == OperationEnum.GUESS && packet.Answer == AnswerEnum.NOT_GUESSED){
                Console.WriteLine("Nie zgadłeś!");
                InputNumber(packet);
            }
            if (packet.Operation == OperationEnum.SUMMARY){
                Console.WriteLine("Gra zakończona nie wygrałeś.");
                gameRunning = false;
                _udpClient.Close();
                Console.ReadLine();
            }
            if (packet.Operation == OperationEnum.TIME && packet.Answer == AnswerEnum.TIME_OUT){
                Console.WriteLine("ID: {0}, data: {1}, answer: {2}, operation: {3}", packet.ID, packet.Data, packet.Answer, packet.Operation);
                Console.WriteLine("GRA ZAKOŃCZONA, NIKT NIE ZGADNAL.");
                gameRunning = false;
                _udpClient.Close();
                Console.ReadLine();
            }
        }

        private static void InputNumber(Data.Packet packet){
            bool isNumber;
            do {
                if (gameRunning == false) {
                    break;
                }
                int number;
                Console.WriteLine("Podaj liczbe:");
                var msg = Console.ReadLine();
                if (int.TryParse(msg, out number)) {
                    Data.Packet packetToSend = new Data.Packet(packet.ID, number, AnswerEnum.NULL, OperationEnum.GUESS);
                    var bytesToSend = packetToSend.Serialize();
                    _udpClient.Send(bytesToSend, bytesToSend.Length);
                    isNumber = true;
                } else {
                    Console.WriteLine("Nie podałeś liczby");
                    isNumber = false;
                }

            } while (!isNumber && gameRunning);
        }
        //private static void DataIN() {
        //    while (true) {
        //        Console.WriteLine("Odbieram wiadomość...");
        //        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Broadcast, timeSenderPort);
        //        byte[] recBuff = _timeClient.Receive(ref remoteEndPoint);
        //        var recMsg = Encoding.ASCII.GetString(recBuff);
        //        Console.WriteLine("Czas z serwera: {0}", recMsg);
        //    }
        //}
    }
}
