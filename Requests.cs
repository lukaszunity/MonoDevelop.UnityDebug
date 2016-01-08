﻿using System;

namespace OpenDebug
{
	public class Request
	{
		public int seq;
		public string type;
		public string command;

		[NonSerialized]
		public static int seqCounter = 0;

		public Request(string command)
		{
			seq = ++seqCounter;
			type = "request";
			this.command = command;
		}
	}

	public class InitializeRequest : Request
	{
		public InitializeRequestArguments arguments = new InitializeRequestArguments();

		public InitializeRequest() : base("initialize")
		{
		}
	}

	public class InitializeRequestArguments
	{
		public string adapterID;
		public bool linesStartAt1;
		public string pathFormat;

		public InitializeRequestArguments()
		{
			adapterID = "unity";
			linesStartAt1 = true;
			pathFormat = "path";
		}
	}

	public class LaunchRequest : Request
	{
		public LaunchRequestArguments arguments;

		public LaunchRequest(string name) : base("launch")
		{
			arguments = new LaunchRequestArguments (name);
		}
	}

	public class LaunchRequestArguments
	{
		public string name;
		public string type;
		public string request;

		public LaunchRequestArguments(string name)
		{
			this.name = name;
			type = "unity";
			request = "attach";
		}
	}
}

