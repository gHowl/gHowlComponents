using System;

namespace Bespoke.Common.Net
{
	/// <summary>
	/// 
	/// </summary>
	public class TcpDataReceivedEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public TcpConnection Connection
		{
			get
			{
				return mConnection;
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
		/// <param name="connection"></param>
		public TcpDataReceivedEventArgs(TcpConnection connection, byte[] data)
		{
			mConnection = connection;
            mData = data;
		}

		private TcpConnection mConnection;
        private byte[] mData;
	}
}
