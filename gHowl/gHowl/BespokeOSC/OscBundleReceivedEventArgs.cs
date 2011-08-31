using System;
using System.Diagnostics;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Arguments for OscBundleReceived events.
	/// </summary>
	public class OscBundleReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// Gets the OscBundle received.
		/// </summary>
		public OscBundle Bundle
		{
			get
			{
				return mBundle;
			}
		}

		/// <summary>
		/// Creates a new instance of OscBundleReceivedEventArgs
		/// </summary>
		/// <param name="bundle">The OscBundle received.</param>
		public OscBundleReceivedEventArgs(OscBundle bundle)
		{
			Trace.Assert(bundle != null);

			mBundle = bundle;
		}

		private OscBundle mBundle;
	}
}
