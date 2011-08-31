using System;
using System.Diagnostics;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Arguments for OscPacketReceived events.
	/// </summary>
	public class OscPacketReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the OscPacket received.
		/// </summary>
		public OscPacket Packet
		{
			get
			{
				return mPacket;
			}
		}

		/// <summary>
		/// Creates a new instance of OscPacketReceivedEventArgs
		/// </summary>
		/// <param name="packet">The OscPacket received.</param>
		public OscPacketReceivedEventArgs(OscPacket packet)
		{
			Trace.Assert(packet != null);

			mPacket = packet;
		}

		private OscPacket mPacket;
	}
}
