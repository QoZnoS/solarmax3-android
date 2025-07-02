using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Solarmax
{
	public class ChapterAssistConfigProvider : Singleton<ChapterAssistConfigProvider>, IDataProvider
	{
		public bool IsXML()
		{
			return true;
		}

		public string Path()
		{
			return "data/CooChapter.xml";
		}

		public void Load()
		{
			this.dataList.Clear();
			try
			{
				string text = LoadResManager.LoadCustomTxt(this.Path());
				if (!string.IsNullOrEmpty(text))
				{
					XElement xelement = XDocument.Parse(text).Element("coos");
					if (xelement != null)
					{
						foreach (XElement element in xelement.Elements("coo"))
						{
							ChapterAssistConfig chapterAssistConfig = new ChapterAssistConfig();
							if (chapterAssistConfig.Load(element))
							{
								this.dataList.Add(chapterAssistConfig);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Singleton<LoggerSystem>.Instance.Error("data/Chapter.xml resource failed " + ex.ToString(), new object[0]);
			}
		}

		public bool Verify()
		{
			return true;
		}

		public List<ChapterAssistConfig> GetAllData()
		{
			return this.dataList;
		}

		public List<ChapterAssistConfig> GetAllData(string chapterID)
		{
			List<ChapterAssistConfig> list = new List<ChapterAssistConfig>();
			for (int i = 0; i < this.dataList.Count; i++)
			{
				if (this.dataList[i].dependChapter == chapterID)
				{
					list.Add(this.dataList[i]);
				}
			}
			return list;
		}

		public ChapterAssistConfig GetData(string id)
		{
			ChapterAssistConfig result = null;
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

		public void LoadExtraData()
		{
		}

		private List<ChapterAssistConfig> dataList = new List<ChapterAssistConfig>();
	}
}
