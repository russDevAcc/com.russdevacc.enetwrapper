using ENet;
using NetStack.Buffers;
using System.Runtime.CompilerServices;

namespace NetLib
{

	public struct NetBufferData
	{
		public int readPosition;
		public int nextPosition;
		public uint[] chunks;
	}
	public class NetBuffer
	{

		private static ArrayPool<uint> bytes;


		private const int defaultCapacity = 375; // 375 * 4 = 1500 bytes
		private const int stringLengthBits = 8;
		private const int stringLengthMax = (1 << stringLengthBits) - 1; // 255
		private const int bitsASCII = 7;
		private const int growFactor = 2;
		private const int minGrow = 1;

		public static void Init()
		{
			bytes = ArrayPool<uint>.Create(1024, 1024);
			
		}
		private static NetBufferData CreateBuffer(int capacity)
		{
			return new NetBufferData()
			{
				readPosition = 0,
				nextPosition = 0,
				chunks = bytes.Rent(capacity)
			};
		}
		public static NetBufferData Create(ushort id, int capacity = defaultCapacity)
		{
			var buffer = CreateBuffer(capacity);
			AddUShort(ref buffer, id);
			return buffer;
		}

		public static void Destroy(ref NetBufferData data)
		{
			bytes.Return(data.chunks);
		}



		public static int GetLength(ref NetBufferData data)
		{
			return ((data.nextPosition - 1) >> 3) + 1;

		}

		public static bool IsFinished(ref NetBufferData data)
		{
			return data.nextPosition == data.readPosition;

		}

		[MethodImpl(256)]
		public static void Clear(ref NetBufferData data)
		{
			data.readPosition = 0;
			data.nextPosition = 0;
		}

		[MethodImpl(256)]
		public static void Add(ref NetBufferData data, int numBits, uint value)
		{

			int index = data.nextPosition >> 5;
			int used = data.nextPosition & 0x0000001F;





			ulong chunkMask = ((1UL << used) - 1);
			ulong scratch = data.chunks[index] & chunkMask;
			ulong result = scratch | ((ulong)value << used);

			data.chunks[index] = (uint)result;
			data.chunks[index + 1] = (uint)(result >> 32);
			data.nextPosition += numBits;

		}

		[MethodImpl(256)]
		public static uint Read(ref NetBufferData data, int numBits)
		{
			uint result = Peek(ref data, numBits);

			data.readPosition += numBits;

			return result;
		}

		[MethodImpl(256)]
		public static uint Peek(ref NetBufferData data, int numBits)
		{

			int index = data.readPosition >> 5;
			int used = data.readPosition & 0x0000001F;

			ulong chunkMask = ((1UL << numBits) - 1) << used;
			ulong scratch = (ulong)data.chunks[index];

			if ((index + 1) < data.chunks.Length)
				scratch |= (ulong)data.chunks[index + 1] << 32;

			ulong result = (scratch & chunkMask) >> used;

			return (uint)result;
		}
		

		public static int ToArray(ref NetBufferData data, byte[] bytes)
		{
			Add(ref data, 1, 1);

			int numChunks = (data.nextPosition >> 5) + 1;
			int length = bytes.Length;

			for (int i = 0; i < numChunks; i++)
			{
				int dataIdx = i * 4;
				uint chunk = data.chunks[i];

				if (dataIdx < length)
					bytes[dataIdx] = (byte)(chunk);

				if (dataIdx + 1 < length)
					bytes[dataIdx + 1] = (byte)(chunk >> 8);

				if (dataIdx + 2 < length)
					bytes[dataIdx + 2] = (byte)(chunk >> 16);

				if (dataIdx + 3 < length)
					bytes[dataIdx + 3] = (byte)(chunk >> 24);
			}

			return GetLength(ref data);
		}


