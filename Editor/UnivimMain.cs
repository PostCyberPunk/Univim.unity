using System;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
namespace PCP.Univim
{

	[InitializeOnLoad]
	public static class UnivimMain
	{
		private static UnixSocketServer mSocketServer;
		private static CommandDispatcher mDispacher;
		private static readonly string socketPath = "/tmp/Univim";
		private static bool isEditorFocused = true;
		internal static bool needUpdate;

		public static bool Enabled { get; private set; } = true;
		static UnivimMain()
		{
			if (!SessionState.GetBool("DisableUnivim", false))
			{
				needUpdate = false;
				CompilationPipeline.compilationStarted += OnCompilationStarted;
				/* CompilationPipeline.compilationFinished += OnCompilationFinished; */
				EditorApplication.quitting += CloseSocketServer;
				EditorApplication.update += OnUpdate;
				Init();
			}
		}

		private static void Init()
		{
			if (mSocketServer != null)
				return;
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
			//Check focus
			if (InternalEditorUtility.isApplicationActive != isEditorFocused)
			{
				isEditorFocused = !isEditorFocused;
				if (isEditorFocused)
				{
					mSocketServer.Enabled = false;
					Debug.Log("Foreground!");
				}
				else if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
				{
					mSocketServer.Enabled = true;
					Debug.Log("Background!");
				}
			}
			if (!isEditorFocused && !EditorApplication.isCompiling && !EditorApplication.isUpdating && needUpdate)
			{
				AssetDatabase.Refresh();
			}
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
