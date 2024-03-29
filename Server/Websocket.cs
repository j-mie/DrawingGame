﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using QuickDrawServer.Structs;
using SuperSocket.SocketBase;
using SuperWebSocket;

namespace QuickDrawServer
{
    internal static class Websocket
    {

        private static WebSocketServer _webSocketServer;
        private static Dictionary<WebSocketSession, Client> _clientList; 
        public static bool Start(int port)
        {
            _webSocketServer = new WebSocketServer();

            if (!_webSocketServer.Setup(port))
            {
                return false;
            }

            if (!_webSocketServer.Start())
            {
                return false;
            }

            _webSocketServer.NewSessionConnected += _webSocketServer_NewSessionConnected;
            _webSocketServer.SessionClosed += _webSocketServer_SessionClosed;
            _webSocketServer.NewMessageReceived += _webSocketServer_NewMessageReceived;
            _webSocketServer.NewDataReceived += _webSocketServer_NewDataReceived;

            _clientList = new Dictionary<WebSocketSession, Client>();

            return true;
        }

        public static void SendMessage(string msg, Client receiver)
        {
            receiver.WebSocketSession.Send(msg);
        }

        public static void SendData(byte[] data, Client receiver)
        {
            receiver.WebSocketSession.Send(data, 0, data.Length);
        }

        private static void _webSocketServer_NewDataReceived(WebSocketSession session, byte[] value)
        {
            throw new NotImplementedException();
        }

        private static void _webSocketServer_NewMessageReceived(WebSocketSession session, string value)
        {
            try
            {
                Console.WriteLine(value);
                dynamic msg = JsonConvert.DeserializeObject(value);
                Type header = msg.Header;
                Packet.PacketHandler(header, _clientList[session], msg);
            }
            catch (Exception)
            {
                Console.WriteLine("Malformed Packet");
            }
            
        }

        private static void _webSocketServer_SessionClosed(WebSocketSession session, CloseReason value)
        {
            Console.WriteLine(value);
            if (_clientList.ContainsKey(session))
            {
                //TODO do something with the CloseReason
                _clientList[session] = null;
                _clientList.Remove(session);
            }
            else
            {
                Console.WriteLine("Error: Client disconnected who wasn't even valided");
            }
            
        }

        private static void _webSocketServer_NewSessionConnected(WebSocketSession session)
        {
            Console.WriteLine("New connection from: {0}", session.RemoteEndPoint);
            _clientList.Add(session, new Client(session));
        }
    }
}