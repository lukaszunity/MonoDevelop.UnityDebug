using System;
using OpenDebug;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace MonoDevelop.UnityDebug
{
	public class UnityDebugProtocol
	{
		StandardInputOutputProtocol stdInOutProtocol = new StandardInputOutputProtocol();
		Dictionary<int, ManualResetEvent> requestEvents = new Dictionary<int, ManualResetEvent>();
		Exception requestException;

		Dictionary<string, List<int>> fileBreakpoints = new Dictionary<string, List<int>> ();

		public void Initialize(string processPath)
		{
			if (!stdInOutProtocol.Start (processPath))
				throw new FileNotFoundException (processPath);

			stdInOutProtocol.OnStandardInputLine += HandleResponse;

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

		void HandleResponse(string response)
		{
			var message = JsonConvert.DeserializeObject<V8Message> (response);

			if (message.type == "event")
				HandleEvent (JsonConvert.DeserializeObject<V8Event> (response));
			else if(message.type == "response")
				HandleReponse (JsonConvert.DeserializeObject<V8Response> (response));
		}

		void HandleEvent(V8Event @event)
		{
			switch (@event.eventType) 
			{
			case "initialized":
				break;
			case "output":
				break;
			case "thread":
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

