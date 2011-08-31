using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Represents a bundle of OSCMessage objects.
	/// </summary>
	public sealed class OscBundle : OscPacket
	{
		/// <summary>
		/// Specifies if the packet is an OSC bundle.
		/// </summary>
		public override bool IsBundle
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the creation time of the bundle.
		/// </summary>
		public long TimeStamp
		{
			get
			{
				return mTimeStamp;
			}
		}

		/// <summary>
		/// Creates a new instance of OscBundle. 
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		public OscBundle(IPEndPoint sourceEndPoint)
			: this(sourceEndPoint, 0)
		{
		}

		/// <summary>
		/// Creates a new instance of OscBundle.
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="timeStamp">The creation time of the bundle.</param>
		public OscBundle(IPEndPoint sourceEndPoint, long timeStamp)
			: base(sourceEndPoint, BundlePrefix)
		{
			mTimeStamp = timeStamp;
		}

		/// <summary>
		/// Serialize the packet.
		/// </summary>
		/// <returns>The newly serialized packet.</returns>
		public override byte[] ToByteArray()
		{
			List<byte> data = new List<byte>();

			data.AddRange(OscPacket.ValueToByteArray(mAddress));
			OscPacket.PadNull(data);

			data.AddRange(OscPacket.ValueToByteArray(mTimeStamp));

			foreach (object value in mData)
			{
				if ((value is OscPacket) && (value is OscBundle == false))
				{
					byte[] packetBytes = ((OscPacket)value).ToByteArray();
					data.AddRange(OscPacket.ValueToByteArray(packetBytes.Length));
					data.AddRange(packetBytes);					
				}
			}

			return data.ToArray();
		}

		/// <summary>
		/// Deserialize the packet.
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="data">The serialized packet.</param>
		/// <param name="start">The starting index into the serialized data stream.</param>
		/// <param name="end">The ending index into the serialized data stream.</param>
		/// <returns>The newly deserialized packet.</returns>
		public static new OscBundle FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start, int end)
		{
			string address = OscPacket.ValueFromByteArray<string>(data, ref start);
			Trace.Assert(address == BundlePrefix);

			long timeStamp = OscPacket.ValueFromByteArray<long>(data, ref start);
			OscBundle bundle = new OscBundle(sourceEndPoint, timeStamp);

			while (start < end)
			{
				int length = OscPacket.ValueFromByteArray<int>(data, ref start);
				int packetEnd = start + length;
				bundle.Append(OscPacket.FromByteArray(sourceEndPoint, data, ref start, packetEnd));
			}

			return bundle;
		}

		/// <summary>
		/// Appends a value to the packet.
		/// </summary>
		/// <typeparam name="T">The type of object being appended.</typeparam>
		/// <param name="value">The value to append.</param>
		/// <remarks>The value must be of type OscMessage.</remarks>
		public override int Append<T>(T value)
		{
			Trace.Assert(value is OscMessage);

			mData.Add(value);

			return mData.Count - 1;
		}

		private const string BundlePrefix = "#bundle";

		private long mTimeStamp;
	}
}
