using System;
using System.Collections;
using UnityEngine;

namespace Solarmax
{
	public class WebRequestor<T> where T : class, IWebResponse
	{
		public WebRequestor(string tag, string[] urls, string subPath, string param, bool encrypt, Action responseCallback)
		{
			this.mTag = tag;
			this.mUrls = urls;
			this.mSubPath = subPath;
			this.mParam = param;
			this.mEncrypt = encrypt;
			this.mOnResponse = responseCallback;
		}

		public T Response { get; private set; }

		private void OnResponseCallback()
		{
			if (this.mOnResponse != null)
			{
				this.mOnResponse();
			}
		}

		private void StartRequestCoroutine()
		{
			global::Coroutine.Start(this.Request());
		}

		private void StartDelayRequestCoroutine()
		{
			global::Coroutine.Start(this.DelayRequest(3f));
		}

		public void StartRequest(int retryTimes)
		{
			this.mRetryTimes = retryTimes;
			this.mCurRetryTimes = 0;
			UpgradeUtil.CheckNetwork(new EventDelegate.Callback(this.StartRequestCoroutine), 0L);
		}

		private IEnumerator DelayRequest(float delay)
		{
			yield return new WaitForSecondsRealtime(delay);
			yield return this.Request();
			yield break;
		}

		public IEnumerator Request()
		{
			this.Response = default(T);
			this.OnResponseCallback();
			yield break;
		}

		private void LogFailed(string url, string errorInfo)
		{
			string eventName = string.Format("{0}Failed", this.mTag);
			string text = string.Format("{0} - {1}", errorInfo, Application.internetReachability);
			MonoSingleton<FlurryAnalytis>.Instance.LogEvent(eventName, "url", url, "info", text);
			Debug.LogErrorFormat("Request url {0} error - {1}!", new object[]
			{
				url,
				text
			});
		}

		private void LogStart(string url, int curRetryTimes)
		{
			string eventName = string.Format("{0}Start", this.mTag);
			MonoSingleton<FlurryAnalytis>.Instance.LogEvent(eventName, "url", url, "retry", curRetryTimes.ToString());
		}

		private IEnumerator Request(string url)
		{
			this.Request();
			yield break;
		}

		private readonly Action mOnResponse;

		private string[] mUrls;

		private readonly string mParam;

		private readonly string mSubPath;

		private readonly bool mEncrypt;

		private int mRetryTimes;

		private int mCurRetryTimes;

		private readonly string mTag = string.Empty;
	}
}
