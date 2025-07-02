using System;

namespace Solarmax
{
	public class Framework : Singleton<Framework>, Lifecycle
	{
		public bool Init()
		{
			if (this.IsInitFramework)
			{
				return true;
			}
			if (Singleton<DataProviderSystem>.Instance.InitDelay())
			{
				if (Singleton<DataHandlerSystem>.Instance.Init())
				{
					if (Singleton<LocalStorageSystem>.Instance.Init())
					{
						if (Singleton<OldLocalStorageSystem>.Instance.Init())
						{
							if (Singleton<ThirdPartySystem>.Instance.Init())
							{
								if (Singleton<NetSystem>.Instance.Init())
								{
									if (Singleton<BattleSystem>.Instance.Init())
									{
										Singleton<CollectionModel>.Get().EnsureInit();
										this.IsInitFramework = true;
										return true;
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		public bool InitLanguageAndPing()
		{
			if (Singleton<DataProviderSystem>.Instance.Init())
			{
				if (Singleton<ConfigSystem>.Instance.Init())
				{
					if (Singleton<LoggerSystem>.Instance.Init())
					{
						if (Singleton<TimeSystem>.Instance.Init())
						{
							if (Singleton<EventSystem>.Instance.Init())
							{
								Singleton<LanguageDataProvider>.Get().Load();
								if (Singleton<EngineSystem>.Instance.Init())
								{
									if (MonoSingleton<UpdateSystem>.Instance.Init())
									{
										if (Singleton<UISystem>.Instance.Init())
										{
											return true;
										}
									}
								}
							}
						}
					}
				}
			}
			return false;
		}

		public void Tick(float interval)
		{
			Singleton<ConfigSystem>.Instance.Tick(interval);
			Singleton<LoggerSystem>.Instance.Tick(interval);
			Singleton<DataProviderSystem>.Instance.Tick(interval);
			Singleton<DataHandlerSystem>.Instance.Tick(interval);
			Singleton<TimeSystem>.Instance.Tick(interval);
			Singleton<EventSystem>.Instance.Tick(interval);
			Singleton<EngineSystem>.Instance.Tick(interval);
			MonoSingleton<UpdateSystem>.Instance.Tick(interval);
			Singleton<LocalStorageSystem>.Instance.Tick(interval);
			Singleton<UISystem>.Instance.Tick(interval);
			Singleton<ThirdPartySystem>.Instance.Tick(interval);
			Singleton<NetSystem>.Instance.Tick(interval);
			Singleton<BattleSystem>.Instance.Tick(interval);
			if (Singleton<LocalPlayer>.Get() != null)
			{
				Singleton<LocalPlayer>.Get().Tick(interval);
			}
		}

		public void UpdateRender(float interval)
		{
			Singleton<BattleSystem>.Instance.UpdateRender(interval);
		}

		public void Destroy()
		{
			Singleton<LoggerSystem>.Instance.Debug("Framework destroy begin", new object[0]);
			Singleton<ConfigSystem>.Instance.Destroy();
			Singleton<EventSystem>.Instance.Destroy();
			Singleton<DataProviderSystem>.Instance.Destroy();
			Singleton<DataHandlerSystem>.Instance.Destroy();
			Singleton<TimeSystem>.Instance.Destroy();
			Singleton<EngineSystem>.Instance.Destroy();
			MonoSingleton<UpdateSystem>.Instance.Destroy();
			Singleton<LocalStorageSystem>.Instance.Destroy();
			Singleton<UISystem>.Instance.Destroy();
			Singleton<ThirdPartySystem>.Instance.Destroy();
			Singleton<NetSystem>.Instance.Destroy();
			Singleton<BattleSystem>.Instance.Destroy();
			Singleton<LoggerSystem>.Instance.Debug("Framework destroy end.", new object[0]);
			Singleton<LoggerSystem>.Instance.Destroy();
		}

		public void SetWritableRootDir(string path)
		{
			this.mWritableRootDir = path;
		}

		public string GetWritableRootDir()
		{
			return this.mWritableRootDir;
		}

		private string mWritableRootDir = string.Empty;

		private bool IsInitFramework;
	}
}
