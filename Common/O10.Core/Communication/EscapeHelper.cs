using System;
using System.IO;

namespace O10.Core.Communication
{
	public class EscapeHelper : IDisposable
	{
		private readonly MemoryStream _memoryStream;
		private readonly BinaryWriter _binaryWriter;
		private bool _disposed = false; // To detect redundant calls


		public EscapeHelper()
		{
			_memoryStream = new MemoryStream();
			_binaryWriter = new BinaryWriter(_memoryStream);
		}

		private static void WriteByteWithEncoding(BinaryWriter bw, byte b)
		{
			if (b == Globals.DLE || b == Globals.STX)
			{
				bw.Write(Globals.DLE);
				b += Globals.DLE;
			}

			bw.Write(b);
		}

		public byte[] GetEscapedPacketBytes(byte[] packet)
		{
			_memoryStream.Seek(0, SeekOrigin.Begin);
			_memoryStream.SetLength(0);

			_binaryWriter.Write(Globals.DLE);
			_binaryWriter.Write(Globals.STX);

			long pos1 = _memoryStream.Position;
			_memoryStream.Seek(8, SeekOrigin.Current);

			WriteEscapedBody(packet);

			long pos2 = _memoryStream.Position;
			_memoryStream.Seek(pos1, SeekOrigin.Begin);
			uint length = (uint)(pos2 - pos1 - 8);
			byte[] lengthByte = BitConverter.GetBytes(length);
			WriteByteWithEncoding(_binaryWriter, lengthByte[0]);
			WriteByteWithEncoding(_binaryWriter, lengthByte[1]);
			WriteByteWithEncoding(_binaryWriter, lengthByte[2]);
			WriteByteWithEncoding(_binaryWriter, lengthByte[3]);
			_memoryStream.Seek(pos2, SeekOrigin.Begin);

			return _memoryStream.ToArray();
		}

		private void WriteEscapedBody(byte[] packet)
		{
			for (int i = 0; i < packet.Length; i++)
			{
				WriteByteWithEncoding(_binaryWriter, packet[i]);
			}
		}

		public byte[] GetEscapedBodyBytes(byte[] packet)
		{
			_memoryStream.Seek(0, SeekOrigin.Begin);
			_memoryStream.SetLength(0);

			WriteEscapedBody(packet);
			return _memoryStream.ToArray();
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_binaryWriter?.Dispose();
					_memoryStream?.Dispose();
				}

				_disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
