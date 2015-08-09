using System;
using System.Threading;

namespace Zepheus.Util
{
	public class UserWorkItem
	{
		#region Properties
		/// <summary>
		/// The delegate to Invoke.
		/// </summary>
		public Delegate Action { get; private set; }
		/// <summary>
		/// Parameters for the Delegate
		/// </summary>
		protected virtual object[] Parameters { get; set; }
		#endregion
		#region .ctor
		/// <summary>
		/// Instances a new <see cref="UserWorkItem"/>-Object
		/// </summary>
		/// <param name="act">Action to perform</param>
		public UserWorkItem(Delegate act)
		{
			this.Action = act;
		}
		#endregion
		#region Methods
		#region Protected
		/// <summary>
		/// Invokes the <see cref="WorkStarted"/>-Event
		/// </summary>
		protected virtual void OnWorkStarted()
		{
			if(WorkStarted != null)
				WorkStarted(this, new EventArgs());
		}
		/// <summary>
		/// Invokes the <see cref="WorkDone"/>-Event
		/// </summary>
		protected virtual void OnWorkDone()
		{
			if(WorkDone != null)
				WorkDone(this, new EventArgs());
		}
		/// <summary>
		/// Invokes the <see cref="Queued"/>-Event
		/// </summary>
		protected virtual void OnQueued()
		{
			if(Queued != null)
				Queued(this, new EventArgs());
		}
		#endregion
		/// <summary>
		/// Perform the action of this UserWorkItem
		/// </summary>
		/// <param name="state"></param>
		protected virtual void DoWork(Object state)
		{
			OnWorkStarted();
			this.Action.DynamicInvoke(this.Parameters);
			OnWorkDone();
		}
		/// <summary>
		/// Queue this item to the <see cref="System.Threading.ThreadPool"/>
		/// </summary>
		public void Queue()
		{
			ThreadPool.QueueUserWorkItem(DoWork);
			OnQueued();
		}
		#endregion
		#region Events
		/// <summary>
		/// Called when the <see cref="Action"/>-Delegate is started
		/// </summary>
		public event EventHandler WorkStarted;
		/// <summary>
		/// Called when the <see cref="Action"/>-Delegate is finished.
		/// </summary>
		public event EventHandler WorkDone;
		/// <summary>
		/// Called when the Item is queued to the <see cref="System.Threading.ThreadPool"/>
		/// </summary>
		public event EventHandler Queued;
		#endregion
	}
}
