using System;
using Solarmax;
using UnityEngine;

public class PreviewWindow : BaseWindow
{
	public void Awake()
	{
		this.mapShow.transform.localScale = Vector3.one * 0.6f;
	}

	public override bool Init()
	{
		base.Init();
		return true;
	}

	public override void OnShow()
	{
		base.OnShow();
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.resumingFrame < 0)
		{
			this.startCountTime = 4;
			this.UpdateStartCountTime();
			this.mapId = Solarmax.Singleton<BattleSystem>.Instance.battleData.matchId;
			this.background.gameObject.SetActive(false);
			this.mapShow.Switch(this.mapId, false);
			Solarmax.Singleton<BattleSystem>.Instance.battleData.root.SetActive(false);
			this.SetInfo();
		}
		else
		{
			this.timeLabel.gameObject.SetActive(false);
			this.userNumLabel.gameObject.SetActive(false);
			Solarmax.Singleton<BattleSystem>.Instance.battleData.root.SetActive(false);
			base.Invoke("OnCloseClick", 1f);
		}
	}

	public override void OnHide()
	{
	}

	public override void OnUIEventHandler(EventId eventId, params object[] args)
	{
	}

	private void SetInfo()
	{
		MapConfig data = Solarmax.Singleton<MapConfigProvider>.Instance.GetData(this.mapId);
		this.userNumLabel.text = string.Format("{0} / {1}", data.player_count, data.player_count);
		Team team = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.GetTeam(Solarmax.Singleton<BattleSystem>.Instance.battleData.currentTeam);
		for (int i = 0; i < data.player_count; i++)
		{
			Team team2 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.GetTeam((TEAM)(i + 1));
			for (int j = 0; j < data.mpcList.Count; j++)
			{
				MapPlayerConfig mapPlayerConfig = data.mpcList[j];
				if (mapPlayerConfig.camption == (int)team2.team)
				{
					if (team2 == team)
					{
						UnityEngine.Object resources = Solarmax.Singleton<AssetManager>.Get().GetResources("Effect_Birth");
						GameObject gameObject = UnityEngine.Object.Instantiate(resources) as GameObject;
						gameObject.transform.SetParent(this.mapShow.GetEffectRoot(mapPlayerConfig.tag).transform);
						NGUITools.SetLayer(gameObject, gameObject.transform.parent.gameObject.layer);
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localScale = Vector3.one * 5f;
						Animator component = gameObject.GetComponent<Animator>();
						component.Play("Effect_Birth_in");
					}
					else if (team.IsFriend(team2.groupID))
					{
						UnityEngine.Object resources2 = Solarmax.Singleton<AssetManager>.Get().GetResources("Effect_Birth_Other");
						GameObject gameObject2 = UnityEngine.Object.Instantiate(resources2) as GameObject;
						gameObject2.transform.SetParent(this.mapShow.GetEffectRoot(mapPlayerConfig.tag).transform);
						NGUITools.SetLayer(gameObject2, gameObject2.transform.parent.gameObject.layer);
						gameObject2.transform.localPosition = Vector3.zero;
						gameObject2.transform.localScale = Vector3.one * 5f;
						Animator component2 = gameObject2.GetComponent<Animator>();
						component2.Play("Effect_Birth_in");
					}
				}
			}
		}
	}

	private void UpdateStartCountTime()
	{
		this.startCountTime--;
		if (this.startCountTime > 0)
		{
			this.timeLabel.text = string.Format("{0}", this.startCountTime);
			Solarmax.Singleton<AudioManger>.Get().PlayEffect("click");
			this.timeLabel.transform.parent.gameObject.SetActive(true);
		}
		else
		{
			this.timeLabel.transform.parent.gameObject.SetActive(false);
		}
		if (this.startCountTime <= 0)
		{
			this.OnCloseClick();
		}
		else
		{
			base.Invoke("UpdateStartCountTime", 1f);
		}
	}

	public void OnCloseClick()
	{
		TweenAlpha tweenAlpha = base.gameObject.GetComponent<TweenAlpha>();
		if (tweenAlpha == null)
		{
			tweenAlpha = base.gameObject.AddComponent<TweenAlpha>();
		}
		tweenAlpha.ResetToBeginning();
		tweenAlpha.from = 1f;
		tweenAlpha.to = 0f;
		tweenAlpha.duration = 0.25f;
		tweenAlpha.SetOnFinished(delegate()
		{
			this.StartGame();
		});
		tweenAlpha.Play(true);
	}

	private void StartGame()
	{
		GameType gameType = Solarmax.Singleton<BattleSystem>.Instance.battleData.gameType;
		GameState gameState = Solarmax.Singleton<BattleSystem>.Instance.battleData.gameState;
		Solarmax.Singleton<AudioManger>.Get().PlayEffect("startBattle");
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.isReplay)
		{
			Solarmax.Singleton<UISystem>.Get().ShowWindow("ReplayBattleWindow");
		}
		else if (gameType == GameType.PVP || gameType == GameType.League)
		{
			if (Solarmax.Singleton<BattleSystem>.Instance.battleData.gameState == GameState.Watcher)
			{
				Solarmax.Singleton<UISystem>.Get().ShowWindow("ReplayBattleWindow");
				Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.ShowForWatchMode, new object[0]);
			}
			else
			{
				Solarmax.Singleton<UISystem>.Get().ShowWindow("BattleWindow");
			}
		}
		else if (gameType == GameType.Single || gameType == GameType.PayLevel || gameType == GameType.TestLevel)
		{
			Solarmax.Singleton<UISystem>.Get().ShowWindow("BattleWidnow_off");
		}
		Solarmax.Singleton<UISystem>.Get().FadeBattle(true, new EventDelegate(delegate()
		{
			Solarmax.Singleton<BattleSystem>.Instance.StartLockStep();
		}));
		Solarmax.Singleton<ShipFadeManager>.Get().SetShipAlpha(0f);
		Solarmax.Singleton<UISystem>.Get().HideWindow("PreviewWindow");
		Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.NoticeSelfTeam, new object[0]);
	}

	public UITexture background;

	public UILabel timeLabel;

	public UILabel userNumLabel;

	public MapShow mapShow;

	private int startCountTime;

	private string mapId;
}
