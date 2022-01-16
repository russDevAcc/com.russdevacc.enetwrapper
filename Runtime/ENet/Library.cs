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

namespace ENet
{
    public static class Library {
		public const uint maxChannelCount = 0xFF;
		public const uint maxPeers = 0xFFF;
		public const uint maxPacketSize = 32 * 1024 * 1024;
		public const uint throttleThreshold = 40;
		public const uint throttleScale = 32;
		public const uint throttleAcceleration = 2;
		public const uint throttleDeceleration = 2;
		public const uint throttleInterval = 5000;
		public const uint timeoutLimit = 32;
		public const uint timeoutMinimum = 5000;
		public const uint timeoutMaximum = 30000;
		public const uint version = (2 << 16) | (4 << 8) | (7);

		public static uint Time {
			get {
				return Native.enet_time_get();
			}
		}

		public static bool Initialize() {
			if (Native.enet_linked_version() != version)
				throw new InvalidOperationException("Incompatatible version");

			return Native.enet_initialize() == 0;
		}

		public static bool Initialize(Callbacks callbacks) {
			if (callbacks == null)
				throw new ArgumentNullException("callbacks");

			if (Native.enet_linked_version() != version)
				throw new InvalidOperationException("Incompatatible version");

			ENetCallbacks nativeCallbacks = callbacks.NativeData;

			return Native.enet_initialize_with_callbacks(version, ref nativeCallbacks) == 0;
		}

		public static void Deinitialize() {
			Native.enet_deinitialize();
		}

		public static ulong CRC64(IntPtr buffers, int bufferCount) {
			return Native.enet_crc64(buffers, bufferCount);
		}
	}
}
