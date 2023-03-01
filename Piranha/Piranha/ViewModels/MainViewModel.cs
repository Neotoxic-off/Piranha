using Piranha.Models;
using Microsoft.Win32;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Piranha.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public SettingsViewModel Settings { get; set; }
        public LoggerViewModel Logger { get; set; }
        public ValidatorViewModel Validator { get; set; }
        public SocketViewModel Socket { get; set; }

        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand DisconnectCommand { get; set; }
        public DelegateCommand SendCommand { get; set; }
        public DelegateCommand GithubCommand { get; set; }
        public DelegateCommand DiscordCommand { get; set; }
        public DelegateCommand CodeCommand { get; set; }

        public MainViewModel()
        {
            Logger = new LoggerViewModel();

            LoadModels();
            LoadCommands();
            Load();
        }

        private void Load()
        {
            Logger.Record("loading client");

            Settings.LoadVersion();

            Logger.Record("client loaded");
        }

        private void LoadModels()
        {
            Logger.Record("loading client");

            Settings = new SettingsViewModel();
            Validator = new ValidatorViewModel();
            Socket = new SocketViewModel(Logger);

            Logger.Record("client loaded");
        }

        private void LoadCommands()
        {
            Logger.Record("loading commands");

            ConnectCommand = new DelegateCommand(Connect);
            DisconnectCommand = new DelegateCommand(Disconnect);
            SendCommand = new DelegateCommand(Send);

            GithubCommand = new DelegateCommand(Github);
            DiscordCommand = new DelegateCommand(Discord);
            CodeCommand = new DelegateCommand(Code);

            Logger.Record("commands loaded");
        }

        private void Connect(object data)
        {
            Socket.Connect(data.ToString());
        }

        private void Send(object data)
        {
            Socket.Send(data.ToString());
        }

        private void Disconnect(object data)
        {
            Socket.Disconnect();
        }

        private void Github(object data)
        {
            System.Diagnostics.Process.Start(Settings.Github);
        }

        private void Discord(object data)
        {
            System.Diagnostics.Process.Start(Settings.Discord);
        }

        private void Code(object data)
        {
            System.Diagnostics.Process.Start(Settings.Code);
        }

        private bool Valid(string name)
        {
            if (Validator.Empty(name) == false)
            {
                return (name.Length > 2);
            }

            return (false);
        }
    }
}
