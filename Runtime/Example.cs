using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WezzelNL.GlobalEvents.Runtime
{
	//Normal (synchronous)

	public struct MyEvent : IGlobalEvent
	{
		public int Parameter { get; }

		public MyEvent(int parameter)
		{
			Parameter = parameter;
		}
	}


//Async (UniTask)

	public struct MyAsyncEvent : IAsyncGlobalEvent
	{
		public int Parameter { get; }

		public MyAsyncEvent(int parameter)
		{
			Parameter = parameter;
		}
	}
	
	public class Example : MonoBehaviour
	{
		public void FromSyncMethod()
		{
			var parameter = 1;
			var evt = new MyAsyncEvent(parameter);
			GlobalEvent.InvokeAsync(evt, destroyCancellationToken).Forget();
		}
		
		public async UniTask FromAsyncMethod(CancellationToken ct)
		{
			var parameter = 1;
			var evt = new MyAsyncEvent(parameter);
			await GlobalEvent.InvokeAsync(evt, ct);
		}
	}
}