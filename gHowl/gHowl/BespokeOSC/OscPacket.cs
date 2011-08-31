using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Represents the base unit of transmission for the Open Sound Control protocol.
	/// </summary>
	public abstract class OscPacket
	{
		#region Properties

		/// <summary>
		/// Specifies if the packet is an OSC bundle.
		/// </summary>
		public abstract bool IsBundle { get; }

		/// <summary>
		/// Gets the origin of the packet.
		/// </summary>
		public IPEndPoint SourceEndPoint
		{
			get
			{
				return mSourceEndPoint;
			}
		}

		/// <summary>
		/// Gets the OSC address pattern.
		/// </summary>
		public string Address
		{
			get
			{
				return mAddress;
			}
			set
			{
				Trace.Assert(string.IsNullOrEmpty(value) == false);
				mAddress = value;
			}
		}

		/// <summary>
		/// Gets the contents of the packet.
		/// </summary>
		public object[] Data
		{
			get
			{
				return mData.ToArray();
			}
		}

        /// <summary>
        /// Gets or sets the destination of sent packets when using TransportType.Tcp.
        /// </summary>
        public OscClient Client
        {
            get
            {
                return mClient;
            }
            set
            {
                mClient = value;
            }
        }

		#endregion

		/// <summary>
		/// Static constructor.
		/// </summary>
		static OscPacket()
		{
			sUdpClient = new UdpClient();
		}

		/// <summary>
		/// Creates a new instance of OscPacket.
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="address">The OSC address pattern.</param>
		public OscPacket(IPEndPoint sourceEndPoint, string address)
            : this(sourceEndPoint, address, null)
		{			
		}

        /// <summary>
        /// Creates a new instance of OscPacket.
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="address">The OSC address pattern.</param>
        /// <param name="client">The destination of sent packets when using TransportType.Tcp.</param>
        public OscPacket(IPEndPoint sourceEndPoint, string address, OscClient client)
        {
            Trace.Assert(string.IsNullOrEmpty(address) == false);

            mSourceEndPoint = sourceEndPoint;
            mAddress = address;
            mData = new List<object>();
            mClient = client;
        }

		/// <summary>
		/// Appends a value to the packet.
		/// </summary>
		/// <typeparam name="T">The type of object being appended.</typeparam>
		/// <param name="value">The value to append.</param>
		/// <returns>The index of the newly appended value.</returns>
		public abstract int Append<T>(T value);

		/// <summary>
		/// Return a entry in the packet.
		/// </summary>
		/// <typeparam name="T">The type of value expected at index.</typeparam>
		/// <param name="index">The index within the data array.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if specified index is out of range.</exception>
		/// <exception cref="InvalidCastException">Thrown if the specified T is incompatible with the data at index.</exception>
		/// <returns>The entry at the specified index.</returns>
		public T At<T>(int index)
		{
			if (index > mData.Count || index < 0)
			{
				throw new IndexOutOfRangeException();
			}

			if ((mData[index] is T) == false)
			{
				throw new InvalidCastException();
			}

			return (T)mData[index];
		}

		/// <summary>
		/// Serialize the packet.
		/// </summary>
		/// <returns>The newly serialized packet.</returns>
		public abstract byte[] ToByteArray();

		/// <summary>
		/// Deserialize the packet.
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="data">The serialized packet.</param>
		/// <returns>The newly deserialized packet.</returns>
		public static OscPacket FromByteArray(IPEndPoint sourceEndPoint, byte[] data)
		{
			Trace.Assert(data != null);

			int start = 0;
			return FromByteArray(sourceEndPoint, data, ref start, data.Length);
		}

		/// <summary>
		/// Deserialize the packet.
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="data">The serialized packet.</param>
		/// <param name="start">The starting index into the serialized data stream.</param>
		/// <param name="end">The ending index into the serialized data stream.</param>
		/// <returns>The newly deserialized packet.</returns>
		public static OscPacket FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start, int end)
		{
			if (data[start] == '#')
			{
				return OscBundle.FromByteArray(sourceEndPoint, data, ref start, end);
			}
			else
			{
				return OscMessage.FromByteArray(sourceEndPoint, data, ref start);
			}
		}

		/// <summary>
		/// Transmit an OSC packet.
		/// </summary>
		/// <param name="packet">The packet to transmit.</param>
		/// <param name="destination">The packet's destination.</param>
		public static void Send(OscPacket packet, IPEndPoint destination)
		{
			packet.Send(destination);
		}

        /// <summary>
        /// Transmit an OSC packet via TCP through the connected OscClient.
        /// </summary>
        /// <param name="packet">The packet to transmit.</param>
        /// <param name="client">The OscClient to communicate through.</param>
        /// <remarks>The OscClient must be connected for successful transmission.</remarks>
        public static void Send(OscPacket packet, OscClient client)
        {
            client.Send(packet);
        }

		/// <summary>
		/// Transmit an OSC packet via UDP.
		/// </summary>
		/// <param name="destination">The packet's destination.</param>
		public void Send(IPEndPoint destination)
		{
			byte[] data = ToByteArray();
			sUdpClient.Send(data, data.Length, destination);
		}

        /// <summary>
        /// Transmit an OSC packet via TCP through the connected OscClient.
        /// </summary>
        /// <remarks>The OscClient must be connected for successful transmission.</remarks>
        public void Send()
        {
            Assert.ParamIsNotNull(mClient);

            mClient.Send(this);
        }

		/// <summary>
		/// Deserialize a value.
		/// </summary>
		/// <typeparam name="T">The value's data type.</typeparam>
		/// <param name="data">The serialized data source.</param>
		/// <param name="start">The starting index into the serialized data stream.</param>
		/// <returns>The newly deserialized value.</returns>
		public static T ValueFromByteArray<T>(byte[] data, ref int start)
		{
			Type type = typeof(T);
			object value;

			if (type.Name == "String")
			{
				int count = 0;
				for (int index = start; data[index] != 0; index++)
				{
					count++;
				}

				value = Encoding.ASCII.GetString(data, start, count);
				start += count + 1;
				start = ((start + 3) / 4) * 4;
			}
			else if (type.Name == "Byte[]")
			{
                int length = ValueFromByteArray<int>(data, ref start);
                byte[] buffer = new byte[length];
                Array.Copy(data, start, buffer, 0, buffer.Length);
                start += buffer.Length + 1;
                start = ((start + 3) / 4) * 4;

                value = buffer;
			}
			else
			{
				byte[] buffer;
				switch (type.Name)
				{
					case "Int32":
					case "Single":
						buffer = new byte[4];
						break;

					case "Int64":
					case "Double":
						buffer = new byte[8];
						break;

					default:
						throw new Exception("Unsupported data type.");
				}

				Array.Copy(data, start, buffer, 0, buffer.Length);
				start += buffer.Length;

				if (BitConverter.IsLittleEndian)
				{
					buffer = Library.SwapEndian(buffer);
				}

				switch (type.Name)
				{
					case "Int32":
						value = BitConverter.ToInt32(buffer, 0);
						break;

					case "Int64":
						value = BitConverter.ToInt64(buffer, 0);
						break;

					case "Single":
						value = BitConverter.ToSingle(buffer, 0);
						break;

					case "Double":
						value = BitConverter.ToDouble(buffer, 0);
						break;

					default:
						throw new Exception("Unsupported data type.");
				}
			}

			return (T)value;
		}

		/// <summary>
		/// Serialize a value.
		/// </summary>
		/// <typeparam name="T">The value's data type.</typeparam>
		/// <param name="value">The value to serialize.</param>
		/// <returns>The serialized version of the value.</returns>
		public static byte[] ValueToByteArray<T>(T value)
		{
			object valueObject = value;
			Type type = value.GetType();
			byte[] data = null;

			switch (type.Name)
			{
				case "Int32":
					data = BitConverter.GetBytes((int)valueObject);
					if (BitConverter.IsLittleEndian)
					{
						data = Library.SwapEndian(data);
					}
					break;

				case "Int64":
					data = BitConverter.GetBytes((long)valueObject);
					if (BitConverter.IsLittleEndian)
					{
						data = Library.SwapEndian(data);
					}
					break;

				case "Single":
					data = BitConverter.GetBytes((float)valueObject);
					if (BitConverter.IsLittleEndian)
					{
						data = Library.SwapEndian(data);
					}
					break;

				case "Double":
					data = BitConverter.GetBytes((double)valueObject);
					if (BitConverter.IsLittleEndian)
					{
						data = Library.SwapEndian(data);
					}
					break;

				case "String":
					data = Encoding.ASCII.GetBytes((string)valueObject);
					break;

				case "Byte[]":
					byte[] valueData = ((byte[])valueObject);
					List<byte> bytes = new List<byte>();
					bytes.AddRange(ValueToByteArray(valueData.Length));
					bytes.AddRange(valueData);
					data = bytes.ToArray();
					break;

				default:
					throw new Exception("Unsupported data type.");
			}

			return data;
		}

		/// <summary>
		/// Pad a series of 0-3 null characters.
		/// </summary>
		/// <param name="data">The data source to pad.</param>
		public static void PadNull(List<byte> data)
		{
			byte zero = 0;
			int pad = 4 - (data.Count % 4);
			for (int i = 0; i < pad; i++)
			{
				data.Add(zero);
			}
		}

		/// <summary>
		/// The origin of the packet.
		/// </summary>
		protected IPEndPoint mSourceEndPoint;

		/// <summary>
		/// The OSC address pattern.
		/// </summary>
		protected string mAddress;

		/// <summary>
		/// The contents of the packet.
		/// </summary>
		protected List<object> mData;

        /// <summary>
        /// The destination of sent packets when using TransportType.Tcp.
        /// </summary>
        protected OscClient mClient;

		private static UdpClient sUdpClient;        
	}
}
