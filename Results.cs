/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace OpenDebug
{
	public class V8Message
	{
		public int seq;
		public string type;

		public V8Message(string typ) {
			type = typ;
		}
	}

	public class V8Request : V8Message
	{
		public string command;
		public dynamic arguments;

		public V8Request(int id, string cmd, dynamic arg) : base("request") {
			seq = id;
			command = cmd;
			arguments = arg;
		}
	}

	public class V8Response0 : V8Message
	{
		public bool success;
		public string message;
		public int request_seq;
		public string command;
		public dynamic body;

		public V8Response0() : base("response") {
		}

		public V8Response0(int rseq, string cmd) : base("response") {
			request_seq = rseq;
			command = cmd;
		}
	}

	public class V8Response : V8Response0
	{
		public bool running;
		public dynamic refs;

		public V8Response() {
		}

		public V8Response(string msg) {
			success = false;
			message = msg;
		}

		public V8Response(bool succ, string msg) {
			success = succ;
			message = msg;
		}

		public V8Response(dynamic m) {
			seq = m.seq;
			success = m.success;
			message = m.message;
			request_seq = m.request_seq;
			command = m.command;
			body = m.body;
			running = m.running;
			refs = m.refs;
		}
	}

	public class V8Event : V8Message
	{
		[JsonProperty(PropertyName = "event")]
		public string eventType;
		public dynamic body;

		public V8Event() : base("event") {
		}

		public V8Event(dynamic m) : base("event") {
			seq = m.seq;
			eventType = m["event"];
			body = m.body;
		}

		public V8Event(string type, dynamic bdy = null) : base("event") {
			eventType = type;
			body = bdy;
		}
	}

	/*
	 * This monomorphic class is used to return results from a debugger request or to return errors.
	 * In addition events can be attached that are fired after the request results have been returned to the caller.
	 */
	public sealed class DebugResult
	{
		public bool Success { get; private set; } // boolean indicating success
		public ResponseBody Body { get; private set; }	// depending on value of success either the result or an error
		public List<DebugEvent> Events { get; private set; } // send this event after returning the result

		/*
		 * A success result without additional data.
		 */
		public DebugResult() {
			Success = true;
		}

		/*
		 * A success result with as additional event.
		 */
		public DebugResult(DebugEvent ev) {
			Success = true;
			Add(ev);
		}

		/*
		 * A result with a response body. If body is a ErrorResponseBody then Success will be set to false.
		 */
		public DebugResult(ResponseBody body) {
			Success = true;
			Body = body;
			if (body is ErrorResponseBody) {
				Success = false;
			}
		}

		/*
		 * A failure result with a full error message.
		 */
		public DebugResult(int id, string format, dynamic arguments = null) {
			Success = false;
			Body = new ErrorResponseBody(new Message(id, format, arguments));
		}

		/*
		 * Add a DebugEvent to this request result.
		 * Events are fired after the result is returned to the caller of the request.
		 */
		public void Add(DebugEvent ev) {
			if (ev != null) {
				if (Events == null) {
					Events = new List<DebugEvent>();
				}
				Events.Add(ev);
			}
		}
	}

	/*
	 * subclasses of ResponseBody are serialized as the response body.
	 * Don't change their instance variables since that will break the OpenDebug protocol.
	 */
	public class ResponseBody {
		// empty
	}

	public class ErrorResponseBody : ResponseBody {

		public Message error { get; private set; }

		public ErrorResponseBody(Message m) {
			error = m;
		}
	}

	public class StackTraceResponseBody : ResponseBody
	{
		public StackFrame[] stackFrames { get; private set; }

		public StackTraceResponseBody(List<StackFrame> frames = null) {
			if (frames == null)
				stackFrames = new StackFrame[0];
			else
				stackFrames = frames.ToArray<StackFrame>();
		}
	}

	public class ScopesResponseBody : ResponseBody
	{
		public Scope[] scopes { get; private set; }

		public ScopesResponseBody(List<Scope> scps = null) {
			if (scps == null)
				scopes = new Scope[0];
			else
				scopes = scps.ToArray<Scope>();
		}
	}

	public class VariablesResponseBody : ResponseBody
	{
		public Variable[] variables { get; private set; }

		public VariablesResponseBody(List<Variable> vars = null) {
			if (vars == null)
				variables = new Variable[0];
			else
				variables = vars.ToArray<Variable>();
		}
	}

	public class SourceResponseBody : ResponseBody
	{
		public string content { get; private set; }

		public SourceResponseBody(string cont) {
			content = cont;
		}
	}

	public class ThreadsResponseBody : ResponseBody
	{
		public Thread[] threads { get; private set; }

		public ThreadsResponseBody(List<Thread> vars = null) {
			if (vars == null)
				threads = new Thread[0];
			else
				threads = vars.ToArray<Thread>();
		}
	}

	public class EvaluateResponseBody : ResponseBody
	{
		public string result { get; private set; }
		public int variablesReference { get; private set; }

		public EvaluateResponseBody(string value, int reff = 0) {
			result = value;
			variablesReference = reff;
		}
	}

	public class SetBreakpointsResponseBody : ResponseBody
	{
		public Breakpoint[] breakpoints { get; private set; }

		public SetBreakpointsResponseBody(List<Breakpoint> bpts = null) {
			if (bpts == null)
				breakpoints = new Breakpoint[0];
			else
				breakpoints = bpts.ToArray<Breakpoint>();
		}
	}
}
