
/// <summary>
/// NetWork type.
/// </summary>
public enum NetWorkType{
	NWT_UNKNOWN = 0,
	NWT_IPV4,
	NWT_IPV6
}

/// <summary>
/// 网络错误码。
/// </summary>
public enum NetworkErrorCode
{
	/// <summary>
	/// 状态错误。
	/// </summary>
	NRC_STATUSERROR,

	/// <summary>
	/// 序列化错误。
	/// </summary>
	NRC_SERIALIZEERROR,

	/// <summary>
	/// 反序列化错误。
	/// </summary>
	NRC_DESERIALIZEERROR,

	/// <summary>
	/// 连接错误。
	/// </summary>
	NRC_CONNECTERROR,

	/// <summary>
	/// 发送错误。
	/// </summary>
	NRC_SENDERROR,

	/// <summary>
	/// 接收错误。
	/// </summary>
	NRC_RECEIVEERROR,

	/// <summary>
	/// 消息包头错误。
	/// </summary>
	NRC_HEADERERROR,

	/// <summary>
	/// 消息包长度错误。
	/// </summary>
	NRC_OUTOFRANGEERROR,

	/// <summary>
	/// 消息包流错误。
	/// </summary>
	NRC_STEAMERROR,
}
