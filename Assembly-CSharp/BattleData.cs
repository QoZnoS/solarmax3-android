using System;
using NetMessage;
using Plugin;
using Solarmax;
using UnityEngine;

public class BattleData : Lifecycle2
{
	public BattleData()
	{
		this.rand = new Rand(0);
	}

	public bool vertical
	{
		get
		{
			return false;
		}
		set
		{
			if (this.currentTable == null)
			{
				return;
			}
			this.currentTable.vertical = value;
		}
	}

	public int currentPlayers
	{
		get
		{
			return (this.currentTable == null) ? 0 : this.currentTable.player_count;
		}
		set
		{
			if (this.currentTable == null)
			{
				return;
			}
			this.currentTable.player_count = value;
		}
	}

	public string mapAudio
	{
		get
		{
			return (this.currentTable == null) ? null : this.currentTable.audio;
		}
		set
		{
			if (this.currentTable == null)
			{
				return;
			}
			this.currentTable.audio = value;
		}
	}

	public string defaultAI
	{
		get
		{
			return (this.currentTable == null) ? null : this.currentTable.defaultai;
		}
		set
		{
			if (this.currentTable == null)
			{
				return;
			}
			this.currentTable.defaultai = value;
		}
	}

	public string teamAI
	{
		get
		{
			return (this.currentTable == null) ? null : this.currentTable.teamAITypes;
		}
		set
		{
			if (this.currentTable == null)
			{
				return;
			}
			this.currentTable.teamAITypes = value;
		}
	}

	public bool Init()
	{
		this.gameState = GameState.Init;
		this.gameType = GameType.SingleLevel;
		this.matchId = string.Empty;
		this.currentTable = null;
		this.currentTeam = TEAM.Neutral;
		this.winTEAM = TEAM.Neutral;
		this.sliderRate = 1f;
		this.sliderNumber = -1f;
		this.isFakeBattle = false;
		this.isReplay = false;
		this.teamFight = false;
		this.isResumeBattle = false;
		this.silent = false;
		this.aiStrategy = -1;
		int[] array = new int[]
		{
			0,
			1,
			2,
			3
		};
		this.pvpUserToTeamIndex = new int[]
		{
			0,
			1,
			2,
			3
		};
		this.battleTime = 0f;
		this.voiceRoomId = null;
		this.voiceRoomToken = null;
		if (this.gameType == GameType.SingleLevel || this.gameType == GameType.GuildeLevel)
		{
			global::Singleton<AchievementManager>.Get().Init(this);
		}
		return true;
	}

	public void Tick(int frame, float interval)
	{
		Solarmax.Singleton<EffectManager>.Get().LaserGunTick(frame, interval);
		if (this.gameType == GameType.SingleLevel || this.gameType == GameType.GuildeLevel)
		{
			this.battleTime += interval;
			global::Singleton<AchievementManager>.Get().BattleTick(interval);
		}
	}

	public void Destroy()
	{
	}

	public GameObject root;

	public GameState gameState;

	public GameType gameType = GameType.PVP;

	public BattlePlayType battleType = BattlePlayType.Normalize;

	public MatchType matchType;

	public CooperationType battleSubType = CooperationType.CT_Null;

	public Rand rand;

	public string matchId;

	public long battleId;

	public int difficultyLevel;

	public int aiLevel;

	public int aiStrategy;

	public int aiParam;

	public string dyncDiffType;

	public string winType;

	public string winTypeParam1;

	public string winTypeParam2;

	public string loseType;

	public string loseTypeParam1;

	public string loseTypeParam2;

	public int[] pvpUserToTeamIndex;

	public MapConfig currentTable;

	public TEAM currentTeam;

	public TEAM winTEAM;

	public float sliderRate;

	public float sliderNumber;

	public bool isFakeBattle;

	public bool isReplay;

	public bool teamFight;

	public bool useAI = true;

	public bool useCommonEndCondition;

	public bool runWithScript;

	public bool isLevelReplay;

	public float battleTime;

	public bool isResumeBattle;

	public string voiceRoomId;

	public int[] voiceRoomToken;

	public bool mapEdit;

	public bool silent;

	public int resumingFrame = -1;
}
