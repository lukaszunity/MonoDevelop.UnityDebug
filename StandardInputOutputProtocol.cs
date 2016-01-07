using System;
using System.IO;
using System.Diagnostics;

namespace MonoDevelop.UnityDebug
{
	public class StandardInputOutputProtocol
	{
		Process process;
		string processPath;
		HttpHeaderReader lineReader = new HttpHeaderReader();

		public bool Start(string processPath)
		{
			if (!File.Exists (processPath)) 
			{
				return false;
			}

			this.processPath = processPath;

			var startInfo = new ProcessStartInfo(processPath);

			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.WorkingDirectory = Path.GetDirectoryName (processPath);
//			startInfo.Arguments = "";

			process = Process.Start (startInfo);

			var standardOutputThread = new System.Threading.Thread (() => ReadOutput (process.StandardOutput, "StandardOutput"));
			standardOutputThread.Start ();

//			var standardErrorThread = new System.Threading.Thread (() => ReadOutput (process.StandardError, "StandardError"));
//			standardErrorThread.Start ();

			return true;
		}

		public event HttpHeaderReader.ContentHandler OnStandardInputLine
		{
			add
			{
				lineReader.OnContent += value;
			}
			remove
			{
				lineReader.OnContent -= value;
			}
		}

		void ReadOutput (StreamReader reader, string name)
		{
			var buffer = new char[4096];

			while (!process.HasExited) 
			{
				int numBytes = reader.Read(buffer, 0, buffer.Length);
				var str = new String (buffer, 0, numBytes);
				lineReader.Read (str);
			}

			Console.WriteLine ( processPath + " process exited");
		}

		public void WriteStandardInput(string message)
		{
			process.StandardInput.Write (message);
			process.StandardInput.Flush ();
		}

	}
}

