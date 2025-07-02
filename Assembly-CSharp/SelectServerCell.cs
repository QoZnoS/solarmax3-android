using System;
using Solarmax;
using UnityEngine;

public class SelectServerCell : MonoBehaviour
{
	public void SetInfoLocal(ServerListItemConfig serverCfg, GameObject parent)
	{
		this.selectPanel = parent;
		this.config = serverCfg;
		this.nameLabel.text = this.config.Name;
	}

	public void OnSelectServer()
	{
		if (this.config == null)
		{
			return;
		}
		string name = this.config.Name;
		this.selectPanel.SetActive(false);
		Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.OnSelectServerList, new object[]
		{
			this.config
		});
		MonoSingleton<FlurryAnalytis>.Instance.FlurrySelectServer(name);
		MiGameAnalytics.MiAnalyticsSelectServer(name);
	}

	private GameObject selectPanel;

	public UILabel nameLabel;

	private ServerListItemConfig config;
}
