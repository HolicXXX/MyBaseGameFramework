
using System;

public enum TaskStatus{
	TS_TODO,
	TS_DOING,
	TS_DONE,
	TS_ERROR
}

public interface ITask {
	int ID{ get; }
	bool Done{ get; }
}

public interface ITaskAgent<T> where T : ITask{
	T Task{ get; }
	float WaitedTime{ get; }
	void Update(float dt);
	void Start (T tast);
	void Reset();
	void Close();
}
