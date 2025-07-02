using System;
using System.Collections.Generic;
using System.IO;

namespace Solarmax
{
	public class DataProviderSystem : Singleton<DataProviderSystem>, Lifecycle
	{
		public bool Init()
		{
			Singleton<LoggerSystem>.Instance.Debug("DataProviderSystem   init   begin", new object[0]);
			this.RegisterDataProvider(Singleton<NameFilterConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<InnateSkillConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<LadderConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<NameConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<RaceSkillConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<SkillConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<UIWindowConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<MapNodeConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<GameVariableConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<NewSkillConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<NewSkillBuffConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<NewSkillEffectConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<GuideDataProvider>.Instance);
			this.RegisterDataProvider(Singleton<LevelConfigConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<CampConfigConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<AIStrategyConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<SkinConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<storeConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<FunctionOpenConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<SeasonRewardProvider>.Instance);
			this.RegisterDataProvider(Singleton<MonthCheckConfgiProvider>.Instance);
			if (!this.Load())
			{
				return false;
			}
			Singleton<LoggerSystem>.Instance.Debug("DataProviderSystem   init   end", new object[0]);
			return true;
		}

		public bool InitDelay()
		{
			this.RegisterDataProvider(Singleton<MapConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<ChapterConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<ChapterAssistConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<TaskConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<AchievementConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<AchievementListProvider>.Instance);
			this.RegisterDataProvider(Singleton<ItemConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<LotteryConfigProvider>.Instance);
			this.RegisterDataProvider(Singleton<LotteryAddProvider>.Instance);
			this.RegisterDataProvider(Singleton<LotteryRotateProvider>.Instance);
			Singleton<MapConfigProvider>.Instance.Load();
			Singleton<ChapterConfigProvider>.Instance.Load();
			Singleton<ChapterAssistConfigProvider>.Instance.Load();
			Singleton<TaskConfigProvider>.Instance.Load();
			Singleton<AchievementConfigProvider>.Instance.Load();
			Singleton<AchievementListProvider>.Instance.Load();
			Singleton<ItemConfigProvider>.Instance.Load();
			Singleton<LotteryConfigProvider>.Instance.Load();
			Singleton<LotteryAddProvider>.Instance.Load();
			Singleton<LotteryRotateProvider>.Instance.Load();
			return true;
		}

		public void Tick(float interval)
		{
		}

		public void Destroy()
		{
			this.mDataProvider.Clear();
		}

		private bool Load()
		{
			for (int i = 0; i < this.mDataProvider.Count; i++)
			{
				IDataProvider dataProvider = this.mDataProvider[i];
				if (dataProvider != null)
				{
					if (!dataProvider.IsXML())
					{
						FileReader.LoadPath(dataProvider.Path());
					}
					dataProvider.Load();
					if (!dataProvider.Verify())
					{
						return false;
					}
					if (!dataProvider.IsXML())
					{
						FileReader.UnLoad();
					}
				}
			}
			return true;
		}

		private void RegisterDataProvider(IDataProvider dataProvider)
		{
			this.mDataProvider.Add(dataProvider);
		}

		public static string FormatDataProviderPath(string datapath)
		{
			return Path.Combine(UtilTools.GetStreamAssetsByPlatform(), datapath);
		}

		public void LoadExtraData(string path)
		{
		}

		private List<IDataProvider> mDataProvider = new List<IDataProvider>();

		private LanguageDataProvider languageProvider = Singleton<LanguageDataProvider>.Instance;
	}
}
