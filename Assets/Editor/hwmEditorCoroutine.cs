using System.Collections;
using UnityEditor;

public class hwmEditorCoroutine
{
	private readonly IEnumerator m_Routine;
	private bool m_IsRuning;

	public static hwmEditorCoroutine Start(IEnumerator routine)
	{
		hwmEditorCoroutine coroutine = new hwmEditorCoroutine(routine);
		coroutine.Start();
		return coroutine;
	}

	private hwmEditorCoroutine(IEnumerator routine)
	{
		m_Routine = routine;
	}

	public bool IsRuning()
	{
		return m_IsRuning;
	}

	private void Start()
	{
		EditorApplication.update += Update;
		m_IsRuning = true;
	}

	public void Stop()
	{
		m_IsRuning = false;
		EditorApplication.update -= Update;
	}

	private void Update()
	{
		if (!m_Routine.MoveNext())
		{
			Stop();
		}
	}
}