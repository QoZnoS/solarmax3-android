using System;
using MiGameSDK;
using UnityEngine;

namespace Solarmax
{
	public class ThirdPartySystem : Singleton<ThirdPartySystem>, Lifecycle
	{
		public bool GettingProductsInfo { get; private set; }

		public bool Init()
		{
			return true;
		}

		public void Tick(float interval)
		{
		}

		public void Destroy()
		{
		}

		public void Login()
		{
			MiGameStatisticSDK.Init();
		}

		public void OnSDKLogin(string account, string token)
		{
			MonoSingleton<FlurryAnalytis>.Instance.SetUserID(account);
			MonoSingleton<BuglyTools>.Instance.SetUserId(account);
			Singleton<LocalAccountStorage>.Get().account = account;
			Singleton<LocalAccountStorage>.Get().webTest = false;
			Singleton<LocalAccountStorage>.Get().token = token;
			Singleton<EventSystem>.Instance.FireEvent(EventId.OnSDKLoginResult, new object[]
			{
				1
			});
		}

		public void OnSDKSwitch(string account, string token)
		{
			MonoSingleton<FlurryAnalytis>.Instance.SetUserID(account);
			MonoSingleton<BuglyTools>.Instance.SetUserId(account);
			Singleton<LocalAccountStorage>.Get().account = account;
			Singleton<LocalAccountStorage>.Get().webTest = false;
			Singleton<LocalAccountStorage>.Get().token = token;
			Singleton<EventSystem>.Instance.FireEvent(EventId.OnSDKLoginResult, new object[]
			{
				1
			});
		}

		public void OnSDKLoginFailed()
		{
			Singleton<EventSystem>.Instance.FireEvent(EventId.OnSDKLoginResult, new object[]
			{
				0
			});
		}

		public void OnSDKOpenPrivacty(string ret)
		{
			Debug.Log("Login -》 OnGetPrivactyResult ret = " + ret);
			if (ret.Equals("1"))
			{
				Singleton<EventSystem>.Instance.FireEvent(EventId.OnGetPrivactyResult, new object[]
				{
					1
				});
			}
			else
			{
				Singleton<EventSystem>.Instance.FireEvent(EventId.OnGetPrivactyResult, new object[]
				{
					0
				});
			}
		}

		public void GetProductsInfo()
		{
			if (!UpgradeUtil.GetGameConfig().EnablePay)
			{
				return;
			}
			if (this.GettingProductsInfo || this.mProductsInfoInited)
			{
				return;
			}
			string[] productIds = Singleton<storeConfigProvider>.Instance.ProductIds;
			if (productIds == null || productIds.Length == 0)
			{
				Debug.LogError("GetProductsInfo - Empty ProductIds");
				return;
			}
			if (!MiPlatformSDK.GetProductsInfo(productIds))
			{
				return;
			}
			MonoSingleton<FlurryAnalytis>.Instance.LogEvent("GetProductInfoStart");
			this.GettingProductsInfo = true;
		}

		public void OnSDKGetProductInfoFailed()
		{
			this.GettingProductsInfo = false;
			Singleton<EventSystem>.Instance.FireEvent(EventId.OnSDKGetProductInfoFailed, new object[0]);
		}

		public void OnSDKGetProductInfoSuccess(SDKProductInfoList list)
		{
			this.GettingProductsInfo = false;
			if (!Singleton<storeConfigProvider>.Get().SetProductInfoList(list))
			{
				return;
			}
			this.mProductsInfoInited = true;
			Singleton<EventSystem>.Instance.FireEvent(EventId.OnSDKGetProductInfoSuccess, new object[0]);
		}

		public string GetChannel()
		{
			string empty = string.Empty;
			return "Unity_Editor";
		}

		private bool mProductsInfoInited;
	}
}
