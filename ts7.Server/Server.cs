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

namespace ts7.Server{
    class Server{
        private const int listenPort = 6100;
        private const int timeSenderPort = 6101;
        private const int playerLimit = 2;
        private static int time;
        private static bool gameRunning = false;
        private static UdpClient _listener;
        private static UdpClient _timeSender;
        private static IPEndPoint _ipEndPoint;
        private static IPEndPoint _ipEndPointTimeSender;
        private static List<PlayerData> _players;

        private static void Main(string[] args){
            SetupServer();
            RegisterUsers();
            StartGame();
            //while (true){
            //    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //    byte[] recvMsg = _listener.Receive(ref sender);
            //    Console.WriteLine("Message from client {0}: {1}", sender.ToString(), Encoding.ASCII.GetString(recvMsg));
            //    var msg = String.Format("Server time: {0}", DateTime.Now.ToShortTimeString());
            //    byte[] sendMsg = Encoding.ASCII.GetBytes(msg);
            //    _listener.Send(sendMsg, sendMsg.Length, sender);
            //}
        }

        private static void SetupServer(){
            _ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listenPort);
            _ipEndPointTimeSender = new IPEndPoint(IPAddress.Parse("127.0.0.1"), timeSenderPort);
            _listener = new UdpClient(_ipEndPoint);
            _timeSender = new UdpClient(_ipEndPointTimeSender);
            _players = new List<PlayerData>();
        }

        private static void RegisterUsers(){
            while (_players.Count < playerLimit){
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 6100);
                byte[] recMsg = _listener.Receive(ref sender);
                if (!_players.Exists(h => h.PlayerEndPoint.Equals(sender))){
                    _players.Add(new PlayerData(sender));
                    Console.WriteLine("Dodano gracza: {0}", sender.ToString());
                    var msgToSend = "Registered";
                    byte[] msgBuffor = Encoding.ASCII.GetBytes(msgToSend);
                    _listener.Send(msgBuffor, msgBuffor.Length, sender);
                }
                else{
                    Console.WriteLine("Player {0} istnieje.", sender.ToString());
                    var msgToSend = String.Format("Gracz {0} istnieje.", sender.ToString());
                    byte[] msgBuffor = Encoding.ASCII.GetBytes(msgToSend);
                    _listener.Send(msgBuffor, msgBuffor.Length, sender);
                }
            }
        }

        private static void StartGame(){
            while (_players.Count < playerLimit){
                
            }
            Thread thread = new Thread(SendTime);
            thread.Start();
        }

        public static void SendTime(){
            int t = 30;
            while (t >= 0){
                Console.WriteLine("Wysyłam wiadomosc");
                var msg = String.Format("Wiadomosc o czasie z servera: {0}", DateTime.Now.ToLongTimeString());
                byte[] msgBuff = Encoding.ASCII.GetBytes(msg);;
                foreach (var playerData in _players) {
                    Console.WriteLine("Wysyłam do: {0}:{1}",playerData.PlayerEndPoint.Address, playerData.PlayerEndPoint.Port);
                    //_timeSender.Send(msgBuff, msgBuff.Length, playerData.PlayerEndPoint);
                    _listener.Send(msgBuff, msgBuff.Length, playerData.PlayerEndPoint);
                }
                t--;
                Thread.Sleep(5000);
            }
        }

        public static void DataIN(object ep){
            IPEndPoint sender = (IPEndPoint) ep;
            try{
                byte[] recMessageBuff = _listener.Receive(ref sender);
                var recMessage = Encoding.ASCII.GetString(recMessageBuff);
                Console.WriteLine("Message from {0}: {1}", sender.ToString(), recMessage);
                ProcessData(recMessage, sender);
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
        }

        private static void StartClientThreads(){
            foreach (var playerData in _players){
                playerData.StartThread();
            }
        }

        private static void ProcessData(string msg, IPEndPoint ep){
        }

        class PlayerData{
            public IPEndPoint PlayerEndPoint{ get; set; }
            private Thread _playerThread;

            public PlayerData(IPEndPoint ep){
                PlayerEndPoint = ep;
                _playerThread = new Thread(Server.DataIN);
            }

            public void StartThread(){
                _playerThread.Start(PlayerEndPoint);
            }
        }
    }
}
