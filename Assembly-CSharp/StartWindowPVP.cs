using System;
using NetMessage;
using Solarmax;
using UnityEngine;

public class StartWindowPVP : BaseWindow
{
	private void Awake()
	{
		this.connectedNum = 0;
	}

	public override bool Init()
	{
		base.Init();
		base.RegisterEvent(EventId.RequestUserResult);
		base.RegisterEvent(EventId.OnRenameFinished);
		base.RegisterEvent(EventId.UpdatePower);
		base.RegisterEvent(EventId.UpdateMoney);
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.SetPlayerInfo();
		global::Singleton<AudioManger>.Get().PlayAudioBG("Empty", 0.5f);
		string[] array = base.gameObject.name.Split(new char[]
		{
			'('
		});
		GuideManager.StartGuide(GuildCondition.GC_Ui, array[0], base.gameObject);
		this.taskMark.SetActive(false);
		if (Solarmax.Singleton<LevelDataHandler>.Get().HaveLevelTaskCompleted() || Solarmax.Singleton<TaskConfigProvider>.Get().HaveDailyTaskCompleted())
		{
			this.taskMark.SetActive(true);
		}
		if (this.userIconTexture != null)
		{
			this.userIconTexture.picUrl = global::Singleton<LocalPlayer>.Get().playerData.icon;
		}
	}

	public override void OnHide()
	{
		base.StopCoroutine("LoginServer");
		GuideManager.ClearGuideData();
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
		if (eventId != EventId.UpdatePower)
		{
			if (eventId == EventId.UpdateMoney)
			{
				this.scoreLabel.text = global::Singleton<LocalPlayer>.Get().playerData.money.ToString();
			}
		}
		else
		{
			this.powerLabel.text = this.FormatPower();
		}
	}

	private void SetPlayerInfo()
	{
		if (string.IsNullOrEmpty(global::Singleton<LocalPlayer>.Get().playerData.name))
		{
			this.userNameLabel.text = "guest";
		}
		else
		{
			this.userNameLabel.text = global::Singleton<LocalPlayer>.Get().playerData.name;
		}
		this.powerLabel.text = this.FormatPower();
		this.scoreLabel.text = global::Singleton<LocalPlayer>.Get().playerData.money.ToString();
		if (Solarmax.Singleton<NetSystem>.Instance.GetConnector().GetConnectStatus() != ConnectionStatus.CONNECTED)
		{
			this.userScore.text = LanguageDataProvider.GetValue(1115);
		}
		else
		{
			this.userScore.text = global::Singleton<LocalPlayer>.Get().playerData.score.ToString();
		}
	}

	private string FormatPower()
	{
		int power = global::Singleton<LocalPlayer>.Get().playerData.power;
		string empty = string.Empty;
		return string.Format("{0} / 30", power);
	}

	private void PlayAnimation(string state)
	{
	}

	public void JoinGameOnClicked()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("PvPRoomWindow");
	}

	public void JoinGame()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Ladder, string.Empty, string.Empty, CooperationType.CT_1v1v1v1, 4, false, string.Empty, -1, string.Empty, false);
		GuideManager.TriggerGuidecompleted(GuildEndEvent.startpvp);
	}

	public void OnSettingClick()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("SettingWindow");
	}

	public void OnBackClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow(new ShowWindowParams("LobbyWindowView", EventId.UpdateChaptersWindow, new object[0]));
	}

	public void OnRewardClick()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("FriendWindow");
	}

	public void OnRankClick()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("RankWindow");
	}

	public void OnRecordClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("ReplayWindow");
	}

	public void OnRoomClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CreateRoomWindow");
	}

	public void OnCooperationClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CooperationRoomWindow");
	}

	public void OnCustomPlayerHead()
	{
		int userId = global::Singleton<LocalPlayer>.Get().playerData.userId;
		if (userId > 0)
		{
			Solarmax.Singleton<NetSystem>.Instance.helper.FriendSearch(string.Empty, userId, 0);
		}
	}

	public void OnBreakThroughMode()
	{
	}

	public void OnClickTask()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("TaskWindow");
	}

	public void OnClickFriends()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("FriendWindow");
	}

	public void OnPlayerFAniamionEnd()
	{
	}

	public void OnClickAddPower()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CommonDialogWindow");
		Solarmax.Singleton<UISystem>.Get().OnEventHandler(33, "CommonDialogWindow", new object[]
		{
			2,
			LanguageDataProvider.GetValue(201),
			new EventDelegate(new EventDelegate.Callback(this.AddPower))
		});
	}

	public void AddPower()
	{
	}

	public NetTexture userIconTexture;

	public UILabel userNameLabel;

	public UILabel scoreLabel;

	public UILabel goldLabel;

	public UILabel powerLabel;

	public UILabel userScore;

	public GameObject taskMark;

	private int connectedNum;
}
