using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;

using Blazor.Fluxor;

namespace Logixware.AspNet.Blazor.Fluxor
{
	/// <summary>
	/// Implements members to make the store observable.
	/// </summary>
	public class ObservableMiddleware : Middleware, IObservableStore
	{
		private readonly List<IObserver<IAction>> _ActionSubscribers;
		private readonly List<IObserver<Object>> _StateSubscribers;

		private Dictionary<Type, Object> _States;

		public ObservableMiddleware()
		{
			this._ActionSubscribers = new List<IObserver<IAction>>();
			this._StateSubscribers = new List<IObserver<Object>>();
			this._States = new Dictionary<Type, Object>();
		}

		/// <summary>
		/// Returns an <see cref="T:System.IObservable`1" /> for the state objects inside the store.
		/// </summary>
		public IObservable<Object> States
		{
			get
			{
				return Observable.Create<Object>(o =>
				{
					this._StateSubscribers.Add(o);
					return () => this._StateSubscribers.Remove(o);
				});
			}
		}

		/// <summary>
		/// Returns an <see cref="T:System.IObservable`1" /> for actions which are dispatched.
		/// </summary>
		public IObservable<IAction> Actions
		{
			get
			{
				return Observable.Create<IAction>(o =>
				{
					this._ActionSubscribers.Add(o);
					return () => this._ActionSubscribers.Remove(o);
				});
			}
		}

		public override void BeforeDispatch(IAction action)
		{
			base.BeforeDispatch(action);

			foreach (var __Feature in base.Store.Features.Values.OrderBy(x => x.GetName()))
			{
				this._States.Add(__Feature.GetStateType(), __Feature.GetState());
			}
		}

		public override void AfterDispatch(IAction action)
		{
			base.AfterDispatch(action);

			this._ActionSubscribers.ForEach(x => x.OnNext(action));

			foreach (var __Feature in base.Store.Features.Values.OrderBy(x => x.GetName()))
			{
				if (this._States.ContainsKey(__Feature.GetStateType()) && !Object.ReferenceEquals(__Feature.GetState(), this._States[__Feature.GetStateType()]))
				{
					this._StateSubscribers.ForEach(x => x.OnNext(__Feature.GetState()));
				}
			}

			this._States.Clear();
		}
	}
}
