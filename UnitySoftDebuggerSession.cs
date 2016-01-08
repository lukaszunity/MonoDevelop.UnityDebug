using System;
using Mono.Debugging.Client;
using System.IO;
using System.Reflection;
using System.Linq;

namespace MonoDevelop.UnityDebug
{
	/// <summary>
	/// Debugger session for Unity scripting code
	/// </summary>
	public class UnitySoftDebuggerSession : DebuggerSession
	{
		UnityDebugProtocol unityDebugProtocol = new UnityDebugProtocol ();

		public void Initialize()
		{
			var assemblyDirectory = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);
			var unityDebugPath = Path.Combine (assemblyDirectory, "UnityDebug", "UnityDebug.exe");
			unityDebugProtocol.Initialize (unityDebugPath);
		}

		protected override void OnRun (DebuggerStartInfo startInfo)
		{
			throw new NotImplementedException ();
		}

		protected override void OnAttachToProcess (long processIndex)
		{
			var targets = UnityProjectServiceExtension.ExecutionTargets.ToArray ();
			var processTarget = targets [processIndex];
			unityDebugProtocol.Attach (processTarget.Name);
		}

		protected override void OnDetach ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnSetActiveThread (long processId, long threadId)
		{
			throw new NotImplementedException ();
		}

		protected override void OnStop ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnExit ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnStepLine ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnNextLine ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnStepInstruction ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnNextInstruction ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnFinish ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnContinue ()
		{
			throw new NotImplementedException ();
		}

		protected override BreakEventInfo OnInsertBreakEvent (BreakEvent breakEvent)
		{
			throw new NotImplementedException ();
		}

		protected override void OnRemoveBreakEvent (BreakEventInfo eventInfo)
		{
			throw new NotImplementedException ();
		}

		protected override void OnUpdateBreakEvent (BreakEventInfo eventInfo)
		{
			throw new NotImplementedException ();
		}

		protected override void OnEnableBreakEvent (BreakEventInfo eventInfo, bool enable)
		{
			throw new NotImplementedException ();
		}

		protected override ThreadInfo[] OnGetThreads (long processId)
		{
			throw new NotImplementedException ();
		}

		protected override ProcessInfo[] OnGetProcesses ()
		{
			throw new NotImplementedException ();
		}

		protected override Backtrace OnGetThreadBacktrace (long processId, long threadId)
		{
			throw new NotImplementedException ();
		}
	}
}
