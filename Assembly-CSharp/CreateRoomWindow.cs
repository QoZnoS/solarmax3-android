using System;
using System.Text;
using NetMessage;
using Solarmax;
using UnityEngine;

public class CreateRoomWindow : BaseWindow
{
	public override bool Init()
	{
		base.Init();
		base.RegisterEvent(EventId.OnStartMatchResult);
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		this.playerMoney.text = global::Singleton<LocalPlayer>.Get().playerData.money.ToString();
		this.upGo.SetActive(true);
		this.centerGo.SetActive(true);
		this.numPage.SetActive(false);
	}

	public override void OnHide()
	{
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
		if (eventId == EventId.OnStartMatchResult)
		{
			ErrCode errCode = (ErrCode)args[0];
			if (errCode == ErrCode.EC_MatchIsFull)
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.GetValue(902), 1f);
			}
			else if (errCode == ErrCode.EC_MatchIsMember)
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.GetValue(903), 1f);
			}
			else if (errCode == ErrCode.EC_NotInMatch)
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.GetValue(1142), 1f);
			}
			else if (errCode == ErrCode.EC_NotMaster)
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.GetValue(905), 1f);
			}
			else if (errCode == ErrCode.EC_RoomNotExist)
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.GetValue(906), 1f);
			}
			else
			{
				Tips.Make(Tips.TipsType.FlowUp, LanguageDataProvider.Format(901, new object[]
				{
					errCode
				}), 1f);
			}
		}
	}

	public void OnFourClick()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Room, string.Empty, string.Empty, CooperationType.CT_1v1v1v1, 4, false, string.Empty, -1, string.Empty, false);
	}

	public void OnThreeClick()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Room, string.Empty, string.Empty, CooperationType.CT_1v1v1, 3, false, string.Empty, -1, string.Empty, false);
	}

	public void OnOneClick()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Room, string.Empty, string.Empty, CooperationType.CT_1v1, 2, false, string.Empty, -1, string.Empty, false);
	}

	public void OnTwoClick()
	{
		Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Room, string.Empty, string.Empty, CooperationType.CT_2v2, 4, false, string.Empty, -1, string.Empty, false);
	}

	public void OnAddClick()
	{
		for (int i = 0; i < this.numPageValues.Length; i++)
		{
			this.numPageValues[i].text = string.Empty;
		}
		this.numPage.SetActive(true);
		this.numPageEnterIndex = 0;
	}

	public void OnNumPadClick()
	{
		string name = UIButton.current.gameObject.name;
		int num = 0;
		if (!int.TryParse(name, out num))
		{
			return;
		}
		if (num >= 0)
		{
			if (this.numPageEnterIndex < 4)
			{
				this.numPageValues[this.numPageEnterIndex].text = name;
				this.numPageEnterIndex++;
			}
		}
		else if (this.numPageEnterIndex > 0)
		{
			this.numPageEnterIndex--;
			this.numPageValues[this.numPageEnterIndex].text = string.Empty;
		}
		if (this.numPageEnterIndex >= 4)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.numPageValues.Length; i++)
			{
				stringBuilder.Append(this.numPageValues[i].text);
			}
			string text = stringBuilder.ToString();
			if (this.lastEnterRoomId.Equals(text))
			{
				return;
			}
			this.lastEnterRoomId = text;
			Solarmax.Singleton<NetSystem>.Instance.helper.StartMatchReq(MatchType.MT_Room, text, string.Empty, CooperationType.CT_Null, 0, false, string.Empty, -1, string.Empty, false);
		}
	}

	public void OnNumPageClose()
	{
		this.numPage.SetActive(false);
		this.upGo.SetActive(true);
		this.centerGo.SetActive(true);
	}

	public void OnBackClick()
	{
		Solarmax.Singleton<UISystem>.Instance.HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("HallWindow");
	}

	public void OnBnSettingsClick()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("SettingWindow");
	}

	public void OnClickAddMoney()
	{
		Solarmax.Singleton<UISystem>.Get().ShowWindow("StoreWindow");
	}

	public GameObject upGo;

	public GameObject centerGo;

	public GameObject numPage;

	public UILabel[] numPageValues;

	public UILabel playerMoney;

	private int numPageEnterIndex;

	private string lastEnterRoomId = string.Empty;
}
