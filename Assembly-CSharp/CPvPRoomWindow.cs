using System;
using NetMessage;
using Solarmax;
using UnityEngine;

public class CPvPRoomWindow : BaseWindow
{
	public override bool Init()
	{
		base.Init();
		base.RegisterEvent(EventId.UpdateMoney);
		base.RegisterEvent(EventId.UpdateAccumulMoney);
		base.RegisterEvent(EventId.OnUpdateLeagueMode);
		base.RegisterEvent(EventId.OnSeasonReward);
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.SetPlayerBaseInfo();
		this.SetSublabelModel(Solarmax.Singleton<LocalPvpSeasonSystem>.Get().pvpType, Solarmax.Singleton<LocalPvpSeasonSystem>.Get().seasonStart, Solarmax.Singleton<LocalPvpSeasonSystem>.Get().seasonEnd);
		if (!Solarmax.Singleton<LocalPlayer>.Get().IsInSeason())
		{
			this.seasonTip2.text = LanguageDataProvider.GetValue(1162);
		}
		this.RefreshMoneyUI(Solarmax.Singleton<LocalPlayer>.Get().nCurAccumulMoney, Solarmax.Singleton<LocalPlayer>.Get().nMaxAccumulMoney);
	}

	public override void OnHide()
	{
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
		if (eventId == EventId.UpdateMoney)
		{
			this.playerMoney.text = Solarmax.Singleton<LocalPlayer>.Get().playerData.money.ToString();
		}
		else if (eventId == EventId.UpdateAccumulMoney)
		{
			this.RefreshMoneyUI(Solarmax.Singleton<LocalPlayer>.Get().nCurAccumulMoney, Solarmax.Singleton<LocalPlayer>.Get().nMaxAccumulMoney);
		}
		else if (eventId == EventId.OnUpdateLeagueMode)
		{
			this.SetPlayerBaseInfo();
			this.SetSublabelModel(Solarmax.Singleton<LocalPvpSeasonSystem>.Get().pvpType, Solarmax.Singleton<LocalPvpSeasonSystem>.Get().seasonStart, Solarmax.Singleton<LocalPvpSeasonSystem>.Get().seasonEnd);
			this.RefreshMoneyUI(Solarmax.Singleton<LocalPlayer>.Get().nCurAccumulMoney, Solarmax.Singleton<LocalPlayer>.Get().nMaxAccumulMoney);
			SeasonRewardView component = this.seasonReward.GetComponent<SeasonRewardView>();
			component.UpdateUI();
		}
		else if (eventId == EventId.OnSeasonReward)
		{
			int num = (int)args[0];
			SeasonRewardModel seasonRewardModel = Solarmax.Singleton<SeasonRewardModel>.Get();
			seasonRewardModel.rewardStatus[num.ToString()] = true;
			SeasonRewardView component2 = this.seasonReward.GetComponent<SeasonRewardView>();
			component2.RefreshUI();
		}
	}

	private bool IsCanClick()
	{
		DateTime? dateTime = this.searchFriendTime;
		if (dateTime != null)
		{
			TimeSpan timeSpan = Solarmax.Singleton<TimeSystem>.Instance.GetServerTime() - this.searchFriendTime.Value;
			if (timeSpan.TotalSeconds < 5.0)
			{
				string message = string.Format(LanguageDataProvider.GetValue(1147), 5 - timeSpan.Seconds);
				Tips.Make(message);
				return false;
			}
		}
		this.searchFriendTime = new DateTime?(Solarmax.Singleton<TimeSystem>.Instance.GetServerTime());
		return true;
	}

