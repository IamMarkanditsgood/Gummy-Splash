using UnityEngine;
using UnityEngine.Events;

namespace Mkey
{
	public class LifeHelper : MonoBehaviour
	{
		private LifesHolder MLife => LifesHolder.Instance;
		public bool callEventsByStart;

		public UnityEvent<int> ChangeEvent;
		public UnityEvent<int> LoadEvent;
		public UnityEvent<float> ChangeProgressEvent;
		public UnityEvent ZeroValueReachedEvent;
		public UnityEvent MaxValueReachedEvent;
		public UnityEvent StartInfiniteLifeEvent;
		public UnityEvent EndInfiniteLifeEvent;
		#region regular
		private void Start()
		{
			MLife.ChangeEvent.AddListener(ChangeEventHandler);
			MLife.LoadEvent.AddListener(LoadEventHandler);

			if (callEventsByStart)
			{
				LoadEventHandler(LifesHolder.Count);
				ChangeEventHandler(LifesHolder.Count);
			}
		}

		private void OnDestroy()
		{
			MLife.ChangeEvent.RemoveListener(ChangeEventHandler);
			MLife.LoadEvent.RemoveListener(LoadEventHandler);
		}
		#endregion regular

		private void ChangeEventHandler(int count)
		{
			ChangeProgressEvent?.Invoke((float)count / (float)MLife.MaxCount);
			if (LifesHolder.Count == 0) ZeroValueReachedEvent?.Invoke();
			if (LifesHolder.Count == MLife.MaxCount) MaxValueReachedEvent?.Invoke();
			ChangeEvent?.Invoke(count);
		}

		private void LoadEventHandler(int count)
		{
			LoadEvent?.Invoke(count);
		}
		
	}
}

