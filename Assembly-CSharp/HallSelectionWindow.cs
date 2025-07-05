using System;
using Solarmax;

public class HallSelectionWindow : BaseWindow
{
	public override bool Init()
	{
		base.Init();
		return true;
	}

	public override void Release()
	{
	}

	public override void OnShow()
	{
		base.OnShow();
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
	}

	public override void OnHide()
	{
	}

	public void OnBnPvpCooperationClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CooperationRoomWindow");
		Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.OnBackWindowName, new object[]
		{
			"HallWindow"
		});
		Solarmax.Singleton<AudioManger>.Get().PlayEffect("onOpen");
	}

	public void OnBnPvpRoomClick()
	{
		Solarmax.Singleton<UISystem>.Get().HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("CreateRoomWindow");
		Solarmax.Singleton<AudioManger>.Get().PlayEffect("onOpen");
	}

	public void OnBackClick()
	{
		Solarmax.Singleton<UISystem>.Instance.HideAllWindow();
		Solarmax.Singleton<UISystem>.Get().ShowWindow("HallWindow");
	}
}
