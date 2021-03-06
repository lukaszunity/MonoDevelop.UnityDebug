// 
// UnityProjectServiceExtension.cs 
//   
// Author:
//       Levi Bard <levi@unity3d.com>
// 
// Copyright (c) 2010 Unity Technologies
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// 

using System.Linq;
using System.Collections.Generic;

using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.Core.Execution;
using MonoDevelop.Core;
using MonoDevelop.Debugger;

namespace MonoDevelop.UnityDebug
{
	/// <summary>
	/// ProjectServiceExtension to allow Unity projects to be executed under the soft debugger
	/// </summary>
	public class UnityProjectServiceExtension : ProjectServiceExtension
	{
		internal static string EditLayout = "Solution";
		private DebuggerEngine unityDebuggerEngine = null;
		UnityExecutionCommand executionCommand = new UnityExecutionCommand();

		static  List<ExecutionTarget> executionTargets = new List<ExecutionTarget> ();

		DebuggerEngine UnityDebuggerEngine
		{
			get 
			{
				if (unityDebuggerEngine == null)
					unityDebuggerEngine = DebuggingService.GetDebuggerEngines ().FirstOrDefault (e => e.Id == "MonoDevelop.UnityDebug");

				return unityDebuggerEngine;
			}
		}

		static UnityProjectServiceExtension()
		{
			executionTargets.Add (new UnityExecutionTarget ("Unity Editor", "Unity.Instance", "Unity Editor", executionTargets.Count));

			if (Platform.IsMac) 
			{
				executionTargets.Add (new UnityExecutionTarget ("OSX Player", "Unity.Instance", "OSXPlayer", executionTargets.Count));
				executionTargets.Add (new UnityExecutionTarget ("OSX WebPlayer", "Unity.Instance", "OSXWebPlayer", executionTargets.Count));
			}

			if (Platform.IsWindows) 
			{
				executionTargets.Add (new UnityExecutionTarget ("Windows Player", "Unity.Instance", "WindowsPlayer", executionTargets.Count));
				executionTargets.Add (new UnityExecutionTarget ("Windows WebPlayer", "Unity.Instance", "WindowsWebPlayer", executionTargets.Count));
			}

			if (Platform.IsLinux) 
			{
				executionTargets.Add (new UnityExecutionTarget ("Linux Player", "Unity.Instance", "LinuxPlayer", executionTargets.Count));
				executionTargets.Add (new UnityExecutionTarget ("Linux WebPlayer", "Unity.Instance", "LinuxWebPlayer", executionTargets.Count));
			}

			executionTargets.Add (new UnityExecutionTarget ("iOS Player", "Unity.Instance", "iPhonePlayer", executionTargets.Count));

			executionTargets.Add (new UnityExecutionTarget ("Android Player", "Unity.Instance", "AndroidPlayer", executionTargets.Count));

			executionTargets.Add (new UnityExecutionTarget ("Attach To Process", "Unity.AttachToProcess", null, executionTargets.Count));
		}

		public UnityProjectServiceExtension()
		{
			MonoDevelop.Ide.IdeApp.FocusIn += delegate {
				if(UnityDebuggerEngine != null)
					UnityDebuggerEngine.GetAttachableProcesses();
			};
		}

		public static IEnumerable<ExecutionTarget> ExecutionTargets
		{
			get { return executionTargets; }
		}

		/// <summary>
		/// Detects whether any of the given projects reference UnityEngine
		/// </summary>
		private static bool ReferencesUnity (IEnumerable<Project> projects)
		{
			return null != projects.FirstOrDefault (project => 
				(project is DotNetProject) && 
				null != ((DotNetProject)project).References.FirstOrDefault (reference =>
				       reference.Reference.Contains ("UnityEngine")
				)
			);
		}
		
		#region ProjectServiceExtension overrides
		
		private bool CanExecuteProject (Project project) {
			return null != project &&  ReferencesUnity (new Project[]{ project });
		}
		
		/// <summary>
		/// Flags Unity projects for debugging with this addin
		/// </summary>
		protected override bool CanExecute (SolutionEntityItem item, ExecutionContext context, ConfigurationSelector configuration)
		{
			if (context.ExecutionHandler != null)
				context.ExecutionHandler.CanExecute (executionCommand);

			if (CanExecuteProject (item as Project))
				return true;

			return base.CanExecute (item, context, configuration);
		}

		private void ShowAttachToProcessDialog()
		{
			DispatchService.GuiDispatch (delegate {
				var dlg = new AttachToProcessDialog ();
				try {
					if (MessageService.RunCustomDialog (dlg) == (int) Gtk.ResponseType.Ok)
						IdeApp.ProjectOperations.AttachToProcess (dlg.SelectedDebugger, dlg.SelectedProcess);
				}
				finally {
					dlg.Destroy ();
				}
			});
		}
		
		/// <summary>
		/// Launch Unity project
		/// </summary>
		public override void Execute (IProgressMonitor monitor, IBuildTarget item, ExecutionContext context, ConfigurationSelector configuration)
		{
			var project = item as Project;
			var target = context.ExecutionTarget as UnityExecutionTarget;

			if (!CanExecuteProject (project) || target == null) 
			{
				base.Execute (monitor, item, context, configuration);
				return;
			}
			
			if (target.Id.StartsWith("Unity.Instance")) 
			{
				DispatchService.GuiDispatch (delegate {
					IdeApp.ProjectOperations.AttachToProcess (unityDebuggerEngine, new Mono.Debugging.Client.ProcessInfo(target.Index, target.Name)); 
				});
			} 
			else if (target.Id == "Unity.AttachToProcess") 
			{
				ShowAttachToProcessDialog ();
			} 
			else
			{
				MessageService.ShowError ("UnityProjectServiceExtension: Unsupported target.Id: " + target.Id);
				MonoDevelop.Core.LoggingService.LogError ("UnityProjectServiceExtension: Unsupported target.Id: " + target.Id);
				base.Execute (monitor, item, context, configuration);
			}
		}

		class UnityExecutionTarget : ExecutionTarget
		{
			string name;
			string id;
			string processName;
			int index;

			public UnityExecutionTarget(string name, string id, string processName, int index)
			{
				this.name = name;
				this.id = id + (processName == null ? "" : "." + processName);
				this.processName = processName;
				this.index = index;
			}

			public override string Name { get { return name; } }
			public override string Id { get { return id; } }
			public string ProcessName { get { return processName; } }
			public int Index { get { return index; } }
		}

		protected override IEnumerable<ExecutionTarget> GetExecutionTargets (SolutionEntityItem item, ConfigurationSelector configuration)
		{
			return ExecutionTargets;
		}
		
		public override bool GetNeedsBuilding (IBuildTarget item, ConfigurationSelector configuration)
		{
			if (item is WorkspaceItem){ return GetNeedsBuilding ((WorkspaceItem)item, configuration); }
			if (item is Project && ReferencesUnity (new Project[]{ (Project)item })) {
				return false;
			}
			return base.GetNeedsBuilding (item, configuration);
		}
		
		#endregion
	}
}

