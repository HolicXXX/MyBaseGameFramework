using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeDataManager : Singleton<RuntimeDataManager> {
	//TODO:Data Field,use properties

	void Awake(){
		//Init Data with PlayerPrefs or config
	}

	void Start () {
		
	}

	void OnDestroy(){
		//Save Data
		PlayerPrefs.Save();
		base.OnDestroy ();
	}
}
