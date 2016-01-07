using System;

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
}