		public static NetBufferData FromArray(byte[] bytes, int length)
		{
			var data = CreateBuffer(length);
			int numChunks = (length / 4) + 1;



			for (int i = 0; i < numChunks; i++)
			{
				int dataIdx = i * 4;
				uint chunk = 0;

				if (dataIdx < length)
					chunk = (uint)bytes[dataIdx];

				if (dataIdx + 1 < length)
					chunk = chunk | (uint)bytes[dataIdx + 1] << 8;

				if (dataIdx + 2 < length)
					chunk = chunk | (uint)bytes[dataIdx + 2] << 16;

				if (dataIdx + 3 < length)
					chunk = chunk | (uint)bytes[dataIdx + 3] << 24;

				data.chunks[i] = chunk;
			}

			int positionInByte = FindHighestBitPosition(bytes[length - 1]);

			data.nextPosition = ((length - 1) * 8) + (positionInByte - 1);
			data.readPosition = 0;
			return data;
		}



		[MethodImpl(256)]
		public static void AddBool(ref NetBufferData data, bool value)
		{
			Add(ref data, 1, value ? 1U : 0U);
		}

		[MethodImpl(256)]
		public static bool ReadBool(ref NetBufferData data)
		{
			return Read(ref data, 1) > 0;
		}

		[MethodImpl(256)]
		public static bool PeekBool(ref NetBufferData data)
		{
			return Peek(ref data, 1) > 0;
		}

		[MethodImpl(256)]
		public static void AddByte(ref NetBufferData data, byte value)
		{
			Add(ref data, 8, value);
		}

		[MethodImpl(256)]
		public static byte ReadByte(ref NetBufferData data)
		{
			return (byte)Read(ref data, 8);
		}

		[MethodImpl(256)]
		public static byte PeekByte(ref NetBufferData data)
		{
			return (byte)Peek(ref data, 8);
		}

		[MethodImpl(256)]
		public static void AddShort(ref NetBufferData data, short value)
		{
			AddInt(ref data, value);
		}

		[MethodImpl(256)]
		public static short ReadShort(ref NetBufferData data)
		{
			return (short)ReadInt(ref data);
		}

		[MethodImpl(256)]
		public static short PeekShort(ref NetBufferData data)
		{
			return (short)PeekInt(ref data);
		}

		[MethodImpl(256)]
		public static void AddUShort(ref NetBufferData data, ushort value)
		{
			AddUInt(ref data, value);
		}

		[MethodImpl(256)]
		public static ushort ReadUShort(ref NetBufferData data)
		{
			return (ushort)ReadUInt(ref data);
		}

		[MethodImpl(256)]
		public static ushort PeekUShort(ref NetBufferData data)
		{
			return (ushort)PeekUInt(ref data);
		}

		[MethodImpl(256)]
		public static void AddInt(ref NetBufferData data, int value)
		{
			uint zigzag = (uint)((value << 1) ^ (value >> 31));

			AddUInt(ref data, zigzag);
		}

		[MethodImpl(256)]
		public static int ReadInt(ref NetBufferData data)
		{
			uint value = ReadUInt(ref data);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));

			return zagzig;
		}

		[MethodImpl(256)]
		public static int PeekInt(ref NetBufferData data)
		{
			uint value = PeekUInt(ref data);
			int zagzig = (int)((value >> 1) ^ (-(int)(value & 1)));

			return zagzig;
		}

		[MethodImpl(256)]
		public static void AddUInt(ref NetBufferData data, uint value)
		{
			uint buffer = 0x0u;

			do
			{
				buffer = value & 0x7Fu;
				value >>= 7;

				if (value > 0)
					buffer |= 0x80u;

				Add(ref data, 8, buffer);
			}

			while (value > 0);
		}

		[MethodImpl(256)]
		public static uint ReadUInt(ref NetBufferData data)
		{
			uint buffer = 0x0u;
			uint value = 0x0u;
			int shift = 0;

			do
			{
				buffer = Read(ref data, 8);

				value |= (buffer & 0x7Fu) << shift;
				shift += 7;
			}

			while ((buffer & 0x80u) > 0);

			return value;
		}

		[MethodImpl(256)]
		public static uint PeekUInt(ref NetBufferData data)
		{
			int tempPosition = data.readPosition;
			uint value = ReadUInt(ref data);

			data.readPosition = tempPosition;

			return value;
		}





		[MethodImpl(256)]
		private static int FindHighestBitPosition(byte data)
		{
			int shiftCount = 0;

			while (data > 0)
			{
				data >>= 1;
				shiftCount++;
			}

			return shiftCount;
		}
	}
}

