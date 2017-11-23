using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ts7.Data;
using ts7.Packet;

namespace ts7.Server {
    class Server {
        private const int listenPort = 6100;
        private const int timeSenderPort = 11000;
        private const int playerLimit = 1;
        private static int time;
        private static int tempTime = 0;
        private static bool gameRunning = false;
        private static int numberToGuess;
        private static UdpClient _listener;

        private static Timer timer;
        private static UdpClient _timeSender;

        private static IPEndPoint _ipEndPoint;
        private static IPEndPoint _ipEndPointTimeSender;

        private static Dictionary<IPEndPoint, PlayerData> _players;


        private static void Main(string[] args) {
            SetupServer();
            RegisterUsers();
            SendStartMessage();
            StartGame();
        }

        private static void SetupServer() {
            _ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            //_ipEndPointTimeSender = new IPEndPoint(IPAddress.Any, timeSenderPort);
            _listener = new UdpClient(_ipEndPoint);
            _timeSender = new UdpClient(timeSenderPort);
            _players = new Dictionary<IPEndPoint, PlayerData>();
        }

        private static void RegisterUsers() {
            while (_players.Count < playerLimit) {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] recvMsg = _listener.Receive(ref sender);
                Data.Packet packet = Data.Packet.Deserialize(recvMsg);
                Console.WriteLine("ID: {0}, data: {1}, answer: {2}, operation: {3}", packet.ID, packet.Data, packet.Answer, packet.Operation);

                ProcessData(packet, sender);
            }
        }

        private static void SendStartMessage() {
            foreach (var playerData in _players) {
                Data.Packet packet = new Data.Packet(playerData.Value.SessionID, 0, AnswerEnum.ACK, OperationEnum.START);
                byte[] bytesToSend = packet.Serialize();
                _listener.Send(bytesToSend, bytesToSend.Length, playerData.Value.PlayerEndPoint);
            }
        }

        private static void StartGame() {
            Console.WriteLine(CalculateTime());
            time = CalculateTime();
            //time = 3;
            gameRunning = true;
            numberToGuess = HelperData.RandomInt(0, 255);
            //numberToGuess = 10;
            Console.WriteLine("Number to guess: {0}", numberToGuess);
            StartClientThreads();
            timer = new Timer(SubstractTime, 5, 0, 1000);


        }

        private static int CalculateTime() {
            int sessionIDSum = 0;
            foreach (var playerData in _players) {
                sessionIDSum += playerData.Value.SessionID;
            }
            return ((sessionIDSum * 99) % 100) + 30;
        }

