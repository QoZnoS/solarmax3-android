using System;
using System.Collections.Generic;
using Solarmax;
using UnityEngine;

public class CooperationRoomWindow : BaseWindow
{
	public override bool Init()
	{
		base.Init();
		base.RegisterEvent(EventId.UpdateMoney);
		base.RegisterEvent(EventId.OnHaveNewChapterUnlocked);
		base.RegisterEvent(EventId.OnBackWindowName);
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.table = this.LevelRoot.GetComponent<UITable>();
		this.SetPlayerBaseInfo();
		this.DisplayCapthers();
	}

	public override void OnHide()
	{
		this.Cells.Clear();
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
		if (eventId == EventId.UpdateMoney)
		{
			this.playerMoney.text = Solarmax.Singleton<LocalPlayer>.Get().playerData.money.ToString();
		}
		else if (eventId == EventId.OnHaveNewChapterUnlocked)
		{
			string chapter = (string)args[0];
			this.RefreshPayChapter(chapter);
		}
		else if (eventId == EventId.OnBackWindowName)
		{
			this.backWindow = (string)args[0];
		}
	}

	private void ClearTable()
	{
		this.LevelRoot.transform.DestroyChildren();
	}

	private void DisplayCapthers()
	{
		this.LevelRoot.transform.DestroyChildren();
		List<ChapterInfo> coopertionList = Solarmax.Singleton<LevelDataHandler>.Instance.coopertionList;
		for (int i = 0; i < coopertionList.Count; i++)
		{
			GameObject gameObject = this.LevelRoot.AddChild(this.template);
			CooperationWidnowCell component = gameObject.GetComponent<CooperationWidnowCell>();
			ChapterInfo info = coopertionList[i];
			gameObject.name = "num" + i;
			gameObject.SetActive(true);
			component.SetInfo(info);
			this.Cells.Add(gameObject);
		}
		this.table.Reposition();
		this.scrollView.ResetPosition();
	}

	private void SetPlayerBaseInfo()
	{
		if (Solarmax.Singleton<LocalPlayer>.Get().playerData != null)
		{
			this.playerMoney.text = Solarmax.Singleton<LocalPlayer>.Get().playerData.money.ToString();
		}
	}

	public void OnClickBack()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow(this.backWindow);
	}

	private void RefreshPayChapter(string chapter)
	{
		for (int i = 0; i < this.Cells.Count; i++)
		{
			GameObject gameObject = this.Cells[i];
			CooperationWidnowCell component = gameObject.GetComponent<CooperationWidnowCell>();
			ChapterInfo chapterInfo = Solarmax.Singleton<LevelDataHandler>.Instance.coopertionList[i];
			if (chapterInfo.id.Equals(chapter))
			{
				component.SetInfo(chapterInfo);
			}
		}
	}

	public void OnBnSettingsClick()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("SettingWindow");
	}

	public void OnClickAddMoney()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("StoreWindow");
	}

	public GameObject LevelRoot;

	public GameObject template;

	public UIScrollView scrollView;

	public UILabel playerMoney;

	private string backWindow = "HomeWindow";

	private List<GameObject> Cells = new List<GameObject>();

	private UITable table;
}
