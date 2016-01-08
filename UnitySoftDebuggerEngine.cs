// 
// UnitySoftDebuggerEngine.cs
//   based on MoonlightSoftDebuggerEngine.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
//       Lucas Meijer <lucas@unity3d.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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


using System;
using System.Linq;
using MonoDevelop.Debugger;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Client;

namespace MonoDevelop.UnityDebug
{
	public class UnitySoftDebuggerEngine: DebuggerEngineBackend
	{
		UnitySoftDebuggerSession session;

		public override bool CanDebugCommand (ExecutionCommand cmd)
		{
			return cmd is UnityExecutionCommand;
		}

		public override bool IsDefaultDebugger (ExecutionCommand cmd)
		{
			return cmd is UnityExecutionCommand;
		}

		public override DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand command)
		{
			return null;
		}
			
		public override DebuggerSession CreateSession ()
		{
			session = new UnitySoftDebuggerSession ();
			session.Initialize ();

			session.TargetExited += delegate{ session = null; };
			return session;
		}
		
		public override ProcessInfo[] GetAttachableProcesses ()
		{
			return null;
		}

		public string Name {
			get {
				return "Mono Soft Debugger for Unity";
			}
		}
	}

	class UnityExecutionCommand : ExecutionCommand
	{
	};
}
