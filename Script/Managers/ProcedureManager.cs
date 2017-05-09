using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedureManager : Singleton<ProcedureManager> {

	bool m_hadOperation;
	ProcedureOperation operationArgs;//might need a list

	ProcedureController m_procedureController;
	public ProcedureController.BetweenSwitchState BetweenSwitchStateCallBack {
		get
		{ 
			return m_procedureController.BetweenSwitchStateCallBack;
		}
		set
		{
			m_procedureController.BetweenSwitchStateCallBack = new ProcedureController.BetweenSwitchState (value);
		}
	}

	void Awake(){
		m_hadOperation = false;
		m_procedureController = new ProcedureController ();
		RegistAllProcedure ();
	}

	void Start () {
		m_procedureController.SwitchProcedure ((int)GameProcedureID.GP_PRELOAD);
	}

	void Update () {
		m_procedureController.OnUpdate ();
		//TODO: any operation on procedure
		if(m_hadOperation){
			m_procedureController.Operation_events (operationArgs);
			m_hadOperation = false;
		}
	}

	void FixedUpdate(){
		m_procedureController.OnFixedUpdate ();
	}

	void LateUpdate(){
		m_procedureController.OnLateUpdate ();
	}

	public void SetProcedureOperation(string name,object add1 = null,object add2 = null)
	{
		if (name.Length == 0)
			return;
		operationArgs = new ProcedureOperation (name, add1, add2);
		m_hadOperation = true;
	}

	void RegistAllProcedure()
	{
		//TODO: RegistAllProcedure
	}

	public void SwitchProcedure(int iNewProcedureID, object param1 = null, object param2 = null)
	{
		m_procedureController.SwitchProcedure (iNewProcedureID, param1, param2);
	}

}
