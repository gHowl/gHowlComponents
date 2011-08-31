using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Represents an Osc Message packet.
	/// </summary>
	public class OscMessage : OscPacket
	{
		/// <summary>
		/// Specifies if the packet is an OSC bundle.
		/// </summary>
		public override bool IsBundle
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a new instance of OscMessage. 
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="address">The OSC address pattern.</param>
		public OscMessage(IPEndPoint sourceEndPoint, string address)
			: this(sourceEndPoint, address, null)
		{
		}

        /// <summary>
        /// Creates a new instance of OscMessage. 
        /// </summary>
        /// <param name="sourceEndPoint">The packet origin.</param>
        /// <param name="address">The OSC address pattern.</param>
        /// <param name="client">The destination of sent packets when using TransportType.Tcp.</param>
        public OscMessage(IPEndPoint sourceEndPoint, string address, OscClient client)
            : base(sourceEndPoint, address, client)
        {
            Trace.Assert(address.StartsWith(AddressPrefix));

            mTypeTag = String.Empty + DefaultTag;
        }

		/// <summary>
		/// Creates a new instance of OscMessage. 
		/// </summary>
		/// <param name="sourceEndPoint">The packet origin.</param>
		/// <param name="address">The OSC address pattern.</param>
		/// <param name="value">A value to append to the message.</param>
		public OscMessage(IPEndPoint sourceEndPoint, string address, object value)
			: this(sourceEndPoint, address)
		{
			Append(value);			
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

			data.AddRange(OscPacket.ValueToByteArray(mTypeTag));
			OscPacket.PadNull(data);

			foreach (object value in mData)
			{
				data.AddRange(OscPacket.ValueToByteArray(value));
				if (value is string || value is byte[])
				{
					OscPacket.PadNull(data);
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
		/// <returns>The newly deserialized packet.</returns>
		public static OscMessage FromByteArray(IPEndPoint sourceEndPoint, byte[] data, ref int start)
		{
			string address = OscPacket.ValueFromByteArray<string>(data, ref start);
			OscMessage message = new OscMessage(sourceEndPoint, address);

			char[] tags = OscPacket.ValueFromByteArray<string>(data, ref start).ToCharArray();
			foreach (char tag in tags)
			{
				object value;
				switch (tag)
				{
					case DefaultTag:
						continue;

					case IntegerTag:
						value = OscPacket.ValueFromByteArray<int>(data, ref start);
						break;

					case LongTag:
						value = OscPacket.ValueFromByteArray<long>(data, ref start);
						break;

					case FloatTag:
						value = OscPacket.ValueFromByteArray<float>(data, ref start);
						break;

					case DoubleTag:
						value = OscPacket.ValueFromByteArray<double>(data, ref start);
						break;

					case StringTag:
					case SymbolTag:
						value = OscPacket.ValueFromByteArray<string>(data, ref start);
						break;

					case BlobTag:
						value = OscPacket.ValueFromByteArray<byte[]>(data, ref start);
						break;

					default:
						Debug.WriteLine("Unknown tag: " + tag);
						continue;
				}

				message.Append(value);
			}

			return message;
		}

		/// <summary>
		/// Appends a value to the message.
		/// </summary>
		/// <typeparam name="T">The type of object being appended.</typeparam>
		/// <param name="value">The value to append.</param>
		public override int Append<T>(T value)
		{
			Type type = value.GetType();
			char typeTag;

			switch (type.Name)
			{
				case "Int32":
					typeTag = IntegerTag;
					break;

				case "Int64":
					typeTag = LongTag;
					break;

				case "Single":
					typeTag = FloatTag;
					break;

				case "Double":
					typeTag = DoubleTag;
					break;

				case "String":
					typeTag = StringTag;
					break;

				case "Byte[]":
					typeTag = BlobTag;
					break;

				default:
					throw new Exception("Unsupported data type.");
			}

			mTypeTag += typeTag;
			mData.Add(value);

			return mData.Count - 1;
		}

		/// <summary>
		/// Update a value within the message at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to update.</param>
		/// <param name="value">The value to update the element with.</param>
		public virtual void UpdateDataAt(int index, object value)
		{
			if (mData.Count == 0 || mData.Count <= index)
			{
				throw new ArgumentOutOfRangeException();
			}

			mData[index] = value;
		}

		///// <summary>
		///// Remove a value from the message at the specified index.
		///// </summary>
		///// <param name="index">The zero-based index of the element to remove.</param>
		//public virtual void RemoveDataAt(int index)
		//{
		//TODO: remove typetag
		//    mData.RemoveAt(index);
		//}

        /// <summary>
        /// Remove all data from the message.
        /// </summary>
        public void ClearData()
        {
            mTypeTag = String.Empty + DefaultTag;
            mData.Clear();
        }

		/// <summary>
		/// The prefix required by OSC address patterns.
		/// </summary>
		protected const string AddressPrefix = "/";

		/// <summary>
		/// The beginning character in an Osc message type tag.
		/// </summary>
		protected const char DefaultTag = ',';

		/// <summary>
		/// The type tag for a 32-bit integer.
		/// </summary>
		protected const char IntegerTag = 'i';

		/// <summary>
		/// The type tag for an floating point value.
		/// </summary>
		protected const char FloatTag = 'f';

		/// <summary>
		/// The type tag for a 64-bit integer.
		/// </summary>
		protected const char LongTag = 'h';

		/// <summary>
		/// The type tag for an double-precision floating point value.
		/// </summary>
		protected const char DoubleTag = 'd';

		/// <summary>
		/// The type tag for a string.
		/// </summary>
		protected const char StringTag = 's';

		/// <summary>
		/// The type tag for a symbol.
		/// </summary>
		protected const char SymbolTag = 'S';

		/// <summary>
		/// The type tag for a blob (binary large object -- byte array).
		/// </summary>
		protected const char BlobTag = 'b';

		private string mTypeTag;
	}
}
