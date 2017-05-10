using System;
using System.Text;

public class StringUtil {

	public static byte[] GetUTF8Bytes(string message){
		return Encoding.GetEncoding("utf-8").GetBytes(message);  
	}

	public static string GetUTF8String(byte[] bytes){
		return Encoding.GetEncoding("utf-8").GetString(bytes);  
	}

	public static string Base64Encode(string message) {  
		byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);  
		return Convert.ToBase64String(bytes);  
	}  

	public static string Base64Decode(string message) {  
		byte[] bytes = Convert.FromBase64String(message);  
		return Encoding.GetEncoding("utf-8").GetString(bytes);  
	}

	public static byte[] GetBase64EncodeBytes(string message){
		return GetUTF8Bytes (Base64Encode (message));
	}

	public static string GetBase64DecodeString(byte[] bytes){
		return Base64Decode (GetUTF8String (bytes));
	}
}
