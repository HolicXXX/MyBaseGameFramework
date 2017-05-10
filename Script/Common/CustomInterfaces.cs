using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomInterfaces{
	public interface IResetable{
		void InitOnce ();
		void Reset ();
	}
}
