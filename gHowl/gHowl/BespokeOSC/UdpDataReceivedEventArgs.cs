using System;
using System.Net;

namespace Bespoke.Common.Net
{
	/// <summary>
	/// 
	/// </summary>
	public class UdpDataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public IPEndPoint SourceEndPoint
		{
			get
			{
				return mSourceEndPoint;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public byte[] Data
		{
			get
			{
				return mData;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceEndPoint"></param>
		/// <param name="data"></param>
		public UdpDataReceivedEventArgs(IPEndPoint sourceEndPoint, byte[] data)
		{
			mSourceEndPoint = sourceEndPoint;
			mData = data;
		}

		private IPEndPoint mSourceEndPoint;
		private byte[] mData;
	}
}
