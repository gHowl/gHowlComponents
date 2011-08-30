using System;
using System.Collections.Generic;
using System.Text;

namespace Bespoke.Common.Net
{
	/// <summary>
	/// 
	/// </summary>
	public enum TransmissionType
	{
		/// <summary>
		/// Transmit to subscribed clients only. Includes heartbeat exchanges.
		/// </summary>
		Unicast,

		/// <summary>
		/// Transmit via UDP multicast. No heartbeat exchanges.
		/// </summary>
		Multicast,

		/// <summary>
		/// Transmit via UDP broadcast. No heartbeat exchanges.
		/// </summary>
		Broadcast,

		/// <summary>
		/// Local unicast without subcription or heartbeat exchanges.
		/// </summary>
		LocalBroadcast
	}
}
