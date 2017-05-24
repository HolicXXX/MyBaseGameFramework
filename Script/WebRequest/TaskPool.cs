
using System.Collections.Generic;

public class TaskPool<T> where T : ITask {
	private readonly LinkedList<T> WaitingTasks;
	private readonly Queue<ITaskAgent<T>> FreeAgents;
	private readonly LinkedList<ITaskAgent<T>> WorkingAgents;

	public int TotalAgentCount{ get { return FreeAgents.Count + WorkingAgents.Count; } }
	public int FreeAgentCount{ get { return FreeAgents.Count; } }
	public int WorkingAgentCount{ get { return WorkingAgents.Count; } }
	public int WaitingTaskCount{ get { return WaitingTasks.Count; } }

	public TaskPool(){
		WaitingTasks = new LinkedList<T> ();
		FreeAgents = new Queue<ITaskAgent<T>> ();
		WorkingAgents = new LinkedList<ITaskAgent<T>> ();
	}

	public void Update(float dt){
		var agent = WorkingAgents.First;
		while(!agent.IsNull()) {
			if (agent.Value.Task.Done) {
				var next = agent.Next;
				agent.Value.Reset ();
				FreeAgents.Enqueue (agent.Value);
				WorkingAgents.Remove (agent);
				agent = next;
				continue;
			}
			agent.Value.Update (dt);
			agent = agent.Next;
		}

		while (WaitingTaskCount > 0) {
			if (FreeAgentCount > 0) {
				T task = WaitingTasks.First.Value;
				WaitingTasks.RemoveFirst ();
				var fagent = FreeAgents.Dequeue ();
				fagent.Start (task);
				WorkingAgents.AddLast (fagent);
			}
		}
	}

	public void AddTask(T task){
		WaitingTasks.AddLast (task);
	}

	public void AddAgent(ITaskAgent<T> agent){
		FreeAgents.Enqueue (agent);
	}

	public T RemoveTask(int serialID){
		foreach (var task in WaitingTasks) {
			if(task.ID == serialID){
				WaitingTasks.Remove (task);
				return task;
			}
		}

		foreach (var agent in WorkingAgents) {
			if (agent.Task.ID == serialID) {
				var task = agent.Task;
				agent.Reset ();
				FreeAgents.Enqueue(agent);
				WorkingAgents.Remove (agent);
				return task;
			}
		}
		return default(T);
	}

	public void RemoveAllTasks(){
		WaitingTasks.Clear ();
		foreach (var agent in WorkingAgents) {
			agent.Reset ();
			FreeAgents.Enqueue (agent);
		}
		WorkingAgents.Clear ();
	}

	public void ClearPool(){
		while (FreeAgentCount > 0) {
			FreeAgents.Dequeue ().Close ();
		}
		foreach (var agent in WorkingAgents) {
			agent.Close ();
		}
		WorkingAgents.Clear ();
		WaitingTasks.Clear ();
	}

}
