using UnityEngine;
using System.Collections;

public class DefaultUIGroupHelper : IUIGroupHelper
{
	public override void SetDepth (int depth){
		gameObject.transform.SetSiblingIndex (depth);
	}
}

