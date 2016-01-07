using System;
using System.Text.RegularExpressions;

namespace MonoDevelop.UnityDebug
{
	public class HttpHeaderReader
	{
		protected static readonly Regex ContentLengthMatcher = new Regex("Content-Length: (\\d+)");
		public delegate void ContentHandler(string line);
		event ContentHandler contentEvent;
		string data;

		public event ContentHandler OnContent
		{
			add
			{
				contentEvent += value;
			}
			remove
			{
				contentEvent -= value;
			}
		}

		public void Read(string str)
		{
			data += str;

			var match = ContentLengthMatcher.Match (data);

			if (match.Success) 
			{
				int contentLength = Int32.Parse(match.Groups [1].Value);
				int requestIndex = data.IndexOf ("\r\n\r\n") + 4;

				if (data.Length >= requestIndex + contentLength) 
				{
					var content = data.Substring (requestIndex, contentLength);
					data = data.Substring (requestIndex + contentLength);

					if (contentEvent != null)
						contentEvent (content);
				}
			}
		}
	}

}