	public void OnFourClick()
	{
		if (!this.IsCanClick())
		{
			return;
		}
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_1v1v1v1, 4, true, string.Empty, -1, string.Empty, false);
	}

	public void OnThreeClick()
	{
		if (!this.IsCanClick())
		{
			return;
		}
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_1v1v1, 3, true, string.Empty, -1, string.Empty, false);
	}

	public void OnOneClick()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_1v1, 2, true, string.Empty, -1, string.Empty, false);
	}

	public void OnTwoClick()
	{
		if (!this.IsCanClick())
		{
			return;
		}
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_2v2, 4, false, string.Empty, -1, string.Empty, false);
	}

	public void OnTwoClickEx()
	{
		if (!this.IsCanClick())
		{
			return;
		}
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_2v2, 4, true, string.Empty, -1, string.Empty, false);
	}

	public void OnBackClick()
	{
		Solarmax.Singleton<UISystem>.Instance.HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("HomeWindow");
	}

	private void SetPlayerBaseInfo()
	{
		if (Solarmax.Singleton<LocalPlayer>.Get().playerData != null)
		{
			if (string.IsNullOrEmpty(Solarmax.Singleton<LocalPlayer>.Get().playerData.name))
			{
				this.playerName.text = "无名喵喵";
			}
			else
			{
				this.playerName.text = Solarmax.Singleton<LocalPlayer>.Get().playerData.name;
			}
			this.playerMoney.text = Solarmax.Singleton<LocalPlayer>.Get().playerData.money.ToString();
			int score = Solarmax.Singleton<LocalPlayer>.Get().playerData.score;
			this.playerScore.text = score.ToString();
		}
	}

	private void SetSublabelModel(int nType, int starDay, int endDay)
	{
		this.seasonTime.SetActive(false);
		foreach (UIButton uibutton in this.buttonArray)
		{
			uibutton.enabled = false;
			uibutton.onClick.Clear();
			uibutton.gameObject.SetActive(true);
		}
		foreach (GameObject gameObject in this.secondButton)
		{
			gameObject.SetActive(false);
		}
		foreach (UILabel uilabel in this.titleLable)
		{
			uilabel.gameObject.SetActive(true);
		}
		long num = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
		if (num >= (long)starDay && num <= (long)endDay)
		{
			DateTime dateTime = new DateTime(1970, 1, 1);
			DateTime dateTime2 = dateTime.AddSeconds((double)endDay);
			string text = dateTime2.ToLocalTime().ToString();
			DateTime dateTime3 = new DateTime(1970, 1, 1);
			string str = dateTime3.AddSeconds((double)starDay).ToLocalTime().ToString("yyyy.MM.dd");
			string str2 = dateTime2.ToLocalTime().ToString("yyyy.MM.dd");
			this.seasonTime.SetActive(true);
			UILabel component = this.seasonTime.transform.Find("date").GetComponent<UILabel>();
			component.text = str + "-" + str2;
			foreach (UIButton uibutton2 in this.buttonArray)
			{
				uibutton2.enabled = true;
			}
			if (nType == 3)
			{
				this.titleLable[0].text = LanguageDataProvider.GetValue(2035);
				this.titleLable[1].text = LanguageDataProvider.GetValue(2143);
				this.titleLable[2].text = LanguageDataProvider.GetValue(2144);
				this.titleLable[3].text = LanguageDataProvider.GetValue(2072);
				this.buttonArray[0].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnFourClick)));
				this.buttonArray[1].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOneClick)));
				this.buttonArray[3].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnThreeClick)));
				this.buttonArray[2].onClick.Add(new EventDelegate(delegate()
				{
					this.secondButton[2].SetActive(true);
					this.titleLable[2].gameObject.SetActive(false);
				}));
				this.marks[0].spriteName = this.bigMarkNames[3];
				this.marks[1].spriteName = this.smallMarkNames[0];
				this.marks[2].spriteName = this.smallMarkNames[1];
				this.marks[3].spriteName = this.smallMarkNames[2];
				this.seasonNotice.text = LanguageDataProvider.Format(2216, new object[]
				{
					LanguageDataProvider.GetValue(2035)
				});
			}
			if (nType == 2)
			{
				this.titleLable[0].text = LanguageDataProvider.GetValue(2072);
				this.titleLable[1].text = LanguageDataProvider.GetValue(2143);
				this.titleLable[2].text = LanguageDataProvider.GetValue(2144);
				this.titleLable[3].text = LanguageDataProvider.GetValue(2035);
				this.buttonArray[1].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOneClick)));
				this.buttonArray[0].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnThreeClick)));
				this.buttonArray[3].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnFourClick)));
				this.buttonArray[2].onClick.Add(new EventDelegate(delegate()
				{
					this.secondButton[2].SetActive(true);
					this.titleLable[2].gameObject.SetActive(false);
				}));
				this.marks[0].spriteName = this.bigMarkNames[2];
				this.marks[1].spriteName = this.smallMarkNames[0];
				this.marks[2].spriteName = this.smallMarkNames[1];
				this.marks[3].spriteName = this.smallMarkNames[3];
				this.seasonNotice.text = LanguageDataProvider.Format(2216, new object[]
				{
					LanguageDataProvider.GetValue(2072)
				});
			}
			if (nType == 0)
			{
				this.titleLable[0].text = LanguageDataProvider.GetValue(2143);
				this.titleLable[1].text = LanguageDataProvider.GetValue(2144);
				this.titleLable[2].text = LanguageDataProvider.GetValue(2072);
				this.titleLable[3].text = LanguageDataProvider.GetValue(2035);
				this.buttonArray[0].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOneClick)));
				this.buttonArray[2].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnThreeClick)));
				this.buttonArray[3].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnFourClick)));
				this.buttonArray[1].onClick.Add(new EventDelegate(delegate()
				{
					this.secondButton[1].SetActive(true);
					this.titleLable[1].gameObject.SetActive(false);
				}));
				this.marks[0].spriteName = this.bigMarkNames[0];
				this.marks[1].spriteName = this.smallMarkNames[1];
				this.marks[2].spriteName = this.smallMarkNames[2];
				this.marks[3].spriteName = this.smallMarkNames[3];
				this.seasonNotice.text = LanguageDataProvider.Format(2216, new object[]
				{
					LanguageDataProvider.GetValue(2143)
				});
			}
			if (nType == 1)
			{
				this.titleLable[0].text = LanguageDataProvider.GetValue(2144);
				this.titleLable[1].text = LanguageDataProvider.GetValue(2143);
				this.titleLable[2].text = LanguageDataProvider.GetValue(2072);
				this.titleLable[3].text = LanguageDataProvider.GetValue(2035);
				this.buttonArray[0].onClick.Add(new EventDelegate(delegate()
				{
					this.secondButton[0].SetActive(true);
					this.titleLable[0].gameObject.SetActive(false);
				}));
				this.buttonArray[1].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOneClick)));
				this.buttonArray[2].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnThreeClick)));
				this.buttonArray[3].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnFourClick)));
				this.marks[0].spriteName = this.bigMarkNames[1];
				this.marks[1].spriteName = this.smallMarkNames[0];
				this.marks[2].spriteName = this.smallMarkNames[2];
				this.marks[3].spriteName = this.smallMarkNames[3];
				this.seasonNotice.text = LanguageDataProvider.Format(2216, new object[]
				{
					LanguageDataProvider.GetValue(2144)
				});
			}
		}
		else
		{
			this.seasonTime.SetActive(false);
			foreach (UIButton uibutton3 in this.buttonArray)
			{
				uibutton3.enabled = true;
			}
			this.titleLable[0].text = LanguageDataProvider.GetValue(2035);
			this.titleLable[1].text = LanguageDataProvider.GetValue(2143);
			this.titleLable[2].text = LanguageDataProvider.GetValue(2144);
			this.titleLable[3].text = LanguageDataProvider.GetValue(2072);
			this.buttonArray[0].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnFourClick)));
			this.buttonArray[1].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnOneClick)));
			this.buttonArray[3].onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnThreeClick)));
			this.buttonArray[2].onClick.Add(new EventDelegate(delegate()
			{
				this.secondButton[2].SetActive(true);
				this.titleLable[2].gameObject.SetActive(false);
			}));
			this.marks[0].spriteName = this.bigMarkNames[3];
			this.marks[1].spriteName = this.smallMarkNames[0];
			this.marks[2].spriteName = this.smallMarkNames[1];
			this.marks[3].spriteName = this.smallMarkNames[2];
			this.seasonNotice.text = LanguageDataProvider.GetValue(2217);
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

	public void OnClickAvatar()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CollectionWindow");
		Solarmax.Singleton<AudioManger>.Get().PlayEffect("onOpen");
	}

	private void RefreshMoneyUI(int nCurMoney, int nMaxMoney)
	{
		string text = string.Format(LanguageDataProvider.GetValue(2262), nCurMoney, nMaxMoney);
		this.accumulation.text = text;
	}

	public NetTexture playerIcon;

	public UILabel playerName;

	public UILabel playerMoney;

	public UILabel playerScore;

	public UILabel[] titleLable;

	public UILabel month;

	public UILabel day;

	public UIButton[] buttonArray;

	public GameObject seasonTime;

	public UILabel seasonNotice;

	public UILabel seasonTip2;

	public GameObject[] secondButton;

	public UISprite[] marks;

	public GameObject seasonReward;

	public UILabel accumulation;

	private string[] bigMarkNames = new string[]
	{
		"Signal1",
		"Signal2",
		"Signal3",
		"Signal4"
	};

	private string[] smallMarkNames = new string[]
	{
		"Signal1_s",
		"Signal2_s",
		"Signal3_s",
		"Signal4_s"
	};

	private DateTime? searchFriendTime;
}
