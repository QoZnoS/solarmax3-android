using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Solarmax
{
	public class LevelConfigConfigProvider : Singleton<LevelConfigConfigProvider>, IDataProvider
	{
		public string Path()
		{
			return "data/Level.xml";
		}

		public bool IsXML()
		{
			return true;
		}

		public void Load()
		{
			this.dataList.Clear();
			try
			{
				string text = LoadResManager.LoadCustomTxt(this.Path());
				if (!string.IsNullOrEmpty(text))
				{
					XElement xelement = XDocument.Parse(text).Element("levels");
					if (xelement != null)
					{
						foreach (XElement element in xelement.Elements("level"))
						{
							LevelConfig levelConfig = new LevelConfig();
							if (levelConfig.Load(element))
							{
								this.dataList.Add(levelConfig);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Singleton<LoggerSystem>.Instance.Error("data/Level.xml resource failed " + ex.ToString(), new object[0]);
			}
		}

		public bool Verify()
		{
			return true;
		}

		public List<LevelConfig> GetAllData()
		{
			return this.dataList;
		}

		public LevelConfig GetData(string id)
		{
			LevelConfig result = null;
			for (int i = 0; i < this.dataList.Count; i++)
			{
				if (this.dataList[i].id == id)
				{
					result = this.dataList[i];
					break;
				}
			}
			return result;
		}

		public List<LevelConfig> GetOneChapterAllLevel(string chapter)
		{
			List<LevelConfig> list = new List<LevelConfig>();
			foreach (LevelConfig levelConfig in this.dataList)
			{
				if (levelConfig.chapter == chapter)
				{
					list.Add(levelConfig);
				}
			}
			return list;
		}

		public LevelConfig GetDataByChapter(string chapter)
		{
			LevelConfig result = null;
			for (int i = 0; i < this.dataList.Count; i++)
			{
				if (this.dataList[i].chapter == chapter)
				{
					result = this.dataList[i];
					break;
				}
			}
			return result;
		}

		public void LoadExtraData()
		{
		}

		private List<LevelConfig> dataList = new List<LevelConfig>();
	}
}
