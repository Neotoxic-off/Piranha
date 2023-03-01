using System;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Security.Policy;

namespace Piranha.ViewModels
{
    public class SocketViewModel: BaseViewModel
    {
        private LoggerViewModel Logger;
        private ClientWebSocket _clientWebSocket;
        public ClientWebSocket ClientWebSocket
        {
            get { return (_clientWebSocket); }
            set { SetProperty(ref _clientWebSocket, value); }
        }

        private Object _status = App.Current.Resources["Disconnected"];
        public Object Status
        {
            get { return (_status); }
            set { SetProperty(ref _status, value); }
        }

        private string _state = "disconnected";
        public string State
        {
            get { return (_state); }
            set { SetProperty(ref _state, value); }
        }

        private Object _color = App.Current.Resources["Red"];
        public Object Color
        {
            get { return (_color); }
            set { SetProperty(ref _color, value); }
        }

        private bool _connectButton = true;
        public bool ConnectButton
        {
            get { return (_connectButton); }
            set { SetProperty(ref _connectButton, value); }
        }

        private bool _sendButton = false;
        public bool SendButton
        {
            get { return (_sendButton); }
            set { SetProperty(ref _sendButton, value); }
        }

        private bool _disconnectButton = false;
        public bool DisconnectButton
        {
            get { return (_disconnectButton); }
            set { SetProperty(ref _disconnectButton, value); }
        }

        public SocketViewModel(LoggerViewModel logger)
        {
            Logger = logger;
        }

        private void Init()
        {
            if (ClientWebSocket == null)
                ClientWebSocket = new ClientWebSocket();
        }

        private void Clear()
        {
            ClientWebSocket.Dispose();
            ClientWebSocket = null;
        }

        public WebSocketState CheckState()
        {
            Dictionary<WebSocketState, (string, string)> bind = new Dictionary<WebSocketState, (string, string)>()
            {
                { WebSocketState.Open, ("Connected", "Green") },
                { WebSocketState.Closed, ("Disconnected", "Red") },
                { WebSocketState.CloseSent, ("Connecting", "Orane") },
                { WebSocketState.None, ("Disconnected", "Red") }
            };
            WebSocketState status = ClientWebSocket.State;

            UpdateStatus(bind[status].Item2, bind[status].Item1);

            return (status);
        }

        private void UpdateStatus(string color, string status)
        {
            Status = App.Current.Resources[status];
            Color = App.Current.Resources[color];
            State = status;
        }

        private bool ValidateUrl(string url)
        {
            if (url != null && url != string.Empty)
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute) == true || url.StartsWith("ws") == true)
                    return (true);
            }
            return (false);
        }

        public async void Connect(string url)
        {
            ConnectButton = false;

            if (ValidateUrl(url) == true)
            {
                Init();
                UpdateStatus("Orange", "Connecting");
                Logger.Record($"connecting socket to '{url}'");

                await ClientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);

                if (CheckState() != WebSocketState.Open)
                {
                    Logger.Record($"socket connection failed");
                    ConnectButton = true;
                    SendButton = false;
                } else
                {
                    Logger.Record($"socket connected");
                    DisconnectButton = true;
                    SendButton = true;
                    Read();
                }
            } else
            {
                Logger.Record($"invalid socket");
                ConnectButton = true;
            }
        }

        public async void Disconnect()
        {
            WebSocketState[] whitelist =
            {
                WebSocketState.Open
            };

            DisconnectButton = false;
            UpdateStatus("Orange", "Connecting");
            if (whitelist.Contains(CheckState()) == true)
            {
                await ClientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                if (whitelist.Contains(CheckState()) == true)
                {
                    DisconnectButton = true;
                    SendButton = true;
                    Logger.Record($"socket disconnection failed");
                } else
                {
                    Logger.Record($"socket disconnected");
                    UpdateStatus("Red", "Disconnected");
                    Clear();
                    ConnectButton = true;
                    SendButton = false;
                }
            } else
            {
                Logger.Record($"socket not connected");
            }
        }

        public async void Send(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            ArraySegment<byte> buff = new ArraySegment<byte>(bytes);

            await ClientWebSocket.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async void Read()
        {
            byte[] buffer = new byte[1056];
            ArraySegment<byte> buff = new ArraySegment<byte>(buffer);
            List<string> content = new List<string>();

            do
            {
                var result = await ClientWebSocket.ReceiveAsync(buff, CancellationToken.None);
                if (result.MessageType != WebSocketMessageType.Close)
                {
                    content.Add(Encoding.ASCII.GetString(buffer, 0, result.Count));
                }
            }
            while (ClientWebSocket.CloseStatus.HasValue == false);
        }
    }
}
