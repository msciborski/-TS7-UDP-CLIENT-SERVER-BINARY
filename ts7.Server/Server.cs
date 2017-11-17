using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ts7.Server {
    class Server{
        private const int listenPort = 11000;
        private static UdpClient listener = new UdpClient(listenPort);
        private static Dictionary<IPEndPoint, string> _players = new Dictionary<IPEndPoint, string>();
        private static List<ClientData> _playersData;
        static IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

        private static void ListenForPlayers(){
            //while (_players.Count < 2){
                Console.WriteLine("Waiting for players.");
                byte[] bytes = listener.Receive(ref groupEP);
                IPEndPoint playerEndPoint = new IPEndPoint(groupEP.Address,groupEP.Port);
                string playerName = Encoding.ASCII.GetString(bytes);
                if (_players.ContainsKey(playerEndPoint)){
                    byte[] msg = Encoding.ASCII.GetBytes("Player exist.");
                    listener.Send(msg, msg.Length, playerEndPoint);
                }
                else{
                    byte[] msg = Encoding.ASCII.GetBytes(String.Format("Player {0} added;{1}", playerName,
                        playerEndPoint.ToString()));
                    listener.Send(msg, msg.Length, playerEndPoint);
                    _playersData.Add(new ClientData(playerEndPoint));
                    _players.Add(playerEndPoint,playerName);
                }

            //}
        }

        public static void DataIn(object ep){
            IPEndPoint endPoint = (IPEndPoint) ep;
            int readBytes;
            while (true){
                try{
                    byte[] buffer = listener.Receive(ref endPoint);
                    Console.WriteLine(Encoding.ASCII.GetString(buffer));
                    ProcessData();
                }
                catch (Exception e){
                    Console.WriteLine("Client disconnected.");
                }
            }
        }

        public static void ProcessData(){
            
        }

        private static void sendTimeInformationToEveryone(){
            while (true){
                var time = DateTime.Now.ToShortTimeString();
                foreach (var player in _players){
                    byte[] data = Encoding.ASCII.GetBytes(time);
                    listener.Send(data, data.Length, player.Key);
                }
                Thread.Sleep(2000);
            }
        }

        private static void StartGame(){
            Thread timeThread = new Thread(sendTimeInformationToEveryone);
            timeThread.Start();
        }
        static void Main(string[] args) {
            ListenForPlayers();
            StartGame();
        }

    }

    class ClientData{
        private Thread dataInClient;
        private IPEndPoint clientEndPoint;

        public ClientData(IPEndPoint ep){
            clientEndPoint = ep;
            dataInClient = new Thread(Server.DataIn);
            dataInClient.Start(clientEndPoint);
        }
    }
}
