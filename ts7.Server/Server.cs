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
        private static UdpClient _listener;
        private static List<ClientData> _players = new List<ClientData>();
        private static Dictionary<IPEndPoint, string> _uniquePlayers = new Dictionary<IPEndPoint, string>();
        private static IPEndPoint ipEndPoint;

        private static void Main(string[] args){
            ipEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            _listener = new UdpClient(ipEndPoint);
            RegisterPlayers();
            //while (true){
            //    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            //    byte[] recvMsg = _listener.Receive(ref sender);
            //    Console.WriteLine("Message from client {0}: {1}", sender.ToString(), Encoding.ASCII.GetString(recvMsg));
            //    var msg = String.Format("Server time: {0}", DateTime.Now.ToShortTimeString());
            //    byte[] sendMsg = Encoding.ASCII.GetBytes(msg);
            //    _listener.Send(sendMsg, sendMsg.Length, sender);
            //}
        }

        public static void DataIN(object ep){
            IPEndPoint sender = (IPEndPoint) ep;
            while (true){
                try{
                    byte[] recBuffer = _listener.Receive(ref sender);
                    var recMsg = Encoding.ASCII.GetString(recBuffer);
                    DataManager(recMsg, sender);
                }
                catch (Exception e){
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static void RegisterPlayers(){
            while (_players.Count <= 2){
                IPEndPoint sender  = new IPEndPoint(IPAddress.Any, 0);
                byte[] recMsg = _listener.Receive(ref sender);
                if (!_players.Exists(h => h.ClientIPEndPoint.Equals(sender))){
                    _players.Add(new ClientData(sender));
                    Console.WriteLine("Dodano gracza {0}", sender.ToString());
                    var msgToSend = String.Format("Dodano cie, uzytkownik: {0}", sender.ToString());
                    _players.Add(new ClientData(sender));
                    byte[] msgBuffor = Encoding.ASCII.GetBytes(msgToSend);
                    _listener.Send(msgBuffor, msgBuffor.Length, sender);
                }
                else{
                    Console.WriteLine("Player istniej");
                    var msgToSend = String.Format("Istniejesz juz; {0}", sender.ToString());
                    byte[] msgBuffor = Encoding.ASCII.GetBytes(msgToSend);
                    _listener.Send(msgBuffor, msgBuffor.Length, sender);
                }
            }
        }

        private static void DataManager(string recMsg, IPEndPoint senderEndPoint){

            var msgToSend = DateTime.Now.ToLongTimeString();
            byte[] sendBuff = Encoding.ASCII.GetBytes(msgToSend);
            _listener.Send(sendBuff, sendBuff.Length, senderEndPoint);
        }

        class ClientData{
            private Thread clientThread;
            public IPEndPoint ClientIPEndPoint { get; private set; }

            public ClientData(IPEndPoint ep){
                ClientIPEndPoint = ep;
                //clientThread = new Thread(Server.DataIN);
                //clientThread.Start(ClientIPEndPoint);
            }

        }
        //    private const int listenPort = 11000;
        //    private static UdpClient listener = new UdpClient(listenPort);
        //    private static Dictionary<IPEndPoint, string> _players = new Dictionary<IPEndPoint, string>();
        //    private static List<ClientData> _playersData;
        //    static IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        //    private static void ListenForPlayers(){
        //        //while (_players.Count < 2){
        //            Console.WriteLine("Waiting for players.");
        //            byte[] bytes = listener.Receive(ref groupEP);
        //            IPEndPoint playerEndPoint = new IPEndPoint(groupEP.Address,groupEP.Port);
        //            string playerName = Encoding.ASCII.GetString(bytes);
        //            if (_players.ContainsKey(playerEndPoint)){
        //                byte[] msg = Encoding.ASCII.GetBytes("Player exist.");
        //                listener.Send(msg, msg.Length, playerEndPoint);
        //            }
        //            else{
        //                byte[] msg = Encoding.ASCII.GetBytes(String.Format("Player {0} added;{1}", playerName,
        //                    playerEndPoint.ToString()));
        //                listener.Send(msg, msg.Length, playerEndPoint);
        //                _playersData.Add(new ClientData(playerEndPoint));
        //                _players.Add(playerEndPoint,playerName);
        //            }

        //        //}
        //    }

        //    public static void DataIn(object ep){
        //        IPEndPoint endPoint = (IPEndPoint) ep;
        //        int readBytes;
        //        while (true){
        //            try{
        //                byte[] buffer = listener.Receive(ref endPoint);
        //                Console.WriteLine(Encoding.ASCII.GetString(buffer));
        //                ProcessData();
        //            }
        //            catch (Exception e){
        //                Console.WriteLine("Client disconnected.");
        //            }
        //        }
        //    }

        //    public static void ProcessData(){

        //    }

        //    private static void sendTimeInformationToEveryone(){
        //        while (true){
        //            var time = DateTime.Now.ToShortTimeString();
        //            foreach (var player in _players){
        //                byte[] data = Encoding.ASCII.GetBytes(time);
        //                listener.Send(data, data.Length, player.Key);
        //            }
        //            Thread.Sleep(2000);
        //        }
        //    }

        //    private static void StartGame(){
        //        Thread timeThread = new Thread(sendTimeInformationToEveryone);
        //        timeThread.Start();
        //    }
        //    static void Main(string[] args) {
        //        ListenForPlayers();
        //        //StartGame();
        //    }

        //}

        //class ClientData{
        //    private Thread dataInClient;
        //    private IPEndPoint clientEndPoint;

        //    public ClientData(IPEndPoint ep){
        //        clientEndPoint = ep;
        //        dataInClient = new Thread(Server.DataIn);
        //        dataInClient.Start(clientEndPoint);
        //    }
        //}
    }
}
