using System;

namespace MonoDevelop.UnityDebug
{
	public class BadRequest : Exception
	{
		public BadRequest (string message) : base(message)
		{
		}
	}
}

