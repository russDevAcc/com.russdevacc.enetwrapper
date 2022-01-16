/*
 *  Managed C# wrapper for an extended version of ENet
 *  Copyright (c) 2013 James Bellinger
 *  Copyright (c) 2016 Nate Shoffner
 *  Copyright (c) 2018 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Text;

namespace ENet
{
	internal static class ArrayPool
	{
		[ThreadStatic]
		private static byte[] byteBuffer;
		[ThreadStatic]
		private static IntPtr[] pointerBuffer;

		public static byte[] GetByteBuffer()
		{
			if (byteBuffer == null)
				byteBuffer = new byte[64];

			return byteBuffer;
		}

		public static IntPtr[] GetPointerBuffer()
		{
			if (pointerBuffer == null)
				pointerBuffer = new IntPtr[Library.maxPeers];

			return pointerBuffer;
		}
	}
	public struct Address {
		private ENetAddress nativeAddress;

		internal ENetAddress NativeData {
			get {
				return nativeAddress;
			}

			set {
				nativeAddress = value;
			}
		}

		internal Address(ENetAddress address) {
			nativeAddress = address;
		}

		public ushort Port {
			get {
				return nativeAddress.port;
			}

			set {
				nativeAddress.port = value;
			}
		}

		public string GetIP() {
			StringBuilder ip = new StringBuilder(1025);

			if (Native.enet_address_get_ip(ref nativeAddress, ip, (IntPtr)ip.Capacity) != 0)
				return String.Empty;

			return ip.ToString();
		}

		public bool SetIP(string ip) {
			if (ip == null)
				throw new ArgumentNullException("ip");

			return Native.enet_address_set_ip(ref nativeAddress, ip) == 0;
		}

		public string GetHost() {
			StringBuilder hostName = new StringBuilder(1025);

			if (Native.enet_address_get_hostname(ref nativeAddress, hostName, (IntPtr)hostName.Capacity) != 0)
				return String.Empty;

			return hostName.ToString();
		}

		public bool SetHost(string hostName) {
			if (hostName == null)
				throw new ArgumentNullException("hostName");

			return Native.enet_address_set_hostname(ref nativeAddress, hostName) == 0;
		}
	}
}