        private static void SubstractTime(object state) {
            if (time > 0) {
                Console.WriteLine(time); ;
                if (tempTime < 3) {
                    tempTime++;
                    Console.WriteLine(tempTime);
                }
                if (tempTime == 3) {
                    Thread thread = new Thread(SendTime);
                    thread.Start(time);
                    tempTime = 0;
                }
                time--;
            } else if (time == 0) {
                gameRunning = false;
                foreach (var playerData in _players) {
                    try {
                        Data.Packet packetToSend = new Data.Packet(playerData.Value.SessionID, 0, AnswerEnum.TIME_OUT,
                            OperationEnum.TIME);
                        byte[] bytesToSend = packetToSend.Serialize();
                        _listener.Send(bytesToSend, bytesToSend.Length, playerData.Value.PlayerEndPoint);
                    } catch (Exception e) {
                        Console.WriteLine("Client disconected: {0}", playerData.Value.SessionID);
                    }
                }
                _listener.Close();
                timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        private static void SendTime(object t) {
            int timeToSend = (int)t;
            foreach (var playerData in _players) {
                Console.WriteLine("Wysyłam czas do: {0}", playerData.Key.ToString());
                Data.Packet packet = new Data.Packet(playerData.Value.SessionID, timeToSend, AnswerEnum.NULL, OperationEnum.TIME);
                byte[] bytesToSend = packet.Serialize();
                _listener.Send(bytesToSend, bytesToSend.Length, playerData.Key);
            }
        }
        //public static void SendTime(){
        //    int t = 30;
        //    while (t >= 0){
        //        Console.WriteLine("Wysyłam wiadomosc");
        //        var msg = String.Format("Wiadomosc o czasie z servera: {0}", DateTime.Now.ToLongTimeString());
        //        byte[] msgBuff = Encoding.ASCII.GetBytes(msg);;
        //        _timeSender.Send(msgBuff, msgBuff.Length, new IPEndPoint(IPAddress.Parse("192.168.56.255"), timeSenderPort));
        //        t--;
        //        Thread.Sleep(1000);
        //    }
        //}

        public static void DataIN(object ep) {
            while (gameRunning) {
                IPEndPoint sender = (IPEndPoint)ep;
                try {
                    byte[] recByte = _listener.Receive(ref sender);
                    Data.Packet packet = Data.Packet.Deserialize(recByte);
                    Console.WriteLine("ID: {0}, data: {1}, answer: {2}, operation: {3}", packet.ID, packet.Data, packet.Answer, packet.Operation);

                    ProcessData(packet, sender);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void StartClientThreads() {
            foreach (var playerData in _players) {
                playerData.Value.StartThread();
            }
        }

        private static void ProcessData(object p, object ep) {
            Data.Packet packet = (Data.Packet)p;
            IPEndPoint endPoint = (IPEndPoint)ep;
            Console.WriteLine("ID: {0}, data: {1}, answer: {2}, operation: {3}", packet.ID, packet.Data, packet.Answer, packet.Operation);

            if (packet.Operation == OperationEnum.REGISTER && packet.Answer == AnswerEnum.REQUEST) {
                Register(packet, endPoint);
            }
            if (packet.Operation == OperationEnum.GUESS) {
                Guessing(packet, endPoint);
            }
        }



        private static void Register(Data.Packet packet, IPEndPoint endPoint) {
            if (!_players.ContainsKey(endPoint)) {
                _players.Add(endPoint, new PlayerData(endPoint, packet.ID));
                Data.Packet packetToSend = new Data.Packet(packet.ID, 0, AnswerEnum.ACK, OperationEnum.REGISTER);
                byte[] bytesToSend = packetToSend.Serialize();
                _listener.Send(bytesToSend, bytesToSend.Length, endPoint);
            }
        }

        private static void Guessing(Data.Packet packet, IPEndPoint endPoint) {
            if (packet.Data == numberToGuess) {
                Data.Packet packetToSend = new Data.Packet(packet.ID, 0, AnswerEnum.GUESSED, OperationEnum.GUESS);
                byte[] bytesToSend = packetToSend.Serialize();
                _listener.Send(bytesToSend, bytesToSend.Length, endPoint);
                gameRunning = false;
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                foreach (var playerData in _players) {
                    if (!playerData.Value.PlayerEndPoint.Equals(endPoint)) {
                        Data.Packet packetToSendForNotGuessed = new Data.Packet(playerData.Value.SessionID, 0,
                            AnswerEnum.NULL, OperationEnum.SUMMARY);
                        byte[] bytesToSendForNotGuessed = packetToSendForNotGuessed.Serialize();
                        _listener.Send(bytesToSendForNotGuessed, bytesToSendForNotGuessed.Length, playerData.Key);
                    }
                }
                Console.ReadLine();
                gameRunning = false;
                _listener.Close();
            } else {
                Data.Packet packetToSend = new Data.Packet(packet.ID, 0, AnswerEnum.NOT_GUESSED, OperationEnum.GUESS);
                byte[] bytesToSend = packetToSend.Serialize();
                _listener.Send(bytesToSend, bytesToSend.Length, endPoint);
            }
        }

        class PlayerData {
            public int SessionID { get; set; }
            public IPEndPoint PlayerEndPoint { get; set; }
            private Thread _playerThread;

            public PlayerData(IPEndPoint ep, int id) {
                PlayerEndPoint = ep;
                SessionID = id;
                _playerThread = new Thread(Server.DataIN);
            }

            public void StartThread() {
                _playerThread.Start(PlayerEndPoint);
            }
        }

        class ThreadObject {
            public IPEndPoint EndPoint { get; set; }
            public Data.Packet Packet { get; set; }

            public ThreadObject(IPEndPoint ep, Data.Packet p) {
                EndPoint = ep;
                Packet = p;
            }
        }
    }
}
