using System;

namespace CustomInterfaces{
	public interface IResetable{
		object Instance{ get; }
		void InitOnce (object userData);
		void Reset (object userData);
		void Destroy();
	}

	public delegate void ActionEx<in T1,in T2,in T3,in T4,in T5>(T1 arg1,T2 arg2,T3 arg3,T4 arg4,T5 arg5);
	public delegate void ActionEx<in T1,in T2,in T3,in T4,in T5,in T6>(T1 arg1,T2 arg2,T3 arg3,T4 arg4,T5 arg5,T6 arg6);
	public delegate void ActionEx<in T1,in T2,in T3,in T4,in T5,in T6,in T7>(T1 arg1,T2 arg2,T3 arg3,T4 arg4,T5 arg5,T6 arg6,T7 arg7);
}
