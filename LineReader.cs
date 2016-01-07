using System;

namespace MonoDevelop.UnityDebug
{
	public class LineReader
	{
		public delegate void LineHandler(string line);
		event LineHandler lineEvent;
		string data;

		public event LineHandler OnLine
		{
			add
			{
				lineEvent += value;
			}
			remove
			{
				lineEvent -= value;
			}
		}

		public void Read(string str)
		{
			data += str;

			int newlineIndex = IndexOfNewline (data);

			while (newlineIndex >= 0) 
			{
				var line = data.Substring (0, newlineIndex);

				data = data.Substring (newlineIndex + NewLineLength(data, newlineIndex));

				if(lineEvent != null)
					lineEvent (line);
	
				newlineIndex = IndexOfNewline (data);
			}
		}

		static int IndexOfNewline(string str)
		{
			return Math.Min (str.IndexOf ("\r\n"), str.IndexOf ("\n"));
		}

		static int NewLineLength(string str, int index)
		{
			if (str [index] == '\n')
				return 1;

			if (str [index] == '\r' && str [index + 1] == '\n')
				return 2;

			throw new NotSupportedException ("Newline character '" + str [index] + "' not supported");
		}


	}

}

