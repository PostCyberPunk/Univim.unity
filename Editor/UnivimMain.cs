using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
namespace PCP.Univim
{

	[InitializeOnLoad]
	public static class UnivimMain
	{
		private static UnixSocketServer mSocketServer;
		private static CommandDispatcher mDispacher;
		private static readonly string socketPath = "/tmp/Univim";

		public static bool Enabled { get; private set; } = true;
		static UnivimMain()
		{
			if (!SessionState.GetBool("DisableUnivim", false))
			{
				CompilationPipeline.compilationStarted += OnCompilationStarted;
				/* CompilationPipeline.compilationFinished += OnCompilationFinished; */
				EditorApplication.quitting += CloseSocketServer;
				EditorApplication.update += OnUpdate;
				Init();
			}
		}

		private static void Init()
		{
			if (mSocketServer != null) return;
			mDispacher ??= new();
			try
			{
				mSocketServer = new(mDispacher, socketPath);
				mSocketServer.Start();
			}
			catch (Exception e)
			{
				Debug.LogError("Univim server started failed:" + e);
			}

		}
		private static void CloseSocketServer() => mSocketServer?.Stop();

		private static void OnUpdate()
		{
			if (!Enabled) return;
			mDispacher.Update();
		}
		[MenuItem("Tools/Univim/Toggle Univim")]
		public static void ToggleAutoCompilation()
		{
			bool toggle = SessionState.GetBool("DisableUnivim", false);
			Enabled = !toggle;
			SessionState.SetBool("DisableUnivim", Enabled);
			Debug.Log("Univim is " + (Enabled ? "Off" : "On"));
		}
		private static void OnCompilationStarted(object _) => CloseSocketServer();
		/* private static void OnCompilationFinished(object _) { if (!isEditorFocused) mSocketServer.Enabled = true; } */
	}
}
