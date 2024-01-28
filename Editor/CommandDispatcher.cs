using System;
using System.Collections;
using System.Collections.Generic;
// Based on https://github.com/PimDeWitte/UnityMainThreadDispatcher/blob/master/Runtime/UnityMainThreadDispatcher.cs
namespace PCP.Univim
{
	internal class CommandDispatcher
	{
		private readonly Queue<Action> cmdQueue = new();
		// TODO::For now i dont see a point to use coroutine or this dispatcher in editor mode,need more information.
		private IEnumerator ActionWrapper(Action action)
		{
			action();
			yield return null;
		}

		public void Update()
		{
			lock (cmdQueue)
			{
				while (cmdQueue.Count > 0)
				{
					cmdQueue.Dequeue().Invoke();
				}
			}
		}
		public void Enqueue(Action action)
		{
			lock (cmdQueue)
			{
				cmdQueue.Enqueue(action);
			}
		}
	}
}
