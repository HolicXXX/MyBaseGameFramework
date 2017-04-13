using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcedureController {
	
	//OperationEvent transport
	public delegate void ProcedureEventHandler(object sender, ProcedureOperation e);
	public event ProcedureEventHandler operationTran;

	/// <summary>
	/// 存储所有注册进来的流程。key是状态ID，value是流程对象
	/// </summary>
	private Dictionary<int, IProcedure> m_dictProcedure;

	/// <summary>
	/// 当前运行的流程
	/// </summary>
	private IProcedure m_curProcedure;

	public ProcedureController()
	{
		m_curProcedure = null;
		m_dictProcedure = new Dictionary<int, IProcedure> ();
	}

	/// <summary>
	/// 注册一个流程
	/// </summary>
	/// <param name="procedure">要注册的流程</param>
	/// <returns>成功返回true，如果此流程ID已存在或流程为NULL，则返回false</returns>
	public bool RegistProcedure(IProcedure procedure)
	{
		if (null == procedure)
		{
			Debug.LogWarning("ProcedureController::RegistProcedure->procedure is null");
			return false;
		}

		if (m_dictProcedure.ContainsKey(procedure.GetProcedureID()))
		{
			Debug.LogWarning("ProcedureController::RegistProcedure->procedure had exist! procedure id=" + procedure.GetProcedureID());
			return false;
		}

		m_dictProcedure[procedure.GetProcedureID()] = procedure;

		return true;
	}

	public IProcedure GetCurrentProcedure()
	{
		if (m_curProcedure != null) 
		{
			return m_curProcedure;
		}
		return null;
	}

	/// <summary>
	/// 尝试获取一个流程
	/// </summary>
	/// <returns>The procedure or null.</returns>
	/// <param name="iProcedureId">procedure id.</param>
	public IProcedure GetProcedure(int iProcedureId)
	{
		IProcedure ret = null;
		m_dictProcedure.TryGetValue(iProcedureId, out ret);
		return ret;
	}

	/// <summary>
	/// 停止当前流程，切换到null
	/// </summary>
	public void StopProcedure(object param1, object param2)
	{
		if (null == m_curProcedure)
		{
			return;
		}

		m_curProcedure.OnExit(null, param1, param2);
		m_curProcedure = null;
	}

	/// <summary>
	/// 取消一个流程的注册
	/// </summary>
	/// <returns>如果找不到流程或者流程正在运行则返回False</returns>
	/// <param name="iProcedureID">procedure ID.</param>
	public bool CancelProcedure(int iProcedureID)
	{
		if (!m_dictProcedure.ContainsKey(iProcedureID))
		{
			return false;
		}

		if (null != m_curProcedure && m_curProcedure.GetProcedureID() == iProcedureID)
		{
			return false;
		}

		return m_dictProcedure.Remove(iProcedureID);
	}

	public delegate void BetweenSwitchState(IProcedure from, IProcedure to, object param1, object param2);
	/// <summary>
	/// 在切换流程之间回调
	/// </summary>
	public BetweenSwitchState BetweenSwitchStateCallBack { get; set; }

	/// <summary>
	/// 切换流程
	/// </summary>
	/// <param name="iNewStateID">要切换的新流程</param>
	/// <returns>如果找不到新的流程，或者新旧流程一样，返回false</returns>
	public bool SwitchProcedure(int iNewProcedureID, object param1 = null, object param2 = null)
	{
		//流程一样，不做转换//
		if (null != m_curProcedure && m_curProcedure.GetProcedureID() == iNewProcedureID)
		{
			return false;
		}

		IProcedure newProcedure = null;
		m_dictProcedure.TryGetValue(iNewProcedureID, out newProcedure);
		if (null == newProcedure)
		{
			return false;
		}

		IProcedure oldProcedure = m_curProcedure;

		if (null != oldProcedure)
		{
			oldProcedure.OnExit(newProcedure, param1, param2);
			operationTran -= oldProcedure.handleOperation;
		}

		//Dynamic callback
		if (BetweenSwitchStateCallBack != null) 
		{
			BetweenSwitchStateCallBack (oldProcedure, newProcedure, param1, param2);
			BetweenSwitchStateCallBack = null;
		}

		m_curProcedure = newProcedure;

		if (null != newProcedure)
		{
			newProcedure.Start ();
			newProcedure.OnEnter(oldProcedure, param1, param2);

			operationTran = new ProcedureEventHandler(newProcedure.handleOperation);
		}

		return true;
	}

	/// <summary>
	/// 获取当前流程ID
	/// </summary>
	/// <returns></returns>
	public int GetCurProcedureID()
	{
		IProcedure procedure = GetCurrentProcedure();
		return (null == procedure) ? 0 : procedure.GetProcedureID();
	}

	/// <summary>
	/// 判断当前是否在某个流程下
	/// </summary>
	/// <param name="iStateID"></param>
	/// <returns></returns>
	public bool IsInProcedure(int iProcedureID)
	{
		if (null == m_curProcedure)
		{
			return false;
		}

		return m_curProcedure.GetProcedureID() == iProcedureID;
	}

	//get operationevent from manager
	public void Operation_events(ProcedureOperation e)
	{
		if (e != null)
		{
			operationTran(this, e);
		}
	}

	/// <summary>
	/// 每帧的更新回调
	/// </summary>
	public void OnUpdate()
	{
		if (null != m_curProcedure)
		{
			m_curProcedure.UpDate();
		}
	}

	/// <summary>
	/// 每帧的更新回调
	/// </summary>
	public void OnFixedUpdate()
	{
		if (null != m_curProcedure)
		{
			m_curProcedure.FixedUpdate();
		}
	}

	/// <summary>
	/// 每帧的更新回调
	/// </summary>
	public void OnLateUpdate()
	{
		if (null != m_curProcedure)
		{
			m_curProcedure.LateUpdate();
		}
	}
}
