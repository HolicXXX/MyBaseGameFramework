using System;
using System.Text;

public class StringUtil {

	public static string Base64Encode(string message) {  
		byte[] bytes = Encoding.GetEncoding("utf-8").GetBytes(message);  
		return Convert.ToBase64String(bytes);  
	}  

	public static string Base64Decode(string message) {  
		byte[] bytes = Convert.FromBase64String(message);  
		return Encoding.GetEncoding("utf-8").GetString(bytes);  
	}
}
