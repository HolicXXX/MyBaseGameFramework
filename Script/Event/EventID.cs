using System;

/// <summary>
/// EventID for FrameWork.
/// </summary>
public enum EventID
{
	Config_Init_Complete = 0,

	Load_AssetBundle_Start,
	Load_AssetBundle_Progress,
	Load_AssetBundle_Success,
	Load_AssetBundle_Failure,
	Load_Asset_Start,
	Load_Asset_Progress,
	Load_Asset_Success,
	Load_Asset_Failure,

	Network_Connected,
	Network_Closed,
	Network_SendPacket,
	Network_Error,
	Network_CustomError,

	WebRequest_Start,
	WebRequest_Success,
	WebRequest_Failure,

	UI_Open_Progress,
	UI_Open_Success,
	UI_Open_Failure,
	UI_Close_Complete,

	Procedure_OnEnter,
	Procedure_OnExit,

	Audio_Play_Success,
	Audio_Play_Failure,

	Scene_Load_Success,
	Scene_Load_Update,
	Scene_Load_Failure,
	Scene_Unload_Success,
	Scene_Unload_Failure,
}

/// <summary>
/// Custom EventID for user.
/// </summary>
public enum CustomEventID
{
	Custom_Event_1 = 10000,
}

