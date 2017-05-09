using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedure operation.Communication between UI/Game Logic and Manager/Controller
/// </summary>
public class ProcedureOperation
{
	public string OperationName{ get; private set;}
	public object addition1{ get; private set;}
	public object addition2{ get; private set;}
	public ProcedureOperation(string _opeName,object _addition1,object _addition2)
	{
		OperationName = _opeName;
		addition1 = _addition1;
		addition2 = _addition2;
	}
}

/// <summary>
/// Game procedure ID,used as a Unique sign.
/// </summary>
public enum GameProcedureID
{
	GP_PRELOAD,
	GP_START,
	GP_LOADING,
	GP_GAME,
}

public abstract class IProcedure {

	protected delegate void OperationFunction(object addition1,object addition2);
	protected Dictionary<string,OperationFunction> m_dictOpeFunc;
	public string ProcedureName{ get; private set;}

	public IProcedure()
	{
		InitOperationDictionary ();
	}

	/// <summary>
	/// Gets the ProcedureID,override by children
	/// </summary>
	/// <returns>(int)GameProcedureID of their own.</returns>
	public abstract int GetProcedureID();

	public virtual void handleOperation(object sender, ProcedureOperation e)
	{
		//Find e.name in m_dictOpeFunc and callback with additions
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
		//Some basic init work
	}

	public virtual void OnEnter(IProcedure prevState, object param1, object param2)
	{
		//Load Assets or Enter Scene or Other init work
	}
	public virtual void OnExit(IProcedure nextState, object param1, object param2)
	{
		//UnLoad Assets or Other clean work
	}

	/// <summary>
	/// Inits the operation dictionary.called by children first
	/// </summary>
	public virtual void InitOperationDictionary()
	{
		m_dictOpeFunc = new Dictionary<string, OperationFunction> ();
	}
}
