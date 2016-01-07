using System;
using OpenDebug;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

namespace MonoDevelop.UnityDebug
{
	public class UnityDebugProtocol
	{
		StandardInputOutputProtocol stdInOutProtocol = new StandardInputOutputProtocol();
		Dictionary<int, Request> queuedRequests = new Dictionary<int, Request>();
		ManualResetEvent initializeEvent = new ManualResetEvent(false);
		bool initialized = false;

		public bool Initialize(string processPath)
		{
			if (!stdInOutProtocol.Start (processPath))
				return false;

			stdInOutProtocol.OnStandardInputLine += HandleResponse;

			SendRequest (new InitializeRequest ());
			initializeEvent.WaitOne ();

			return initialized;
		}

		void SendRequest(Request request)
		{
			queuedRequests [request.seq] = request;
			var jsonString = JsonConvert.SerializeObject (request);
			var data = string.Format ("Content-Length: {0}\r\n\r\n{1}", jsonString.Length, jsonString);
			stdInOutProtocol.WriteStandardInput (data);
		}

		void HandleResponse(string response)
		{
			var message = JsonConvert.DeserializeObject<V8Message> (response);

			if (message.type == "event")
				HandleEvent (JsonConvert.DeserializeObject<V8Event> (response));
			else if(message.type == "request")
				HandleReponse (JsonConvert.DeserializeObject<V8Response> (response));
		}

		void HandleEvent(V8Event @event)
		{
			switch (@event.eventType) 
			{
				case "initialized":
					initializeEvent.Set ();
				break;
			}

		}

		void HandleReponse(V8Response request)
		{

		}
	}
}

