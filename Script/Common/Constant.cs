
namespace Constant {
	public static class HTTPRequest{
		public readonly static string Request = "Request";
	}

	public struct MsgBase{
		public int type;
		public string data;
	}

	public enum MsgType{
		Msg_Type1,
		Msg_Type2
	}
}
