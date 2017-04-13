using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedureOperation
{
	public string OperationName;
	public object addition1;
	public object addition2;
	public ProcedureOperation(string _opeName,object _addition1,object _addition2)
	{
		OperationName = _opeName;
		addition1 = _addition1;
		addition2 = _addition2;
	}
}

public enum GameProcedureID
{
	GP_PRELOAD,
	GP_START,
	GP_LOADING,
	GP_GAME,
	GP_SETTLEMENT
}

public class IProcedure {

	protected delegate void OperationFunction(object addition1,object addition2);
	protected Dictionary<string,OperationFunction> m_dictOpeFunc;
	public string ProcedureName;

	public IProcedure()
	{
		InitOperationDictionary ();
	}

	public virtual int GetProcedureID()
	{
		return 0;
	}
	public virtual void handleOperation(object sender, ProcedureOperation e)
	{
	}
	public virtual void UpDate()
	{
	}
	public virtual void FixedUpdate()
	{
	}
	public virtual void LateUpdate()
	{
	}
	public virtual void Start()
	{
	}

	public virtual void OnEnter(IProcedure prevState, object param1, object param2)
	{
	}
	public virtual void OnExit(IProcedure nextState, object param1, object param2)
	{
	}

	public virtual void InitOperationDictionary()
	{
		m_dictOpeFunc = new Dictionary<string, OperationFunction> ();
	}
}
