using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
namespace PCP.Univim
{
	internal class UnixSocketServer
	{
		private readonly string socketPath;
		private Socket mSocket;
		private UnixDomainSocketEndPoint mEndpoint;
		private Socket mHandler;
		private Thread socketThread;

		private readonly CommandDispatcher cmdDispatcher;

		public bool Enabled { get; set; } = true;

		public UnixSocketServer(CommandDispatcher _cmd, string _path)
		{
			cmdDispatcher = _cmd;
			socketPath = _path;
		}
		public void Start()
		{
			socketThread = new(StartServer);
			socketThread.Start();
		}
		public void Stop()
		{
			StopSever();
			socketThread.Abort();
		}
		private void StopSever()
		{
			mSocket?.Close(1000);
			DeleteSocketFile();
			Debug.Log("Socket closed");
		}

		private void StartServer()
		{
			mSocket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
			mEndpoint = new(socketPath);
			DeleteSocketFile();

			mSocket.Bind(mEndpoint);
			mSocket.Listen(5);
			Debug.Log("Server started waiting for client to connect...");

			//PERF:maybe i need a counter for that?but seems good from debugger
			while (true)
			{
				/* if (mSocket.Poll(0, SelectMode.SelectRead) && Enabled) */
				/* { */
				mHandler = mSocket.Accept();
				byte[] buffer = new byte[1024];
				int received = mHandler.Receive(buffer, 0, buffer.Length, SocketFlags.None);
				if (received > 0)
				{
					string message = Encoding.UTF8.GetString(buffer, 0, received);
					//FIX: async?
					CommandHandler(message);
				}
				/* } */
			}
		}
		private void CommandHandler(string msg)
		{
			Action _action = null;
			switch (msg)
			{
				case "quit":
					StopSever();
					break;
				case "play":
					_action = EditorApplication.EnterPlaymode;
					/* Debug.Log("Entering Playmode"); */
					break;
				case "stop":
					_action = EditorApplication.ExitPlaymode;
					/* Debug.Log("Exit Playmode"); */
					break;
				case "pause":
					_action = () => { EditorApplication.isPaused = !EditorApplication.isPaused; };
					/* Debug.Log("Pause Playmode"); */
					break;
				case "comp":
					_action = () => { UnivimMain.needUpdate = true; };
					break;
				default:
					Debug.Log($"Received:{msg}");
					break;
			}
			if (_action != null)
				cmdDispatcher.Enqueue(_action);
		}

		private void DeleteSocketFile()
		{
			if (System.IO.File.Exists(socketPath))
				System.IO.File.Delete(socketPath);
		}
	}
}
