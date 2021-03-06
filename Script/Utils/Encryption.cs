﻿
using System;

namespace Utility
{
	/// <summary>
	/// 加密解密相关的实用函数。
	/// </summary>
	internal static class Encryption
	{
		private const int QuickEncryptLength = 220;

		/// <summary>
		/// 将 bytes 使用 code 做异或运算的快速版本。
		/// </summary>
		/// <param name="bytes">原始二进制流。</param>
		/// <param name="code">异或二进制流。</param>
		/// <returns>异或后的二进制流。</returns>
		public static byte[] GetQuickXorBytes(byte[] bytes, byte[] code)
		{
			return GetXorBytes(bytes, code, QuickEncryptLength);
		}

		/// <summary>
		/// 将 bytes 使用 code 做异或运算。
		/// </summary>
		/// <param name="bytes">原始二进制流。</param>
		/// <param name="code">异或二进制流。</param>
		/// <param name="length">异或计算长度，若小于等于 0，则计算整个二进制流。</param>
		/// <returns>异或后的二进制流。</returns>
		public static byte[] GetXorBytes(byte[] bytes, byte[] code, int length = 0)
		{
			if (bytes.IsNull() || code.IsNull() || code.Length <= 0)
			{
				return null;
			}

			int codeIndex = 0;
			int bytesLength = bytes.Length;
			if (length <= 0 || length > bytesLength)
			{
				length = bytesLength;
			}

			byte[] result = new byte[bytesLength];
			Buffer.BlockCopy(bytes, 0, result, 0, bytesLength);

			for (int i = 0; i < length; i++)
			{
				result[i] ^= code[codeIndex++];
				codeIndex = codeIndex % code.Length;
			}

			return result;
		}
	}
}

