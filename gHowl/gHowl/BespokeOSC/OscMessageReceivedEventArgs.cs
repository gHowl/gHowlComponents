using System;
using System.Diagnostics;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Arguments for OscMessageReceived events.
	/// </summary>
	public class OscMessageReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the OscMessage received.
		/// </summary>
		public OscMessage Message
		{
			get
			{
				return mMessage;
			}
		}

		/// <summary>
		/// Creates a new instance of OscMessageReceivedEventArgs
		/// </summary>
		/// <param name="message">The OscMessage received.</param>
		public OscMessageReceivedEventArgs(OscMessage message)
		{
			Trace.Assert(message != null);

			mMessage = message;
		}

		private OscMessage mMessage;
	}
}
