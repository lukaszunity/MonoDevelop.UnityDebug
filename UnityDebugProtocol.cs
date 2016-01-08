using System;
using OpenDebug;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Mono.Debugging.Client;
using Newtonsoft.Json.Linq;

namespace MonoDevelop.UnityDebug
{
	public class UnityDebugProtocol
	{
		StandardInputOutputProtocol stdInOutProtocol = new StandardInputOutputProtocol();
		Dictionary<int, ManualResetEvent> requestEvents = new Dictionary<int, ManualResetEvent>();
		Exception requestException;

		Dictionary<string, List<int>> fileBreakpoints = new Dictionary<string, List<int>> ();

		public delegate void ThreadEventHandler(int threadId);
		public event ThreadEventHandler ThreadStarted;
		public event ThreadEventHandler ThreadExited;

		public delegate void BreakpointEventHandler(string path, int line);
		public event BreakpointEventHandler BreakpointHit;

		public void Initialize(string processPath)
		{
			if (!stdInOutProtocol.Start (processPath))
				throw new FileNotFoundException (processPath);

			stdInOutProtocol.OnStandardInputLine += HandleResult;

			SendRequest (new InitializeRequest ());
		}

		public void Attach(string target)
		{
			SendRequest (new LaunchRequest (target));
		}

		public void AddBreakpoint(string filePath, int line)
		{
			List<int> lines;

			if (!fileBreakpoints.ContainsKey (filePath)) {
				lines = new List<int> ();
				fileBreakpoints [filePath] = lines;
			} else
				lines = fileBreakpoints [filePath];

			if (!lines.Contains (line))
				lines.Add (line);

			SendRequest (new SetBreakpointsRequest (filePath, lines.ToArray()));
		}

		void SendRequest(Request request)
		{
			var requestEvent = new ManualResetEvent(false);
			requestEvents [request.seq] = requestEvent;

			var jsonString = JsonConvert.SerializeObject (request);
			var data = string.Format ("Content-Length: {0}\r\n\r\n{1}", jsonString.Length, jsonString);

			DebuggerLoggingService.LogMessage ("Request: {0}", jsonString);

			stdInOutProtocol.WriteStandardInput (data);

			// Wait for response
			requestEvent.WaitOne();

			if (requestException != null) 
			{
				var e = requestException;
				requestException = null;
				throw e;
			}	
		}

		void HandleResult(string result)
		{
			DebuggerLoggingService.LogMessage ("Result: {0}", result);

			var message = JsonConvert.DeserializeObject<V8Message> (result);

			if (message.type == "event")
				HandleEvent (JsonConvert.DeserializeObject<V8Event> (result));
			else if(message.type == "response")
				HandleReponse (JsonConvert.DeserializeObject<V8Response> (result));
		}

		void HandleEvent(V8Event @event)
		{
			var body = @event.body as JObject;

			switch (@event.eventType) 
			{
			case "initialized":
				break;
			case "output":
				break;
			case "thread":
				var threadEvent = body.ToObject<ThreadEvent> ();
				var threadId = threadEvent.threadId;

				if (threadEvent.reason == "started" && ThreadStarted != null)
					ThreadStarted (threadEvent.threadId);
				
				if (threadEvent.reason == "exited" && ThreadExited != null)
					ThreadExited (threadEvent.threadId);

			break;

			case "stopped":
				var stoppedEvent = body.ToObject<StoppedEvent> ();

				if (stoppedEvent.reason == "breakpoint")
				{
					if (BreakpointHit != null)
						BreakpointHit (stoppedEvent.source.path, stoppedEvent.line);
				}

				break;

			default:
				requestException = new BadRequest ("Unhandled event '" + @event.eventType + "'");
				break;
			}
		}

		void HandleReponse(V8Response response)
		{
			if (response.success == false)
				requestException = new BadRequest (response.message);

			// Signal that a response has been processsed
			var requestEvent = requestEvents [response.request_seq];
			requestEvents.Remove (response.request_seq);
			requestEvent.Set ();
		}
	}
}

