using System;
using System.Threading;
using System.Reflection;
using System.Security.AccessControl;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Fasdr.Backend
{
	public class SingleGlobalInstance : IDisposable
	{
		public bool hasHandle = false;
		Mutex mutex;

		private void InitMutex()
		{
			string appGuid = ((GuidAttribute)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(GuidAttribute), false).GetValue(0)).Value.ToString();
			string mutexId = string.Format("Global\\{{{0}}}", appGuid);
			mutex = new Mutex(false, mutexId);

			var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
			var securitySettings = new MutexSecurity();
			securitySettings.AddAccessRule(allowEveryoneRule);
			if (!IsRunningOnMono ()) {
				mutex.SetAccessControl (securitySettings);
			}
		}

		public SingleGlobalInstance(int timeOut)
		{
			InitMutex();
			try
			{
				if(timeOut < 0)
					hasHandle = mutex.WaitOne(Timeout.Infinite, false);
				else
					hasHandle = mutex.WaitOne(timeOut, false);

				if (hasHandle == false)
					throw new TimeoutException("Timeout waiting for exclusive access on SingleInstance");
			}
			catch (AbandonedMutexException)
			{
				hasHandle = true;
			}
		}


		public void Dispose()
		{
			if (mutex != null)
			{
				if (hasHandle)
					mutex.ReleaseMutex();
				mutex.Dispose();
			}
		}

		static bool IsRunningOnMono ()
		{
			return Type.GetType ("Mono.Runtime") != null;
		}
	}
}

