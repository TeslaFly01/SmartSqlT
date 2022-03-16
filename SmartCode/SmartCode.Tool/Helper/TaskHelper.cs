// Copyright (c) 2015, Yves Goergen, http://unclassified.software/source/taskhelper
// Copying and distribution of this file, with or without modification, are permitted provided the
// copyright notice and this notice are preserved. This file is offered as-is, without any warranty.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SmartCode.Helper
{
	/// <summary>
	/// Provides static helper methods for easier execution of asynchronous and synchronised
	/// operations.
	/// </summary>
	public static class TaskHelper
	{
		/// <summary>
		/// Gets or sets a handler method for unhandled task exceptions.
		/// </summary>
		public static Action<Exception> UnhandledTaskException { get; set; }

		/// <summary>
		/// Starts an Action in a pool thread.
		/// </summary>
		/// <param name="action">Action to start in a pool thread.</param>
		/// <param name="endAction">Action to start in the source synchronisation context when the background work has finished.</param>
		/// <returns>A CancellationTokenSource instance to control cancelling of the background task.</returns>
		public static CancellationTokenSource Start(Action<CancellationToken> action, Action<Task> endAction = null)
		{
			Task task;
			return Start(action, endAction, out task);
		}

		/// <summary>
		/// Starts an Action in a pool thread.
		/// </summary>
		/// <param name="action">Action to start in a pool thread.</param>
		/// <param name="task">Returns the created task instance.</param>
		/// <returns>A CancellationTokenSource instance to control cancelling of the background task.</returns>
		public static CancellationTokenSource Start(Action<CancellationToken> action, out Task task)
		{
			return Start(action, null, out task);
		}

		/// <summary>
		/// Starts an Action in a pool thread.
		/// </summary>
		/// <param name="action">Action to start in a pool thread.</param>
		/// <param name="endAction">Action to start in the source synchronisation context when the background work has finished.</param>
		/// <param name="task">Returns the created task instance.</param>
		/// <returns>A CancellationTokenSource instance to control cancelling of the background task.</returns>
		public static CancellationTokenSource Start(Action<CancellationToken> action, Action<Task> endAction, out Task task)
		{
			TaskScheduler scheduler = null;
			if (endAction != null)
			{
				scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			}
			var cts = new CancellationTokenSource();
			task = Task.Factory.StartNew(() => action(cts.Token), cts.Token);
			if (endAction != null)
			{
				task = task.ContinueWith(endAction, scheduler);
			}
			if (scheduler == null)
			{
				scheduler = TaskScheduler.Current;
			}
			task.ContinueWith(t =>
			{
				if (UnhandledTaskException != null)
				{
					UnhandledTaskException(t.Exception);
				}
			}, cts.Token, TaskContinuationOptions.OnlyOnFaulted, scheduler);
			return cts;
		}

		/// <summary>
		/// Posts an Action to the Dispatcher queue for execution.
		/// </summary>
		/// <param name="action">Action to post to the queue.</param>
		public static void Post(Action action)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Normal);
		}

		/// <summary>
		/// Posts an Action to the Dispatcher queue for execution when all loading has been done.
		/// </summary>
		/// <param name="action">Action to post to the queue.</param>
		public static void WhenLoaded(Action action)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Loaded);
		}

		/// <summary>
		/// Posts an Action to the Dispatcher queue for execution with Background priority.
		/// </summary>
		/// <param name="action">Action to post to the queue.</param>
		public static void Background(Action action)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(action, DispatcherPriority.Background);
		}

		// Source: http://msdn.microsoft.com/de-de/library/system.windows.threading.dispatcher.pushframe.aspx
		/// <summary>
		/// Enters the message loop to process all pending messages down to the specified priority.
		/// This method returns after all messages have been processed.
		/// </summary>
		/// <param name="priority">Minimum priority of the messages to process.</param>
		public static void DoEvents(DispatcherPriority priority = DispatcherPriority.Background)
		{
			DispatcherFrame frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(
				priority,
				new DispatcherOperationCallback(ExitFrame), frame);
			Dispatcher.PushFrame(frame);
		}

		private static object ExitFrame(object f)
		{
			((DispatcherFrame)f).Continue = false;
			return null;
		}

		/// <summary>
		/// Waits for the WaitHandle to become set and then invokes the Action.
		/// </summary>
		/// <param name="handle">The WaitHandle to wait on.</param>
		/// <param name="action">The Action to invoke when <paramref name="handle"/> is set.</param>
		/// <param name="repeatCondition">A function that determines whether to wait for the handle
		/// again after invoking the action. If null, the waiting is not repeated.</param>
		public static void WaitAction(this WaitHandle handle, Action action, Func<bool> repeatCondition = null)
		{
			Start(c =>
			{
				do
				{
					handle.WaitOne();
					action();
				}
				while (repeatCondition != null && repeatCondition());
			});
		}

		public static void AppDispatch(Action action)
		{
			Application.Current.Dispatcher.Invoke(action);
		}
	}
}
