using System;
using System.Collections.Generic;
using Solarmax;
using UnityEngine;

public abstract class Node : Lifecycle2
{
	public Node(string name)
	{
		for (int i = 0; i < this.shipsRelifeTag.Length; i++)
		{
			this.shipsRelifeTag[i] = -1;
		}
		this.tag = name;
		this.nodeIsHide = false;
		this.shipMap = new Dictionary<TEAM, List<Ship>>();
		for (int j = 0; j < this.attribute.Length; j++)
		{
			this.attribute[j] = new AttributeObject();
		}
		this.skillBuffLogic = new NewSkillBuffLogic();
	}

	public string tag
	{
		get
		{
			return this.nodeTag;
		}
		set
		{
			this.nodeTag = value;
			if (this.entity != null)
			{
				this.entity.ReName(string.Format("{1}-{0}", this.type.ToString(), this.nodeTag));
			}
		}
	}

	public NodeManager nodeManager { get; set; }

	public int bomber { get; set; }

	public virtual NodeType type
	{
		get
		{
			return NodeType.None;
		}
	}

	public TEAM team
	{
		get
		{
			if (this.currentTeam == null)
			{
				return TEAM.Neutral;
			}
			return this.currentTeam.team;
		}
	}

	public Team currentTeam
	{
		get
		{
			return this.realTeam;
		}
		set
		{
			this.SetRealTeam(value, false);
		}
	}

	public int aistrategy
	{
		get
		{
			return this.nodeAIStrategy;
		}
		set
		{
			this.nodeAIStrategy = value;
			this.aiActions.Clear();
			this.aiActions = Solarmax.Singleton<AIStrategyConfigProvider>.Instance.GetAIActions(this.nodeAIStrategy);
		}
	}

	public void SetRealTeam(Team value, bool initNode = false)
	{
		if (this.realTeam == value)
		{
			return;
		}
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(this.nodeManager.sceneManager.battleData.currentTeam);
		if (this.realTeam != null)
		{
			this.realTeam.SetAttributeBase(TeamAttr.PopulationMax, (float)(this.realTeam.GetAttributeBaseInt(TeamAttr.PopulationMax) - this.population));
			if (team == this.realTeam)
			{
				Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.OnPopulationDown, new object[]
				{
					this.realTeam.current,
					this.realTeam.currentMax,
					this.population
				});
			}
		}
		this.realTeam = value;
		if (this.realTeam != null)
		{
			this.realTeam.SetAttributeBase(TeamAttr.PopulationMax, (float)(this.realTeam.GetAttributeBaseInt(TeamAttr.PopulationMax) + this.population));
			if (team == this.realTeam)
			{
				Solarmax.Singleton<EventSystem>.Instance.FireEvent(EventId.OnPopulationUp, new object[]
				{
					this.realTeam.current,
					this.realTeam.currentMax,
					this.population
				});
			}
		}
		if (this.entity == null)
		{
			return;
		}
		if (this.type != NodeType.BarrierLine)
		{
			this.entity.SetColor(this.realTeam.color);
		}
		if (!initNode)
		{
			if (Solarmax.Singleton<BattleSystem>.Instance.battleData.isFakeBattle)
			{
				this.SetGlowColor(this.realTeam.color);
			}
			else
			{
				Solarmax.Singleton<EffectManager>.Get().AddGlow(this, this.realTeam.color);
			}
			if (this.realTeam.team == TEAM.Neutral)
			{
				this.entity.SetHaloColor(Color.white);
			}
			this.entity.SetAlpha(1f);
			return;
		}
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.isFakeBattle)
		{
			this.SetGlowColor(this.realTeam.color);
		}
		if (this.type != NodeType.BarrierLine && (this.realTeam.team == TEAM.Neutral || this.realTeam.team == TEAM.Team_5 || this.realTeam.team == TEAM.Team_6))
		{
			this.entity.SetHaloColor(Color.white);
		}
	}

	public void SetBasePopulation(int Pop)
	{
		if (this.realTeam != null)
		{
			this.realTeam.SetAttributeBase(TeamAttr.PopulationMax, (float)(this.realTeam.GetAttributeBaseInt(TeamAttr.PopulationMax) - this.population));
		}
		this.SetAttributeBase(NodeAttr.Poplation, (float)Pop);
		if (this.realTeam != null)
		{
			this.realTeam.SetAttributeBase(TeamAttr.PopulationMax, (float)(this.realTeam.GetAttributeBaseInt(TeamAttr.PopulationMax) + this.population));
		}
	}

	private Team realTeam { get; set; }

	public NodeState state
	{
		get
		{
			return this.nodeState;
		}
		set
		{
			if (value == this.nodeState)
			{
				return;
			}
			if (this.state != NodeState.Battle)
			{
				this.lastNodeState = this.nodeState;
			}
			this.ModifyState(this.nodeState, value);
			this.nodeState = value;
		}
	}

	public float hp { get; set; }

	public float hpMax
	{
		get
		{
			return this.GetAttributeFloat(NodeAttr.HpMax);
		}
	}

	public float AttackRage
	{
		get
		{
			return this.GetAttributeFloat(NodeAttr.AttackRange);
		}
	}

	public float AttackSpeed
	{
		get
		{
			return this.GetAttributeFloat(NodeAttr.AttackSpeed);
		}
	}

	public float SkillSpeed
	{
		get
		{
			return this.GetAttributeFloat(NodeAttr.SkillSpeed);
		}
	}

	public float AttackPower
	{
		get
		{
			return this.GetAttributeFloat(NodeAttr.AttackPower);
		}
	}

	public int population
	{
		get
		{
			return this.GetAttributeInt(NodeAttr.Poplation);
		}
	}

	private Dictionary<TEAM, List<Ship>> shipMap { get; set; }

	public virtual bool Init()
	{
		this.InitNode();
		this.InitNodeTouch();
		this.skillBuffLogic.Init();
		for (int i = 0; i < this.attribute.Length; i++)
		{
			this.attribute[i].Reset();
		}
		this.SetAttributeBase(NodeAttr.OccupiedSpeed, 1f);
		this.SetAttributeBase(NodeAttr.ProduceSpeed, 1f);
		return true;
	}

	public virtual void Tick(int frame, float interval)
	{
		if (this.entity != null)
		{
			this.entity.Tick(frame, interval);
		}
		this.UpdateHideStatus();
		this.UpdateAiTimers(frame, interval);
		this.UpdateRevolution(frame, interval);
		this.skillBuffLogic.Tick(frame, interval);
	}

	public virtual void Destroy()
	{
		if (this.hud != null)
		{
			this.hud.ShowLaserLine(false);
			this.hud = null;
		}
		if (this.entity != null)
		{
			this.entity.Destroy();
		}
		this.state = NodeState.Idle;
		this.shipMap.Clear();
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			this.shipArray[i] = null;
		}
		this.bomber = 0;
		for (int j = 0; j < this.numArray.Length; j++)
		{
			this.numArray[j] = 0;
		}
		foreach (BaseNewSkill baseNewSkill in this.m_mapSkill)
		{
			if (baseNewSkill != null)
			{
				baseNewSkill.Destroy();
			}
		}
		this.skillBuffLogic.Destroy();
	}

	public virtual void ReturnToNeutral()
	{
		this.state = NodeState.Idle;
		this.shipMap.Clear();
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			if (this.shipArray[i] != null && this.shipArray[i].Count > 0)
			{
				int j = 0;
				while (j < this.shipArray[i].Count)
				{
					this.shipArray[i][j].Bomb(NodeType.None);
				}
			}
			this.shipArray[i] = null;
		}
		this.bomber = 0;
		for (int k = 0; k < this.numArray.Length; k++)
		{
			this.numArray[k] = 0;
		}
		this.skillBuffLogic.Destroy();
		this.occupiedTeam = TEAM.Neutral;
		this.capturingTeam = TEAM.Neutral;
		this.temp = TEAM.Neutral;
		this.currentTeam = this.nodeManager.sceneManager.teamManager.GetTeam(TEAM.Neutral);
		this.missileAttack = false;
		this.choosedBombTarget = false;
		this.choosedMissileAttack = false;
		this.updateHUD(false);
		this.hp = 0f;
	}

	public virtual void ReturnToNeutralWithoutBomb()
	{
		this.state = NodeState.Idle;
		this.skillBuffLogic.Destroy();
		this.occupiedTeam = TEAM.Neutral;
		this.capturingTeam = TEAM.Neutral;
		this.temp = TEAM.Neutral;
		this.currentTeam = this.nodeManager.sceneManager.teamManager.GetTeam(TEAM.Neutral);
		this.updateHUD(false);
		this.hp = 0f;
		if (this.nodeType == NodeType.WarpDoor && TouchHandler.currentSelect != null && TouchHandler.currentNode == this && Solarmax.Singleton<BattleSystem>.Instance.sceneManager.GetIntersection(TouchHandler.currentNode.GetPosition(), TouchHandler.currentSelect.GetPosition()))
		{
			TouchHandler.SetWarning(1);
		}
	}

	public void AddShip(int team, int count, bool noAnim = true, bool normal = true)
	{
		Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)team);
		for (int i = 0; i < count; i++)
		{
			if (this.GetShipCount(team) != Node.SHIP_MAX)
			{
				Ship ship = this.nodeManager.sceneManager.shipManager.Alloc();
				ship.Init();
				ship.currentTeam = team2;
				this.AddShip(ship);
				if (!normal)
				{
					ship.temNum++;
				}
				ship.InitShip(this.nodeManager.sceneManager.shipManager, this.nodeManager.sceneManager.battleData.vertical, noAnim);
				this.AddShipOrbit(ship);
			}
			else
			{
				this.AddShipNum(team, 1, true, normal);
				team2.SetAttribute(TeamAttr.Population, 1f, true);
			}
		}
		this.updateHUD(false);
	}

	private Ship AddShipNum(int team, int add, bool flag = true, bool normal = true)
	{
		int num = (this.numArray[team] >> 5) + 1;
		Ship result = null;
		List<Ship> list = this.GetShips(team);
		if (list == null)
		{
			list = new List<Ship>();
			this.shipMap.Add((TEAM)team, list);
			this.shipArray[team] = list;
		}
		int i = 0;
		while (i < list.Count)
		{
			if (list[i].num < num)
			{
				list[i].num += add;
				this.numArray[team] += add;
				if (!normal)
				{
					list[i].temNum += add;
				}
				result = list[i];
				if (flag)
				{
					Solarmax.Singleton<EffectManager>.Get().AddMakeEffect(list[i].currentNode, list[i].GetPosition(), list[i].entity.GetColor(), true, false);
					break;
				}
				break;
			}
			else
			{
				i++;
			}
		}
		return result;
	}

	public void EnterNode(Ship ship, Node from = null, bool warp = false)
	{
		if (this.GetShipCount((int)ship.team) <= Node.SHIP_MAX)
		{
			int num = Node.SHIP_MAX - this.GetShipCount((int)ship.team);
			if (ship.num - 1 < num)
			{
				num = ship.num - 1;
			}
			for (int i = 0; i < num; i++)
			{
				ship.currentTeam.SetAttribute(TeamAttr.Population, -1f, true);
				this.AddShip((int)ship.team, 1, true, true);
				int num2 = ship.num;
				ship.num = num2 - 1;
			}
			this.AddShip(ship);
			if (warp)
			{
				ship.entity.SetWarpTrailEffect(from, this);
			}
		}
		else
		{
			Ship ship2;
			if (ship.temNum > 0)
			{
				ship2 = this.AddShipNum((int)ship.team, ship.temNum, false, false);
				ship2 = this.AddShipNum((int)ship.team, ship.num - ship.temNum, false, true);
			}
			else
			{
				ship2 = this.AddShipNum((int)ship.team, ship.num, false, true);
			}
			ship.PoolRecovery();
			if (warp && ship2 != null)
			{
				ship2.entity.SetWarpTrailEffect(from, this);
			}
		}
		this.AddShipOrbit(ship);
		this.updateHUD(false);
	}

	public int GetTeams(TEAM team = TEAM.Neutral)
	{
		int num = 0;
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			if (i != (int)team && this.numArray[i] > 0)
			{
				num++;
			}
		}
		return num;
	}

	private void AddShip(Ship ship)
	{
		List<Ship> list = this.GetShips(ship.team);
		if (list == null)
		{
			list = new List<Ship>();
			this.shipMap.Add(ship.team, list);
			this.shipArray[(int)ship.team] = list;
		}
		ship.currentNode = this;
		ship.sceneManager = this.nodeManager.sceneManager;
		list.Add(ship);
		this.numArray[(int)ship.team] += ship.num;
	}

	public void RemoveShip(TEAM team, Ship ship, bool sub = true)
	{
		List<Ship> ships = this.GetShips(team);
		if (ships == null)
		{
			return;
		}
		if (!ships.Remove(ship))
		{
			return;
		}
		this.updateHUD(false);
	}

	public List<Ship> GetShips(TEAM team)
	{
		return this.shipArray[(int)team];
	}

	public List<Ship> GetShips(int team)
	{
		return this.shipArray[team];
	}

	public int GetShipCount(int team)
	{
		List<Ship> list = this.shipArray[team];
		if (list == null)
		{
			return 0;
		}
		return list.Count;
	}

	public int GetShipCountEdit(int team)
	{
		int num = 0;
		List<Ship> list = this.shipArray[team];
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				num += list[i].num;
			}
		}
		return num;
	}

	public int GetTempShipCount(int team)
	{
		List<Ship> list = this.shipArray[team];
		int num = 0;
		if (list == null)
		{
			return 0;
		}
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].temNum;
		}
		return num;
	}

	public int GetShipFactCount(int team)
	{
		List<Ship> list = this.shipArray[team];
		int num = 0;
		if (list == null)
		{
			return 0;
		}
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].num;
		}
		return num;
	}

	public bool IsHaveAnyShip()
	{
		foreach (List<Ship> list in this.shipArray)
		{
			if (list != null)
			{
				using (List<Ship>.Enumerator enumerator = list.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.num > 0)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public int BombShip(TEAM t, TEAM w, float rate)
	{
		int destroy = Mathf.FloorToInt((float)this.GetShipFactCount((int)t) * rate);
		return this.BombShipNum(t, w, destroy);
	}

	public int BombShipNum(TEAM t, TEAM w, int destroy)
	{
		int num = 0;
		if (destroy > 0)
		{
			List<Ship> list = this.shipArray[(int)t];
			if (list == null)
			{
				return num;
			}
			int i = 0;
			while (i < list.Count)
			{
				this.numArray[(int)t]--;
				destroy--;
				num++;
				list[i].Bomb(NodeType.None);
				this.nodeManager.sceneManager.teamManager.AddDestory(t);
				this.nodeManager.sceneManager.teamManager.AddHitShip(w);
				if (destroy <= 0 || list.Count == 0)
				{
					break;
				}
			}
		}
		this.updateHUD(false);
		return num;
	}

	public int BombShipNumWithoutScaling(TEAM t, int destroy)
	{
		int num = 0;
		if (destroy > 0)
		{
			List<Ship> list = this.shipArray[(int)t];
			if (list == null)
			{
				return num;
			}
			int i = 0;
			while (i < list.Count)
			{
				this.numArray[(int)t]--;
				destroy--;
				num++;
				list[i].Bomb(NodeType.None);
				if (destroy <= 0 || list.Count == 0)
				{
					break;
				}
			}
		}
		return num;
	}

	public int BombTempShipNum(TEAM t, int destroy)
	{
		int num = 0;
		if (destroy > 0)
		{
			List<Ship> list = this.shipArray[(int)t];
			if (list == null)
			{
				return num;
			}
			int i = 0;
			while (i < list.Count)
			{
				if (list[i].temNum == 0)
				{
					i++;
				}
				else
				{
					this.numArray[(int)t]--;
					destroy--;
					num++;
					list[i].Bomb(NodeType.None);
					this.nodeManager.sceneManager.teamManager.AddDestory(this.team);
					if (destroy <= 0 || list.Count == 0)
					{
						break;
					}
				}
			}
		}
		return num;
	}

	public void BombAllShips()
	{
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			List<Ship> list = this.shipArray[i];
			if (list != null)
			{
				int j = 0;
				while (j < list.Count)
				{
					this.numArray[i]--;
					list[j].Bomb(NodeType.None);
					this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)i);
					if (list.Count == 0)
					{
						break;
					}
				}
			}
		}
	}

	public void MoveTo(TEAM team, Node node, float rate, bool warp = false)
	{
		rate = (float)Math.Round((double)rate, 2);
		int shipFactCount = this.GetShipFactCount((int)team);
		int num = Mathf.CeilToInt(rate * (float)shipFactCount);
		if (num == 0)
		{
			return;
		}
		if (shipFactCount < num)
		{
			num = shipFactCount;
		}
		this.moveto(team, node, num, warp);
	}

	public void MoveTo(TEAM team, Node node, int count, bool warp = false)
	{
		if (count <= 0)
		{
			return;
		}
		int shipFactCount = this.GetShipFactCount((int)team);
		if (count > shipFactCount)
		{
			count = shipFactCount;
		}
		this.moveto(team, node, count, warp);
	}

	public void AIMoveTo(TEAM team, Node node, int count)
	{
		this.nodeManager.MoveTo(this, node, team, 0f, count);
	}

	public void MoveTo(TEAM team, Node[] nodes, float rate)
	{
		int num = nodes.Length;
		if (num < 1)
		{
			Debug.LogError("nodes is error");
			return;
		}
		int shipFactCount = this.GetShipFactCount((int)team);
		int num2 = (int)(rate * (float)shipFactCount);
		if (num2 == 0)
		{
			return;
		}
		int num3 = Mathf.CeilToInt((float)num2 / (float)num);
		for (int i = 0; i < num; i++)
		{
			if (num2 >= num3)
			{
				this.moveto(team, nodes[i], num3, false);
				num2 -= num3;
			}
			else
			{
				this.moveto(team, nodes[i], num2, false);
				num2 = 0;
			}
			if (num2 == 0)
			{
				break;
			}
		}
	}

	public void MoveTo(TEAM team, Node[] nodes, int nShips)
	{
		int num = nodes.Length;
		if (num < 1)
		{
			Debug.LogError("nodes is error");
			return;
		}
		int num2 = this.GetShipFactCount((int)team);
		if (num2 == 0)
		{
			return;
		}
		if (nShips < num2)
		{
			num2 = nShips;
		}
		int num3 = Mathf.CeilToInt((float)num2 / (float)num);
		for (int i = 0; i < num; i++)
		{
			if (num2 >= num3)
			{
				this.moveto(team, nodes[i], num3, false);
				num2 -= num3;
			}
			else
			{
				this.moveto(team, nodes[i], num2, false);
				num2 = 0;
			}
			if (num2 == 0)
			{
				break;
			}
		}
	}

	private void moveto(TEAM team, Node node, int count, bool warp = false)
	{
		if (count == 0)
		{
			return;
		}
		if (node == null)
		{
			return;
		}
		List<Ship> ships = this.GetShips(team);
		int shipFactCount = this.GetShipFactCount((int)team);
		int num = shipFactCount - count;
		if (ships == null || ships.Count == 0 || shipFactCount == 0)
		{
			return;
		}
		Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam(team);
		bool flag = this.CanWarp(team);
		if (warp)
		{
			flag = warp;
		}
		int num2 = 0;
		int count2 = ships.Count;
		for (int i = 0; i < count2; i++)
		{
			if (count < ships[i].num)
			{
				Ship ship = this.nodeManager.sceneManager.shipManager.Alloc();
				ship.Init();
				ship.currentTeam = team2;
				this.AddShip(ship);
				ship.InitShip(this.nodeManager.sceneManager.shipManager, this.nodeManager.sceneManager.battleData.vertical, false);
				this.AddShipOrbit(ship);
				this.numArray[(int)team]--;
				team2.SetAttribute(TeamAttr.Population, -1f, true);
				ship.num = ships[i].num - count;
				ships[i].num = count;
				if (ships[i].temNum > 0 && ships[i].temNum > count)
				{
					ship.temNum = ships[i].temNum - count;
				}
			}
			this.RemoveShipOrbit(ships[i]);
			ships[i].MoveTo(node, flag);
			this.numArray[(int)ships[i].team] -= ships[i].num;
			count -= ships[i].num;
			num2++;
			if (count == 0)
			{
				break;
			}
		}
		ships.RemoveRange(0, num2);
		this.updateHUD(false);
		if (flag)
		{
			Solarmax.Singleton<EffectManager>.Get().AddWarpArrive(node, this.currentTeam.color);
		}
		List<Ship> ships2 = this.GetShips(team);
		if (shipFactCount > Node.SHIP_MAX && num != 0 && num < Node.SHIP_MAX && num > ships2.Count)
		{
			List<Ship> ships3 = this.GetShips(team);
			int num3 = num - ships2.Count;
			for (int j = 0; j < num3; j++)
			{
				Ship ship2 = this.nodeManager.sceneManager.shipManager.Alloc();
				ship2.Init();
				ship2.currentTeam = team2;
				this.AddShip(ship2);
				ship2.InitShip(this.nodeManager.sceneManager.shipManager, this.nodeManager.sceneManager.battleData.vertical, false);
				this.AddShipOrbit(ship2);
				team2.SetAttribute(TeamAttr.Population, -1f, true);
			}
			foreach (Ship ship3 in ships3)
			{
				ship3.num = 1;
				ship3.temNum = 0;
			}
			this.numArray[(int)team] = num;
		}
	}

	public bool CanWarp()
	{
		return this.team != TEAM.Neutral && this.type == NodeType.WarpDoor && (this.team == this.nodeManager.sceneManager.battleData.currentTeam || this.nodeManager.sceneManager.teamManager.GetTeam(this.nodeManager.sceneManager.battleData.currentTeam).IsFriend(this.currentTeam.groupID));
	}

	private bool CanWarp(TEAM team)
	{
		return (this.team == team || this.nodeManager.sceneManager.teamManager.GetTeam(team).IsFriend(this.currentTeam.groupID)) && this.type == NodeType.WarpDoor;
	}

	public virtual bool IsOurNode(TEAM team)
	{
		Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam(this.team);
		Team team3 = this.nodeManager.sceneManager.teamManager.GetTeam(team);
		return this.team == team || team2.IsFriend(team3.groupID);
	}

	public void updateHUD(bool refreshLabel = false)
	{
		if (this.nodeManager.sceneManager.battleData.isFakeBattle)
		{
			return;
		}
		if (this.GetGO() == null)
		{
			return;
		}
		if (this.hud == null)
		{
			this.hud = new NodeHUD();
			this.hud.SetNode(this);
		}
		this.m_HPArray.Clear();
		this.m_teamArray.Clear();
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			int num = this.numArray[i];
			if (num != 0)
			{
				Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				this.m_teamArray.Add(team);
				this.m_HPArray.Add((float)num);
			}
		}
		if (!refreshLabel)
		{
			this.hud.ShowPopulation(this.m_teamArray, this.m_HPArray);
			return;
		}
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.isResumeBattle)
		{
			this.hud.ResetLabelPos(this.m_teamArray, this.m_HPArray);
		}
	}

	public void SetMapEditModel()
	{
		if (this is BarrierLineNode)
		{
			return;
		}
		TouchHandler componentInChildren = this.entity.GetGO().GetComponentInChildren<TouchHandler>();
		if (componentInChildren != null)
		{
			UnityEngine.Object.Destroy(componentInChildren);
		}
		NodeEvent nodeEvent = this.entity.GetGO().GetComponentInChildren<NodeEvent>();
		if (nodeEvent == null)
		{
			nodeEvent = this.entity.GetGO().AddComponent<NodeEvent>();
		}
		nodeEvent.node = this;
		nodeEvent.enabled = true;
		if (this.entity.GetGO().GetComponentInChildren<BoxCollider>() == null)
		{
			this.entity.GetGO().AddComponent<BoxCollider>();
		}
	}

	public void RemoveAllShip()
	{
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			List<Ship> list = this.shipArray[i];
			if (list != null)
			{
				List<Ship> list2 = new List<Ship>();
				list2.AddRange(list);
				for (int j = 0; j < list2.Count; j++)
				{
					Ship ship = list2[j];
					if (ship != null)
					{
						this.numArray[i]--;
						this.RemoveShip((TEAM)i, ship, true);
						ship.PoolRecovery();
					}
				}
			}
		}
	}

	public void SetGlowColor(Color color)
	{
		if (this.entity == null)
		{
			return;
		}
		this.entity.CalcGlowShape(color);
	}

	public void SetAttributeBase(NodeAttr attr, float num)
	{
		AttributeObject attributeObject = this.attribute[(int)attr];
		attributeObject.baseNum = num;
		attributeObject.Calculate();
	}

	public void SetAttribute(NodeAttr attr, float num, bool absolute)
	{
		AttributeObject attributeObject = this.attribute[(int)attr];
		if (absolute)
		{
			attributeObject.addNum += num;
		}
		else
		{
			attributeObject.addPercent += num;
		}
		if (attr == NodeAttr.Ice)
		{
			attributeObject.addNum = (float)((num <= 0f) ? 0 : 1);
		}
		attributeObject.Calculate();
	}

	public float GetAttributeFloat(NodeAttr attr)
	{
		return this.attribute[(int)attr].fixedNum;
	}

	public int GetAttributeInt(NodeAttr attr)
	{
		return Convert.ToInt32(this.GetAttributeFloat(attr));
	}

	public void SetGlowEnable(bool b)
	{
		if (this.entity == null)
		{
			return;
		}
		this.entity.SetGlowEnable(b);
	}

	public void SetNodeSize(float fSize)
	{
		if (this.entity == null)
		{
			return;
		}
		this.entity.nodesize = fSize;
	}

	public void SetNodeAngle(float fAngle)
	{
		this.nodeAngle = fAngle;
		this.SetRotation(new Vector3(0f, 0f, this.nodeAngle));
	}

	public void RotateNodeImage(float fAngle)
	{
		if (this.entity != null)
		{
			this.entity.RotateImage(fAngle);
		}
	}

	public void RotateNodeImage(Vector3 from, Vector3 to, float duration, float angle)
	{
		if (this.entity != null)
		{
			this.entity.TweenRotate(from, to, duration, angle);
		}
	}

	public float GetNodeImageAngle()
	{
		if (this.entity != null)
		{
			return this.entity.GetImageRotation();
		}
		return 0f;
	}

	public void GetTransitShips(Team t)
	{
		this.transitShips[(int)t.team] = 0;
		List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip(t.team);
		for (int i = 0; i < flyShip.Count; i++)
		{
			if (flyShip[i].entity.targetNode == this && flyShip[i].currentTeam.team == t.team)
			{
				this.transitShips[(int)t.team]++;
			}
		}
	}

	public void ResetAICalculateCache()
	{
		this.aiCache.Reset();
	}

	public int TeamStrength(TEAM t, bool useFriend = true)
	{
		if (useFriend)
		{
			if (this.aiCache.teamStrength[(int)t] > -1)
			{
				return this.aiCache.teamStrength[(int)t];
			}
		}
		else if (this.aiCache.teamStrengthMine[(int)t] > -1)
		{
			return this.aiCache.teamStrengthMine[(int)t];
		}
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(t);
		int num = this.GetShipFactCount((int)t);
		this.aiCache.teamStrengthMine[(int)t] = num;
		if (!useFriend)
		{
			return num;
		}
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			if (this.shipArray[i] != null && i != (int)t)
			{
				Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				if (team.IsFriend(team2.groupID))
				{
					num += this.GetShipFactCount(i);
				}
			}
		}
		this.aiCache.teamStrength[(int)t] = num;
		return num;
	}

	public int PredictedTeamStrength(TEAM t)
	{
		return this.PredictedTeamStrength(t, false);
	}

	public int OppStrength(TEAM t)
	{
		if (this.aiCache.oppStrength[(int)t] > -1)
		{
			return this.aiCache.oppStrength[(int)t];
		}
		int num = 0;
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(t);
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			if (i != (int)t)
			{
				Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				if (!team.IsFriend(team2.groupID))
				{
					int num2 = this.TeamStrength((TEAM)i, true);
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
		}
		this.aiCache.oppStrength[(int)t] = num;
		return num;
	}

	public int PredictedOppStrength(TEAM t)
	{
		return this.PredictedOppStrength(t, false);
	}

	public bool AICanLink(Node target, Team t)
	{
		bool result;
		if (this.type == NodeType.WarpDoor)
		{
			result = this.CanWarp(t.team);
		}
		else
		{
			result = !this.nodeManager.sceneManager.GetIntersection(this.GetPosition(), target.GetPosition());
		}
		return result;
	}

	public int GetOppLinks(Team t, List<Node> allNodes)
	{
		this.oppLinks.Clear();
		for (int i = 0; i < allNodes.Count; i++)
		{
			Node node = allNodes[i];
			if (node != this && (node.team == TEAM.Neutral || node.team != this.team || node.PredictedOppStrength(t.team) > 0) && this.AICanLink(node, t))
			{
				this.oppLinks.Add(node);
			}
		}
		return this.oppLinks.Count;
	}

	public void ResetAiTimer(TEAM t, float interval)
	{
		this.aiTimers[(int)t] = interval;
	}

	protected void UpdateAiTimers(int frame, float dt)
	{
		for (int i = 0; i < this.aiTimers.Length; i++)
		{
			if (this.aiTimers[i] > 0f)
			{
				this.aiTimers[i] -= dt;
				if (this.aiTimers[i] < 0f)
				{
					this.aiTimers[i] = 0f;
				}
			}
		}
	}

	public virtual bool CanBeTarget()
	{
		return this.type != NodeType.Barrier && this.type != NodeType.BarrierLine && this.type != NodeType.FixedWarpDoor && this.type != NodeType.Curse;
	}

	public void AttackToShip(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		this.AttackTime += dt;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			Vector3 position = this.GetPosition();
			float num = this.GetWidth() * this.GetAttackRage();
			num *= num;
			int num2;
			if (this.AttackTeam == TEAM.Neutral)
			{
				num2 = 1;
			}
			else
			{
				num2 = (int)this.AttackTeam;
			}
			if (num2 == 0 || num2 >= LocalPlayer.MaxTeamNum)
			{
				num2 = 1;
			}
			int i = 1;
			while (i < LocalPlayer.MaxTeamNum)
			{
				if (num2 <= 0 || num2 >= LocalPlayer.MaxTeamNum)
				{
					num2 = 1;
				}
				if (this.team != (TEAM)num2)
				{
					Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)num2);
					if (!team.hideFly && !this.currentTeam.IsFriend(team.groupID))
					{
						List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)num2);
						int num3 = 0;
						while (num3 < flyShip.Count && (flyShip[num3].forceShow || !flyShip[num3].currentTeam.hideFly))
						{
							if ((position - flyShip[num3].GetPosition()).sqrMagnitude <= num)
							{
								Solarmax.Singleton<EffectManager>.Get().AddLaserLine(position, flyShip[num3].GetPosition(), this.entity.GetColor(), true, false);
								Solarmax.Singleton<AudioManger>.Get().PlayLaser(this.GetPosition());
								if (!flyShip[num3].currentTeam.hideFly)
								{
									this.AttackTeam = flyShip[num3].team;
									flyShip[num3].Bomb(this.type);
									this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)num2);
									this.nodeManager.sceneManager.teamManager.AddHitShip(this.team);
									if (this.glowType == GlowType.GlowNone)
									{
										this.glowType = GlowType.GlowOn;
										this.entity.SetGlowEnable(true);
									}
									return;
								}
								if (this.glowType == GlowType.GlowNone)
								{
									this.glowType = GlowType.GlowOn;
									break;
								}
								break;
							}
							else
							{
								num3++;
							}
						}
					}
				}
				i++;
				num2++;
			}
		}
		GlowType glowType = this.glowType;
		if (glowType != GlowType.GlowOn)
		{
			if (glowType == GlowType.GlowOff)
			{
				this.alpha -= dt * 10f;
				if (this.alpha <= 0f)
				{
					this.alpha = 0f;
					this.glowType = GlowType.GlowNone;
					this.entity.SetGlowEnable(false);
				}
				Color color = this.entity.GetColor();
				color.a = this.alpha;
				this.entity.CalcGlowShape(color);
				return;
			}
		}
		else
		{
			this.alpha += dt * 20f;
			if (this.alpha >= 1f)
			{
				this.alpha = 1f;
				this.glowType = GlowType.GlowOff;
			}
			Color color2 = this.entity.GetColor();
			color2.a = this.alpha;
			this.entity.CalcGlowShape(color2);
		}
	}

	public void TwistShip(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		this.AttackTime += dt;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			Vector3 position = this.GetPosition();
			float num = this.GetWidth() * this.GetAttackRage();
			num *= num;
			int num2;
			if (this.AttackTeam == TEAM.Neutral)
			{
				num2 = 1;
			}
			else
			{
				num2 = (int)this.AttackTeam;
			}
			if (num2 == 0 || num2 >= LocalPlayer.MaxTeamNum)
			{
				num2 = 1;
			}
			int i = 1;
			while (i < LocalPlayer.MaxTeamNum)
			{
				if (num2 <= 0 || num2 >= LocalPlayer.MaxTeamNum)
				{
					num2 = 1;
				}
				if (this.team != (TEAM)num2)
				{
					Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)num2);
					if (!team.hideFly && !this.currentTeam.IsFriend(team.groupID))
					{
						List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)num2);
						int num3 = 0;
						while (num3 < flyShip.Count && !this.currentTeam.IsFriend(flyShip[num3].currentTeam.groupID) && (flyShip[num3].forceShow || !flyShip[num3].currentTeam.hideFly))
						{
							if ((position - flyShip[num3].GetPosition()).sqrMagnitude <= num)
							{
								bool isMax = false;
								if (this.currentTeam != null && this.currentTeam.current >= this.currentTeam.currentMax)
								{
									isMax = true;
								}
								Solarmax.Singleton<EffectManager>.Get().PlayTwistEffect(this, position, flyShip[num3].GetPosition(), this.entity.GetColor(), flyShip[num3].currentTeam.color, isMax);
								Solarmax.Singleton<AudioManger>.Get().PlayTwist(this.GetPosition());
								if (!flyShip[num3].currentTeam.hideFly)
								{
									this.AttackTeam = flyShip[num3].team;
									flyShip[num3].Bomb(this.type);
									if (this.currentTeam != null && this.currentTeam.current < this.currentTeam.currentMax)
									{
										this.nodeManager.sceneManager.AddShip(this, 1, (int)this.team, true, true);
									}
									this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)num2);
									this.nodeManager.sceneManager.teamManager.AddHitShip(this.team);
									if (this.glowType == GlowType.GlowNone)
									{
										this.glowType = GlowType.GlowOn;
										this.entity.SetGlowEnable(true);
									}
									return;
								}
								if (this.glowType == GlowType.GlowNone)
								{
									this.glowType = GlowType.GlowOn;
									break;
								}
								break;
							}
							else
							{
								num3++;
							}
						}
					}
				}
				i++;
				num2++;
			}
		}
	}

	public void AttackToAddShip(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		this.AttackTime += dt;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			Vector3 position = this.GetPosition();
			float num = this.GetWidth() * this.GetAttackRage();
			num *= num;
			for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
			{
				Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				if (!team.hideFly)
				{
					List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)i);
					int j = 0;
					while (j < flyShip.Count)
					{
						if (this.currentTeam.groupID == -1)
						{
							if (this.currentTeam != flyShip[j].currentTeam)
							{
								break;
							}
						}
						else if (!this.currentTeam.IsFriend(team.groupID))
						{
							break;
						}
						if (flyShip[j].currentTeam.current >= flyShip[j].currentTeam.currentMax || (!flyShip[j].forceShow && flyShip[j].currentTeam.hideFly))
						{
							break;
						}
						if ((position - flyShip[j].GetPosition()).sqrMagnitude <= num)
						{
							Solarmax.Singleton<EffectManager>.Get().PlayCloneEffect(this, position, flyShip[j].GetPosition(), this.entity.GetColor());
							Solarmax.Singleton<AudioManger>.Get().PlayClone(this.GetPosition());
							if (!flyShip[j].currentTeam.hideFly)
							{
								if (flyShip[j].currentTeam != null && flyShip[j].currentTeam.current < flyShip[j].currentTeam.currentMax)
								{
									flyShip[j].AddOne();
								}
								if (this.glowType == GlowType.GlowNone)
								{
									this.glowType = GlowType.GlowOn;
									this.entity.SetGlowEnable(true);
								}
								return;
							}
							if (this.glowType == GlowType.GlowNone)
							{
								this.glowType = GlowType.GlowOn;
								break;
							}
							break;
						}
						else
						{
							j++;
						}
					}
				}
			}
		}
	}

	public void LasergunAttack(int frame, float dt)
	{
		if (this.ani == null)
		{
			this.ani = this.entity.GetGO().GetComponent<Animator>();
		}
		if (this.team == TEAM.Neutral)
		{
			this.ani.Play(Node.hash1, 0, 0f);
			this.AttackSkillTime = 0f;
			return;
		}
		this.AttackSkillTime += dt;
		if (this.AttackSkillTime < 5f)
		{
			return;
		}
		this.ani.Play(Node.hash, 0, 0f);
		this.AttackSkillTime = 0f;
		float time = 5f;
		int num = 0;
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			Team team = this.sceneManager.teamManager.GetTeam((TEAM)i);
			if (team.groupID != -1)
			{
				if (this.nodeManager.sceneManager.teamManager.GetTeam(this.team).IsFriend(team.groupID))
				{
					num += this.GetShipFactCount((int)team.team);
				}
			}
			else if (this.nodeManager.sceneManager.teamManager.GetTeam(this.team) == team)
			{
				num += this.GetShipFactCount((int)team.team);
			}
		}
		Solarmax.Singleton<EffectManager>.Get().AddLasergun(this, time, num, true, 3f, 10f, 0f);
		Solarmax.Singleton<AudioManger>.Get().PlayLaser(this.GetPosition());
	}

	public virtual void SetShowRange(bool show)
	{
		this.showRange = show;
	}

	public virtual bool CanShowRange()
	{
		return this.showRange;
	}

	public virtual bool CanShowLaser()
	{
		return this.type == NodeType.Lasergun;
	}

	public void ShowRange(bool bShow)
	{
		if (this.CanShowRange())
		{
			if (this.hud == null)
			{
				this.hud = new NodeHUD();
				this.hud.SetNode(this);
			}
			this.hud.ShowWarningRange(bShow);
		}
		if (this is LasergunNode)
		{
			if (this.hud == null)
			{
				this.hud = new NodeHUD();
				this.hud.SetNode(this);
			}
			this.hud.ShowLaserLine(bShow);
		}
	}

	public void ResetLasergunShowRange()
	{
		if (this is LasergunNode)
		{
			if (this.hud != null)
			{
				this.hud.ShowLaserLine(false);
			}
			NodeHUD nodeHUD = new NodeHUD();
			this.hud = nodeHUD;
			this.hud.SetNode(this);
			this.hud.ShowLaserLine(true);
		}
	}

	public void GunturretAttack(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		LoggerSystem.CodeComments("代码注释by天凌喵：计算旋转时的角速度。不过不知道为什么要这样设置喵……");
		float num = 1f;
		if (this.AttackTime < num)
		{
			num = 2f;
		}
		else if (this.AttackTime < num * 2f)
		{
			num = 1.5f;
		}
		else if (this.AttackTime < num * 3f)
		{
			num = 1f;
		}
		else if (this.AttackTime < num * 4f)
		{
			num = 1.5f;
		}
		else
		{
			num = 2f;
		}
		float num2 = 100f;
		int num3 = 0;
		Vector3 vector = Vector3.zero;
		TEAM team = TEAM.Team_1;
		while ((int)team < LocalPlayer.MaxTeamNum)
		{
			if (team != this.team && !this.currentTeam.IsFriend(this.sceneManager.teamManager.GetTeam(team).groupID))
			{
				List<Ship> flyShip = this.sceneManager.shipManager.GetFlyShip(team);
				if (flyShip.Count > 0)
				{
					Ship ship = flyShip[0];
					if (ship.entity.targetNode != null)
					{
						float magnitude = (this.GetPosition() - ship.GetPosition()).magnitude;
						float num4 = (float)(num3 / 5) - num2;
						float num5 = (float)(flyShip.Count / 5) - magnitude;
						if (num3 == 0 || num4 > num5)
						{
							num2 = magnitude;
							num3 = flyShip.Count * 2;
							vector.x = ship.GetPosition().x;
							vector.y = ship.GetPosition().y;
						}
					}
				}
			}
			team += 1;
		}
		List<Node> usefulNodeList = this.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < usefulNodeList.Count; i++)
		{
			Node node = usefulNodeList[i];
			if (node.CanBeTarget() && node != this && node.IsHaveAnyShip())
			{
				for (TEAM team2 = TEAM.Team_1; team2 < TEAM.Team_7; team2 += 1)
				{
					if (team2 != this.team && !this.currentTeam.IsFriend(this.sceneManager.teamManager.GetTeam(team2).groupID))
					{
						int shipCount = node.GetShipCount((int)team2);
						if (shipCount > 0)
						{
							float magnitude2 = (this.GetPosition() - node.GetPosition()).magnitude;
							float num6 = (float)(num3 / 5) - num2;
							float num7 = (float)(shipCount / 5) - magnitude2;
							if (num6 < num7)
							{
								num2 = magnitude2;
								vector = node.GetPosition();
								num3 = shipCount;
								break;
							}
						}
					}
				}
			}
		}
		if (num3 > 0)
		{
			LoggerSystem.CodeComments("代码注释-计算追踪炮的旋转角度by天凌喵：vector是目标的位置，num8是目标角度（乘57.3是弧度制转角度制）;NodeImageAngle得到的是介于-180到180之间的值;如果二者差值小于1度，那么直接旋转到目标角度，否则按之前代码计算的一定速度旋转");
			float y = vector.y - this.GetPosition().y;
			float x = vector.x - this.GetPosition().x;
			float num8 = Mathf.Atan2(y, x) * 57.295776f;
			float nodeImageAngle = this.GetNodeImageAngle();
			float num9 = MathUtils.AngleProcess(num8 - nodeImageAngle);
			float fAngle = (Math.Abs(num9) < 1f) ? num9 : ((float)Math.Sign(num9) * num);
			this.RotateNodeImage(fAngle);
		}
		this.AttackTime += dt;
		if (this.AttackTime < this.AttackSpeed)
		{
			return;
		}
		this.AttackTime -= this.AttackSpeed;
		Solarmax.Singleton<EffectManager>.Get().AddLasergun(this, 2f, 2, false, 0.8f, 15f, 0.8f);
	}

	public SceneManager sceneManager { get; set; }

	public void InitSkills(string skilllib, Team t)
	{
		if (skilllib == "NULL")
		{
			return;
		}
		string[] array = skilllib.Split(new char[]
		{
			','
		});
		for (int i = 0; i < array.Length; i++)
		{
			int skillId = int.Parse(array[i]);
			BaseNewSkill baseNewSkill = this.sceneManager.newSkillManager.AddSkillEX(skillId, t);
			if (baseNewSkill != null)
			{
				baseNewSkill.owerNode = this;
				this.m_mapSkill.Add(baseNewSkill);
				if (baseNewSkill.config.logicId == 48 || baseNewSkill.config.logicId == 49)
				{
					this.SetAttributeBase(NodeAttr.SkillSpeed, baseNewSkill.config.cd);
				}
			}
		}
	}

	protected void UpdateBattle(int frame, float dt)
	{
		if (this.state != NodeState.Battle)
		{
			return;
		}
		this.CalcDamage(dt);
		this.DamageToShip();
		this.BattleHUD();
	}

	protected void UpdateNodeSkill(int frame, float dt)
	{
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.runWithScript)
		{
			return;
		}
		this.TickSkillCD(frame, dt);
		this.AutoUseSkill(frame, dt);
		this.UseGlobalSkill(frame, dt);
	}

	private void DamageToShip()
	{
		LoggerSystem.CodeComments("代码注释-飞船伤害计算代码by天凌喵：在此处计算对飞船的伤害。大致如下（在今后的编译过程中局部变量名可能有改变，请自行研判）：dmgs存储每一队的飞船造成的总伤害，num存储的是其他非己方队伍的数目，在第38行左右的除式表示将自己造成的伤害平摊到各敌方队伍上；随后每一队的伤害num2对单个飞船施加直到飞船血量归零后伤害溢出至下一艘飞船，直到伤害为0为止。引爆血量为0的飞船。");
		for (int i = 1; i < this.dmgs.Length; i++)
		{
			if (this.dmgs[i] > 0f)
			{
				int num = 0;
				Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				for (int j = 1; j < this.dmgs.Length; j++)
				{
					if (i != j && this.dmgs[j] > 0f)
					{
						Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)j);
						if (team != null && team2 != null && !team.IsFriend(team2.groupID))
						{
							num++;
						}
					}
				}
				for (int k = 1; k < this.dmgs.Length; k++)
				{
					if (i != k)
					{
						Team team3 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)k);
						if (team3 != null && !team.IsFriend(team3.groupID))
						{
							float num2 = this.dmgs[i] / (float)num;
							while (num2 > 0f)
							{
								List<Ship> ships = this.GetShips(k);
								if (ships == null || ships.Count <= 0 || num2 <= 0f)
								{
									break;
								}
								Ship ship = ships[0];
								if (num2 >= ship.hp)
								{
									num2 -= ship.hp;
									ship.hp = 0f;
									this.numArray[k]--;
									ship.Bomb(NodeType.None);
									this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)k);
									this.nodeManager.sceneManager.teamManager.AddHitShip((TEAM)i);
								}
								else
								{
									ship.hp -= num2;
									num2 = 0f;
								}
							}
						}
					}
				}
			}
		}
	}

	public float DamageToShip(float dmg, int team, int dmgteam)
	{
		while (dmg > 0f)
		{
			List<Ship> ships = this.GetShips(team);
			if (ships == null || ships.Count == 0)
			{
				break;
			}
			Ship ship = ships[0];
			if (dmg >= ship.hp)
			{
				dmg -= ship.hp;
				ship.hp = 0f;
				this.numArray[team]--;
				ship.Bomb(NodeType.None);
				this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)team);
				this.nodeManager.sceneManager.teamManager.AddHitShip((TEAM)dmgteam);
			}
			else
			{
				ship.hp -= dmg;
				dmg = 0f;
			}
			this.updateHUD(false);
		}
		return dmg;
	}

	private void CalcDamage(float dt)
	{
		Node.MULT_FLAG = true;
		this.dmgs[0] = 0f;
		for (int i = 1; i < this.dmgs.Length; i++)
		{
			this.dmgs[i] = 0f;
			int num = this.numArray[i];
			if (num != 0)
			{
				Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				this.dmgs[i] = (float)num * Mathf.Round(team.attack) * dt;
				if (num >= Node.MULT_COUNT)
				{
					Node.MULT_FLAG = false;
				}
			}
		}
		if (Node.MULT_FLAG)
		{
			for (int j = 1; j < this.dmgs.Length; j++)
			{
				this.dmgs[j] *= Node.MULT_NUM;
			}
		}
	}

	private void AutoUseSkill(int frame, float dt)
	{
		int i = 0;
		while (i < this.m_mapSkill.Count)
		{
			this.currentSkill = this.m_mapSkill[i];
			if (this.currentSkill == null || !this.currentSkill.CanUse() || this.currentSkill.config.logicId == 51)
			{
				return;
			}
			if (this.currentSkill.config.logicId == 47 || this.currentSkill.config.logicId == 48 || this.currentSkill.config.logicId == 49)
			{
				if (this.unknownFirst && this.id == 40)
				{
					this.unknownFirst = false;
					this.currentSkill.isFirst = true;
					this.currentSkill.ReadySkill(this.currentTeam, this, false);
					this.currentSkill.isFirst = false;
				}
				this.SkillTime += dt;
				if (this.SkillTime >= this.SkillSpeed)
				{
					this.SkillTime = 0f;
					goto IL_13B;
				}
			}
			else if (this.team == TEAM.Neutral)
			{
				this.AttackTime = 0f;
			}
			else
			{
				this.AttackTime += dt;
				if (this.AttackTime >= this.AttackSpeed)
				{
					this.AttackTime -= this.AttackSpeed;
					goto IL_13B;
				}
			}
			IL_132:
			i++;
			continue;
			IL_13B:
			if (this.currentTeam.Valid() && (this.currentSkill.config.cost == 0 || this.currentSkill.config.cost == 2))
			{
				Team team = this.sceneManager.teamManager.GetTeam(this.team);
				if (!this.currentSkill.OnCast(team) || this.type != NodeType.Lasercannon || this.state == NodeState.Occupied)
				{
					goto IL_132;
				}
				if (this.state != NodeState.Battle)
				{
					Solarmax.Singleton<EffectManager>.Get().PlayDriftSkillEffect(this, "Effect_Jiguangpao", 6f, 1f);
					goto IL_132;
				}
				if (this.lastNodeState != NodeState.Occupied)
				{
					Solarmax.Singleton<EffectManager>.Get().PlayDriftSkillEffect(this, "Effect_Jiguangpao", 6f, 1f);
					goto IL_132;
				}
				goto IL_132;
			}
			else
			{
				if (!this.currentTeam.Valid() && (this.currentSkill.config.cost == 1 || this.currentSkill.config.cost == 2))
				{
					Team team2 = this.sceneManager.teamManager.GetTeam(TEAM.Neutral);
					this.currentSkill.OnCast(team2);
					goto IL_132;
				}
				goto IL_132;
			}
		}
	}

	private void UseGlobalSkill(int frame, float dt)
	{
		for (int i = 0; i < this.m_mapSkill.Count; i++)
		{
			this.currentSkill = this.m_mapSkill[i];
			if (this.currentSkill == null || this.currentSkill.config.logicId != 51)
			{
				return;
			}
			if (this.type == NodeType.Curse)
			{
				CurseNode curseNode = (CurseNode)this;
				if (curseNode.isFirst)
				{
					this.AttackTime += dt;
					if (this.AttackTime < curseNode.curseDelay)
					{
						return;
					}
					curseNode.isFirst = false;
					this.AttackTime = 0f;
				}
				else
				{
					this.AttackTime += dt;
					if (this.AttackTime < this.AttackSpeed)
					{
						return;
					}
					this.AttackTime = 0f;
				}
				this.currentSkill.OnCast(null);
			}
		}
	}

	private void TickSkillCD(int frame, float interval)
	{
		if (!this.currentTeam.Valid())
		{
			return;
		}
		for (int i = 0; i < this.m_mapSkill.Count; i++)
		{
			this.m_mapSkill[i].Tick(frame, interval);
		}
	}

	private void BattleHUD()
	{
		if (this.hud == null)
		{
			this.hud = new NodeHUD();
			this.hud.SetNode(this);
		}
		this.m_HPArray.Clear();
		this.m_teamArray.Clear();
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			int num = this.numArray[i];
			if (num != 0)
			{
				Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				this.m_teamArray.Add(team);
				this.m_HPArray.Add((float)num);
			}
		}
		if (1 < this.m_teamArray.Count)
		{
			this.hud.ShowProgress(this.m_teamArray, this.m_HPArray);
		}
	}

	protected void UpdateCannon(int frame, float dt)
	{
		if (this.type != NodeType.Cannon)
		{
			return;
		}
		this.MissileEffect(frame, dt);
		CannonNode cannonNode = (CannonNode)this;
		if (this.team == TEAM.Neutral || this.missileAttack)
		{
			cannonNode.cd = 0f;
			return;
		}
		if (cannonNode.cd < cannonNode.AttackSpeed)
		{
			cannonNode.cd += dt;
			return;
		}
		cannonNode.cd = 0f;
		List<Node> list = this.RandomSortList<Node>(this.sceneManager.nodeManager.GetUsefulNodeList());
		list.Sort((Node x, Node y) => -x.GetHalfNodeSize().CompareTo(y.GetHalfNodeSize()));
		foreach (Node node in list)
		{
			node.GetHalfNodeSize();
			if (node.team != this.team && node.team != TEAM.Neutral && !node.missileAttack && !node.choosedBombTarget && !node.choosedMissileAttack && !cannonNode.currentTeam.IsFriend(node.currentTeam.groupID))
			{
				this.useTeam = this.team;
				cannonNode.attackNode = node;
				node.choosedMissileAttack = true;
				break;
			}
		}
		if (cannonNode.attackNode != null)
		{
			Solarmax.Singleton<EffectManager>.Get().PlayCannonEffect(this, cannonNode.attackNode);
			this.bombCD = cannonNode.AttackSpeed - 5f;
			return;
		}
		cannonNode.cd = 1f;
	}

	private void MissileEffect(int frame, float dt)
	{
		CannonNode cannonNode = (CannonNode)this;
		if (this.bombCD > 0f)
		{
			this.bombCD -= dt;
			if (this.bombCD < 1E-05f && cannonNode.attackNode != null)
			{
				Solarmax.Singleton<EffectManager>.Get().AddGlow(cannonNode.attackNode, Color.white);
				if (cannonNode.attackNode.team != this.useTeam && cannonNode.attackNode.team != TEAM.Neutral)
				{
					cannonNode.attackNode.missileAttack = true;
					this.effectCD = 4.5f;
					Solarmax.Singleton<EffectManager>.Get().PlayCannonBombEffect(cannonNode.attackNode);
				}
				else
				{
					this.bombCD = 0f;
					this.effectCD = 0f;
					this.useTeam = TEAM.Neutral;
					cannonNode.attackNode.missileAttack = false;
					cannonNode.attackNode.choosedMissileAttack = false;
					this.useTeam = TEAM.Neutral;
					cannonNode.attackNode = null;
				}
			}
		}
		if (this.effectCD > 0f)
		{
			this.effectCD -= dt;
			if (this.effectCD < 1E-05f && cannonNode.attackNode != null)
			{
				if (cannonNode.attackNode.team != this.useTeam && cannonNode.attackNode.team != TEAM.Neutral)
				{
					cannonNode.attackNode.ReturnToNeutralWithoutBomb();
				}
				this.bombCD = 0f;
				this.effectCD = 0f;
				this.useTeam = TEAM.Neutral;
				cannonNode.attackNode.missileAttack = false;
				cannonNode.attackNode.choosedMissileAttack = false;
				this.useTeam = TEAM.Neutral;
				cannonNode.attackNode = null;
			}
		}
	}

	public List<T> RandomSortList<T>(List<T> ListT)
	{
		new System.Random();
		List<T> list = new List<T>();
		foreach (T item in ListT)
		{
			if (list.Count == 0)
			{
				list.Add(item);
			}
			else
			{
				list.Insert(Solarmax.Singleton<BattleSystem>.Instance.battleData.rand.Range(0, list.Count), item);
			}
		}
		return list;
	}

	protected void UpdateCapturing(int frame, float dt)
	{
		if (this.state != NodeState.Capturing)
		{
			return;
		}
		if (this.node_state == NODESTATE.NODE_PAUSE_BATTLE)
		{
			return;
		}
		if (this.node_state == NODESTATE.NODE_CANNOT_CAP)
		{
			return;
		}
		if (this.currentTeam.GetAttributeInt(TeamAttr.StopBeCapture) > 0)
		{
			return;
		}
		float num = this.CalcOccupiedRate(this.capturingTeam);
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(this.capturingTeam);
		num *= team.GetAttributeFloat(TeamAttr.CapturedSpeed);
		num *= this.currentTeam.GetAttributeFloat(TeamAttr.BeCapturedSpeed);
		this.hp -= num * dt;
		this.CapturingHUD();
		if (this.hp <= 0f)
		{
			this.occupiedTeam = this.capturingTeam;
			this.capturingTeam = TEAM.Neutral;
			this.hp = 0f;
			Team currentTeam = this.currentTeam;
			int currentMax = this.currentTeam.currentMax;
			this.currentTeam = this.nodeManager.sceneManager.teamManager.GetTeam(TEAM.Neutral);
			this.currentTeam.teamManager.sceneManager.newSkillManager.OnCaptured(this, currentTeam, this.nodeManager.sceneManager.teamManager.GetTeam(this.occupiedTeam));
			if (Solarmax.Singleton<GameTimeManager>.Get().CheckTimer(TimerType.T_Maker))
			{
				Solarmax.Singleton<EffectManager>.Get().AddHalo(this, this.currentTeam.color, true, false);
				Solarmax.Singleton<AudioManger>.Get().PlayCapture(this.GetPosition());
			}
			if (this.type == NodeType.Tower || this.type == NodeType.Castle)
			{
				this.ResetTrueView();
			}
		}
	}

	private void CapturingHUD()
	{
		if (this.hud == null)
		{
			this.hud = new NodeHUD();
			this.hud.SetNode(this);
		}
		Team[] array = new Team[]
		{
			this.nodeManager.sceneManager.teamManager.GetTeam((this.occupiedTeam != TEAM.Neutral) ? this.occupiedTeam : this.team)
		};
		float[] hparray = new float[]
		{
			this.hp
		};
		this.hud.ShowProgress(array, hparray);
		array[0] = null;
	}

	protected void UpdateCursing(int frame, float dt)
	{
		if (this.type != NodeType.Curse)
		{
			return;
		}
		CurseNode curseNode = (CurseNode)this;
		curseNode.cd += dt;
		if (curseNode.isFirst)
		{
			curseNode.cd = 0f;
			return;
		}
		if (curseNode.cd <= curseNode.skillDelay)
		{
			curseNode.useSkill = false;
		}
		else if (curseNode.cd > curseNode.skillDelay && curseNode.cd <= curseNode.skillDelay + curseNode.skillEffective)
		{
			curseNode.useSkill = true;
		}
		else if (curseNode.cd < curseNode.AttackSpeed)
		{
			curseNode.useSkill = false;
		}
		else
		{
			curseNode.cd = 0f;
			curseNode.useSkill = false;
		}
		if (!curseNode.useSkill)
		{
			return;
		}
		foreach (Node node in Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetUsefulNodeList())
		{
			if (Mathf.Abs(node.GetGO().transform.position.y - curseNode.skillK * node.GetGO().transform.position.x - curseNode.skillB) / Mathf.Sqrt(1f + curseNode.skillK * curseNode.skillK) - node.GetWidth() <= curseNode.skillRange)
			{
				node.BombAllShips();
			}
		}
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			List<Ship> flyShip = Solarmax.Singleton<BattleSystem>.Get().sceneManager.shipManager.GetFlyShip((TEAM)i);
			for (int j = 0; j < flyShip.Count; j++)
			{
				if (Mathf.Abs(flyShip[j].entity.GetGO().transform.position.y - curseNode.skillK * flyShip[j].entity.GetGO().transform.position.x - curseNode.skillB) / Mathf.Sqrt(1f + curseNode.skillK * curseNode.skillK) <= curseNode.skillRange)
				{
					flyShip[j].hp = 0f;
					flyShip[j].Bomb(NodeType.None);
					this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)i);
				}
			}
		}
	}

	protected void DefenseToShip(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			this.DefenseCDTime = 0f;
			this.DefenseActivateTime = 0f;
			this.bCD = true;
			this.occupied = true;
			this.b3DEffect = true;
			this.isactive = false;
			if (this.holeEffect != null)
			{
				this.holeEffect.RecycleSelf();
				this.holeEffect = null;
			}
			return;
		}
		if (this.bCD)
		{
			if (this.b3DEffect)
			{
				this.b3DEffect = false;
				this.holeEffect = Solarmax.Singleton<EffectManager>.Get().PlayBlackHoleEffect(this);
			}
			if (this.occupied)
			{
				this.occupied = false;
			}
			this.DefenseCDTime += dt;
			if (this.DefenseCDTime >= 10f)
			{
				this.DefenseCDTime = 0f;
				this.DefenseActivateTime = 0f;
				this.bCD = false;
				this.isactive = true;
			}
			return;
		}
		this.DefenseActivateTime += dt;
		if (this.DefenseActivateTime >= 5f)
		{
			this.isactive = false;
			this.bCD = true;
			this.bEffect = true;
			this.b3DEffect = true;
			this.DefenseCDTime = 0f;
			this.DefenseActivateTime = 0f;
			return;
		}
		this.AttackTime += dt;
		this.blackHoleCD = this.DefenseCDTime;
		this.act = this.DefenseActivateTime;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			int i = 10000;
			if (this.glowType == GlowType.GlowNone)
			{
				this.glowType = GlowType.GlowOn;
				if (this.bEffect)
				{
					this.bEffect = false;
				}
			}
			Vector3 position = this.GetPosition();
			float num = this.GetWidth() * this.GetAttackRage();
			num *= num;
			if (i > 0)
			{
				bool flag = false;
				while (i > 0)
				{
					int num2 = 1;
					for (int j = 1; j < LocalPlayer.MaxTeamNum; j++)
					{
						Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)j);
						if (this.currentTeam.IsFriend(team.groupID) || this.currentTeam == team)
						{
							num2++;
						}
						else if (!team.hideFly)
						{
							List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)j);
							Ship ship = null;
							int num3 = 0;
							while (num3 < flyShip.Count && !flyShip[num3].currentTeam.hideFly)
							{
								if ((position - flyShip[num3].GetPosition()).sqrMagnitude <= num && !flyShip[num3].currentTeam.hideFly && flyShip[num3].num > 0)
								{
									ship = flyShip[num3];
									break;
								}
								num3++;
							}
							if (ship != null)
							{
								if (!flag)
								{
									flag = true;
									Solarmax.Singleton<AudioManger>.Get().PlayLaser(this.GetPosition());
								}
								ship.Bomb(this.type);
								this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)j);
								this.nodeManager.sceneManager.teamManager.AddHitShip(this.team);
								i--;
							}
							else
							{
								num2++;
							}
						}
					}
					if (num2 == LocalPlayer.MaxTeamNum)
					{
						break;
					}
				}
			}
		}
		GlowType glowType = this.glowType;
		if (glowType != GlowType.GlowOn)
		{
			if (glowType == GlowType.GlowOff)
			{
				this.alpha -= dt * 20f;
				if (this.alpha <= 0f)
				{
					this.alpha = 0f;
					this.glowType = GlowType.GlowNone;
				}
				Color color = this.entity.GetColor();
				color.a = this.alpha;
				this.entity.CalcGlowShape(color);
				return;
			}
		}
		else
		{
			this.alpha += dt * 20f;
			if (this.alpha >= 1f)
			{
				this.alpha = 1f;
				this.glowType = GlowType.GlowOff;
			}
			Color color2 = this.entity.GetColor();
			color2.a = this.alpha;
			this.entity.CalcGlowShape(color2);
		}
	}

	protected void DefenseFlyingShip(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			this.DefenseCDTime = 0f;
			this.DefenseActivateTime = 0f;
			this.bCD = true;
			return;
		}
		if (this.bCD)
		{
			this.DefenseCDTime += dt;
			if (this.DefenseCDTime < 10f)
			{
				return;
			}
			this.DefenseCDTime = 0f;
			this.DefenseActivateTime = 0f;
			this.bCD = false;
		}
		else
		{
			this.DefenseActivateTime += dt;
			if (this.DefenseActivateTime >= 5f)
			{
				this.bCD = true;
				this.DefenseCDTime = 0f;
				this.DefenseActivateTime = 0f;
				return;
			}
		}
		this.AttackTime += dt;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			int i = 2560;
			if (this.glowType == GlowType.GlowNone)
			{
				this.glowType = GlowType.GlowOn;
			}
			Vector3 position = this.GetPosition();
			float num = this.GetWidth() * this.GetAttackRage();
			num *= num;
			if (i > 0)
			{
				bool flag = false;
				while (i > 0)
				{
					int num2 = 1;
					for (int j = 1; j < LocalPlayer.MaxTeamNum; j++)
					{
						Team team = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)j);
						if (this.currentTeam.IsFriend(team.groupID) || this.currentTeam == team)
						{
							num2++;
						}
						else if (!team.hideFly)
						{
							List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)j);
							Ship ship = null;
							int num3 = 0;
							while (num3 < flyShip.Count && !flyShip[num3].currentTeam.hideFly)
							{
								if ((position - flyShip[num3].GetPosition()).sqrMagnitude <= num && !flyShip[num3].currentTeam.hideFly && flyShip[num3].num > 0)
								{
									ship = flyShip[num3];
									break;
								}
								num3++;
							}
							if (ship != null)
							{
								if (!flag)
								{
									flag = true;
									Solarmax.Singleton<AudioManger>.Get().PlayLaser(this.GetPosition());
								}
								ship.Bomb(NodeType.None);
								this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)j);
								this.nodeManager.sceneManager.teamManager.AddHitShip(this.team);
								i--;
							}
							else
							{
								num2++;
							}
						}
					}
					if (num2 == LocalPlayer.MaxTeamNum)
					{
						break;
					}
				}
			}
		}
		GlowType glowType = this.glowType;
		if (glowType != GlowType.GlowOn)
		{
			if (glowType == GlowType.GlowOff)
			{
				this.alpha -= dt * 20f;
				if (this.alpha <= 0f)
				{
					this.alpha = 0f;
					this.glowType = GlowType.GlowNone;
				}
				Color color = this.entity.GetColor();
				color.a = this.alpha;
				this.entity.CalcGlowShape(color);
				return;
			}
		}
		else
		{
			this.alpha += dt * 20f;
			if (this.alpha >= 1f)
			{
				this.alpha = 1f;
				this.glowType = GlowType.GlowOff;
			}
			Color color2 = this.entity.GetColor();
			color2.a = this.alpha;
			this.entity.CalcGlowShape(color2);
		}
	}

	protected void UpdateDevelop(int frame, float dt)
	{
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		int num = this.GetShipFactCount((int)this.team);
		if (num == 0)
		{
			return;
		}
		if (num > Node.SHIP_MAX)
		{
			num = Node.SHIP_MAX;
		}
		int num2 = Mathf.FloorToInt((float)num * 0.03125f * 10f);
		if (num2 == 0)
		{
			return;
		}
		if (frame % num2 != 0)
		{
			return;
		}
		PlayerData playerData = this.nodeManager.sceneManager.teamManager.GetTeam(this.team).playerData;
		int skillPower = playerData.skillPower;
		playerData.skillPower = skillPower + 1;
	}

	public EntityNode entity { get; set; }

	public NODESTATE node_state { get; set; }

	private void InitNode()
	{
		if (this.entity != null)
		{
			this.entity.Destroy();
			this.entity = null;
		}
		this.entity = this.CreateEntity(this.tag, this.perfab);
		EntityNode entity = this.entity;
	}

	private EntityNode CreateEntity(string name, string perfab)
	{
		EntityNode entityNode = null;
		switch (this.type)
		{
		case NodeType.Planet:
		case NodeType.BarrierPoint:
			entityNode = new EntityPlanet(name);
			break;
		case NodeType.Castle:
			entityNode = new EntityCastle(name);
			break;
		case NodeType.WarpDoor:
			entityNode = new EntityWarpDoor(name);
			break;
		case NodeType.Tower:
			entityNode = new EntityTower(name);
			break;
		case NodeType.Barrier:
			entityNode = new EntityBarrier(name);
			break;
		case NodeType.BarrierLine:
			entityNode = new EntityBarrierLine(name);
			break;
		case NodeType.Master:
			entityNode = new EntityMaster(name);
			break;
		case NodeType.Defense:
			entityNode = new EntityDefense(name);
			break;
		case NodeType.Power:
			entityNode = new EntityPower(name);
			break;
		case NodeType.BlackHole:
		case NodeType.Lasercannon:
		case NodeType.Attackship:
		case NodeType.Lifeship:
		case NodeType.Speedship:
		case NodeType.Captureship:
		case NodeType.AntiAttackship:
		case NodeType.AntiLifeship:
		case NodeType.AntiSpeedship:
		case NodeType.AntiCaptureship:
		case NodeType.Magicstar:
		case NodeType.Hiddenstar:
		case NodeType.Inhibit:
		case NodeType.Twist:
		case NodeType.AddTower:
		case NodeType.Sacrifice:
		case NodeType.OverKill:
		case NodeType.CloneTo:
		case NodeType.Treasure:
		case NodeType.UnknownStar:
			entityNode = new EntityCreature(name);
			break;
		case NodeType.House:
			entityNode = new EntityHouse(name);
			break;
		case NodeType.Arsenal:
			entityNode = new EntityArsenal(name);
			break;
		case NodeType.AircraftCarrier:
			entityNode = new EntityAircraftCarrier(name);
			break;
		case NodeType.FixedWarpDoor:
			entityNode = new EntityFixedWarpDoor(name);
			break;
		case NodeType.Clouds:
			entityNode = new EntityClouds(name);
			break;
		case NodeType.Lasergun:
			entityNode = new EntityLasergun(name);
			break;
		case NodeType.Mirror:
			entityNode = new EntityMirror(name);
			break;
		case NodeType.Gunturret:
			entityNode = new EntityLasergun(name);
			break;
		case NodeType.Diffusion:
			entityNode = new EntityDiffusion(name);
			break;
		case NodeType.Curse:
			entityNode = new EntityCurse(name);
			break;
		case NodeType.Cannon:
			entityNode = new EntityCannon(name);
			break;
		}
		if (entityNode != null)
		{
			entityNode.perfab = perfab;
			entityNode.Init();
		}
		return entityNode;
	}

	public void SetPosition(float x, float y)
	{
		this.SetPosition(new Vector2(x, y));
	}

	public void SetPosition(Vector2 pos)
	{
		if (this.entity == null)
		{
			return;
		}
		this.entity.SetPosition(pos);
	}

	public Vector3 GetPosition()
	{
		if (this.entity == null)
		{
			return Vector3.zero;
		}
		return this.entity.GetPosition();
	}

	public void SetScale(float scale)
	{
		if (this.entity == null)
		{
			return;
		}
		this.entity.SetScale(scale);
	}

	public float GetScale()
	{
		if (this.entity == null)
		{
			return 0f;
		}
		return this.entity.GetScale();
	}

	public void SetRotation(Vector3 r3)
	{
		if (this.entity != null)
		{
			this.entity.SetRotation(r3);
		}
	}

	public float GetDist()
	{
		if (this.entity == null)
		{
			return 1f;
		}
		return this.entity.GetDist() * 0.65f;
	}

	public float GetWidth()
	{
		if (this.entity == null)
		{
			return 1f;
		}
		return this.entity.GetWidth() * 0.5f;
	}

	public float GetHalfNodeSize()
	{
		if (this.entity == null)
		{
			return 1f;
		}
		return this.entity.GetHalfNodeSize();
	}

	public GameObject GetGO()
	{
		if (this.entity == null)
		{
			return null;
		}
		return this.entity.GetGO();
	}

	public float GetAttackRage()
	{
		return this.AttackRage;
	}

	public int GetAttackCount()
	{
		int num = 0;
		int i = 0;
		int count = this.sceneManager.nodeManager.GetUsefulNodeList().Count;
		while (i < count)
		{
			Node node = this.sceneManager.nodeManager.GetUsefulNodeList()[i];
			if (node != this)
			{
				float num2 = this.GetWidth() * this.GetAttackRage();
				if ((this.GetPosition() - node.GetPosition()).magnitude <= num2)
				{
					num++;
				}
			}
			i++;
		}
		return num;
	}

	public void CalcGlowShape(Color c)
	{
		this.entity.CalcGlowShape(c);
	}

	public void Deformating(string res)
	{
		if (this.entity.GetType() == typeof(EntityBlack))
		{
			EntityBlack entityBlack = (EntityBlack)this.entity;
			bool flag = false;
			entityBlack.Deformating(res, out flag);
			if (flag)
			{
				this.InitNodeTouch();
			}
			if (this.hud != null)
			{
				this.hud.ResetHost(this);
			}
		}
	}

	protected void EyeView(int frame, float dt)
	{
		if (this.GetAttributeInt(NodeAttr.TrueView) <= 0)
		{
			return;
		}
		Vector3 position = this.GetPosition();
		float num = this.GetWidth() * this.GetAttackRage();
		num *= num;
		int i = 0;
		int count = this.eyeShips.Count;
		while (i < count)
		{
			Ship ship = this.eyeShips[i];
			if ((position - ship.GetPosition()).sqrMagnitude > num)
			{
				ship.forceShow = false;
				this.eyeTemp.Add(ship);
			}
			i++;
		}
		int j = 0;
		int count2 = this.eyeTemp.Count;
		while (j < count2)
		{
			this.eyeShips.Remove(this.eyeTemp[j]);
			j++;
		}
		this.eyeTemp.Clear();
		for (int k = 1; k < LocalPlayer.MaxTeamNum; k++)
		{
			if (this.team != (TEAM)k)
			{
				List<Ship> flyShip = this.nodeManager.sceneManager.shipManager.GetFlyShip((TEAM)k);
				for (int l = 0; l < flyShip.Count; l++)
				{
					Ship ship2 = flyShip[l];
					if (this.currentTeam.IsFriend(ship2.currentTeam.groupID))
					{
						break;
					}
					if (!ship2.forceShow && (position - ship2.GetPosition()).sqrMagnitude <= num)
					{
						ship2.forceShow = true;
						this.eyeShips.Add(ship2);
					}
				}
			}
		}
	}

	protected void ResetTrueView()
	{
		for (int i = 0; i < this.eyeShips.Count; i++)
		{
			this.eyeShips[i].forceShow = false;
		}
		this.eyeShips.Clear();
		this.eyeTemp.Clear();
	}

	protected void UpdateLost(int frame, float dt)
	{
		if (this.initTEAM == TEAM.Neutral)
		{
			return;
		}
		if (this.state != NodeState.Idle)
		{
			return;
		}
		if (this.team == this.initTEAM)
		{
			return;
		}
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		this.DestroyTeam(this.initTEAM);
		this.initTEAM = TEAM.Neutral;
	}

	private void DestroyTeam(TEAM dt)
	{
	}

	protected void UpdateOccupied(int frame, float dt)
	{
		if (this.state != NodeState.Occupied)
		{
			return;
		}
		float num = this.CalcOccupiedRate(this.occupiedTeam);
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(this.occupiedTeam);
		num *= team.GetAttributeFloat(TeamAttr.OccupiedSpeed);
		num *= this.GetAttributeFloat(NodeAttr.OccupiedSpeed);
		if (this.type == NodeType.Tower || this.type == NodeType.Castle)
		{
			num *= 0.5f;
		}
		this.hp += num * dt;
		if (this.hp > this.hpMax)
		{
			this.hp = this.hpMax;
			this.currentTeam = this.nodeManager.sceneManager.teamManager.GetTeam(this.occupiedTeam);
			this.occupiedTeam = TEAM.Neutral;
			this.capturingTeam = TEAM.Neutral;
			if (Solarmax.Singleton<GameTimeManager>.Get().CheckTimer(TimerType.T_Maker))
			{
				Solarmax.Singleton<EffectManager>.Get().AddHalo(this, this.currentTeam.color, true, false);
				Solarmax.Singleton<AudioManger>.Get().PlayCapture(this.GetPosition());
			}
			this.currentTeam.teamManager.sceneManager.newSkillManager.OnOccupied(this, this.currentTeam);
			this.currentSkill = null;
			for (int i = 0; i < this.m_mapSkill.Count; i++)
			{
				BaseNewSkill baseNewSkill = this.m_mapSkill[i];
				baseNewSkill.ResetSkillForNode(this.currentTeam);
				baseNewSkill.OnOccupied(this, this.currentTeam);
			}
		}
		this.OccupiedHUD();
	}

	protected float CalcOccupiedRate(TEAM team)
	{
		int num = this.numArray[(int)team];
		int groupID = this.nodeManager.sceneManager.teamManager.GetTeam(team).groupID;
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			if (i != (int)team && this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i).IsFriend(groupID))
			{
				num += this.numArray[i];
			}
		}
		float num2 = (-5000f / (float)(num + 100) + 50f) / (3f * this.entity.GetScale());
		num2 *= this.nodeManager.sceneManager.GetbattleScaleSpeed();
		if (num2 > 100f)
		{
			return 100f;
		}
		return num2;
	}

	public void OccupiedHUD()
	{
		if (this.hud == null)
		{
			this.hud = new NodeHUD();
			this.hud.SetNode(this);
		}
		List<Ship> ships = this.GetShips(this.occupiedTeam);
		if (ships != null && ships.Count <= 0)
		{
			return;
		}
		Team[] array = new Team[]
		{
			this.nodeManager.sceneManager.teamManager.GetTeam(this.occupiedTeam)
		};
		float[] hparray = new float[]
		{
			this.hp
		};
		this.hud.ShowProgress(array, hparray);
		array[0] = null;
	}

	protected void CreateOrbitList()
	{
	}

	protected void UpdateOrbit(int frame, float dt)
	{
	}

	public void AddShipOrbit(Ship ship)
	{
	}

	public void RemoveShipOrbit(Ship ship)
	{
	}

	public int id { get; set; }

	public int produceFrame { get; set; }

	public int produceNum { get; set; }

	public float occupiedCreateFixTime { get; set; }

	public float occupiedCreateFixPercent { get; set; }

	public int weight { get; set; }

	public int unitLost { get; set; }

	protected void UpdateProduce(int frame, float dt, bool fixPopulation = true)
	{
		bool flag = fixPopulation & this.nodeType != NodeType.Hiddenstar;
		if (this.shipProduceOverride != -1)
		{
			flag = (this.shipProduceOverride == 0);
		}
		if (this.produceFrame == 0)
		{
			this.produceFrame = 50;
		}
		if (this.occupiedCreateFixTime > 0f)
		{
			this.occupiedCreateFixTime -= dt;
			if (this.occupiedCreateFixTime <= 0f)
			{
				this.occupiedCreateFixPercent = 0f;
				this.occupiedCreateFixPercent = 0f;
			}
		}
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		if (this.state != NodeState.Idle && this.state != NodeState.Battle && this.state != NodeState.Occupied)
		{
			return;
		}
		if (this.currentTeam == null)
		{
			return;
		}
		if (this.state == NodeState.Battle && this.GetShipFactCount((int)this.currentTeam.team) == 0)
		{
			return;
		}
		if (this.node_state != NODESTATE.NODE_NORMAL)
		{
			return;
		}
		if (this.currentTeam.team < TEAM.Team_5)
		{
			if (fixPopulation && this.currentTeam.current >= this.currentTeam.currentMax)
			{
				return;
			}
		}
		else
		{
			if (this.currentTeam.team == TEAM.Team_5)
			{
				return;
			}
			if (fixPopulation && this.currentTeam.current >= this.currentTeam.currentMax)
			{
				return;
			}
		}
		float num = 0.5f;
		num *= this.currentTeam.GetAttributeFloat(TeamAttr.ProduceSpeed);
		num *= this.GetAttributeFloat(NodeAttr.ProduceSpeed);
		num *= this.nodeManager.sceneManager.GetbattleScaleSpeed();
		int num2 = Mathf.FloorToInt((float)this.produceFrame / num + 0.5f);
		if (frame % num2 != 0)
		{
			return;
		}
		if (flag)
		{
			int num3 = this.population - this.GetShipFactCount((int)this.currentTeam.team);
			if (num3 > this.produceNum)
			{
				this.nodeManager.sceneManager.AddShip(this, this.produceNum, (int)this.team, false, true);
				this.nodeManager.sceneManager.teamManager.AddProduce(this.team, this.produceNum);
				return;
			}
			if (num3 > 0)
			{
				this.nodeManager.sceneManager.AddShip(this, num3, (int)this.team, false, true);
				this.nodeManager.sceneManager.teamManager.AddProduce(this.team, num3);
				return;
			}
		}
		else
		{
			this.nodeManager.sceneManager.AddShip(this, this.produceNum, (int)this.team, false, true);
			this.nodeManager.sceneManager.teamManager.AddProduce(this.team, this.produceNum);
		}
	}

	protected void UpdatePurify(int frame, float dt)
	{
		if (this.state == NodeState.Idle)
		{
			return;
		}
		if (this.team == TEAM.Neutral)
		{
			return;
		}
		this.AttackTime += dt;
		if (this.AttackTime >= this.AttackSpeed)
		{
			this.AttackTime -= this.AttackSpeed;
			if (this.glowType == GlowType.GlowNone)
			{
				this.glowType = GlowType.GlowOn;
			}
			if (this.state != NodeState.Idle)
			{
				for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
				{
					if (this.team != (TEAM)i && !this.currentTeam.IsFriend(this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i).groupID))
					{
						List<Ship> ships = this.GetShips(i);
						if (ships != null && ships.Count > 0)
						{
							Ship ship = ships[0];
							ship.hp = 0f;
							this.numArray[i]--;
							ship.Bomb(NodeType.None);
							this.nodeManager.sceneManager.teamManager.AddDestory((TEAM)i);
							this.nodeManager.sceneManager.teamManager.AddHitShip(this.team);
							break;
						}
					}
				}
			}
		}
		GlowType glowType = this.glowType;
		if (glowType != GlowType.GlowOn)
		{
			if (glowType == GlowType.GlowOff)
			{
				this.alpha -= dt * 20f;
				if (this.alpha <= 0f)
				{
					this.alpha = 0f;
					this.glowType = GlowType.GlowNone;
				}
				Color color = this.entity.GetColor();
				color.a = this.alpha;
				this.entity.CalcGlowShape(color);
				return;
			}
		}
		else
		{
			this.alpha += dt * 20f;
			if (this.alpha >= 1f)
			{
				this.alpha = 1f;
				this.glowType = GlowType.GlowOff;
			}
			Color color2 = this.entity.GetColor();
			color2.a = this.alpha;
			this.entity.CalcGlowShape(color2);
		}
	}

	public void SetRevolution(int type, string param1, string param2, bool clockwise = false)
	{
		this.revoParam1 = param1;
		this.revoParam2 = param2;
		Vector3 vector = Converter.ConvertVector3D(param1);
		Vector3 vector2 = Converter.ConvertVector3D(param2);
		float x = vector.x;
		float y = vector.y;
		this.revoType = (RevolutionType)type;
		if (this.revoType == RevolutionType.RT_None)
		{
			return;
		}
		this.isClockWise = clockwise;
		this.revoAngle = 0f;
		switch (this.revoType)
		{
		case RevolutionType.RT_Circular:
			this.posList.Add(new Vector3(x, y, 0f));
			break;
		case RevolutionType.RT_Triangle:
		{
			Vector3 position = this.entity.GetPosition();
			Vector3 a = new Vector3(x, y, 0f);
			this.posList.Add(position);
			Vector3 vector3 = this.RotateVector3(position, a, 2.0943952f, this.isClockWise);
			this.posList.Add(vector3);
			vector3 = this.RotateVector3(vector3, a, 2.0943952f, this.isClockWise);
			this.posList.Add(vector3);
			break;
		}
		case RevolutionType.RT_Quadrilateral:
		{
			Vector3 position2 = this.entity.GetPosition();
			Vector3 a2 = new Vector3(x, y, 0f);
			this.posList.Add(position2);
			Vector3 vector4 = this.RotateVector3(position2, a2, 1.5707964f, this.isClockWise);
			this.posList.Add(vector4);
			vector4 = this.RotateVector3(vector4, a2, 1.5707964f, this.isClockWise);
			this.posList.Add(vector4);
			vector4 = this.RotateVector3(vector4, a2, 1.5707964f, this.isClockWise);
			this.posList.Add(vector4);
			break;
		}
		case RevolutionType.RT_Ellipse:
		{
			Vector3 position3 = this.entity.GetPosition();
			float num = Vector3.Distance(position3, vector) + Vector3.Distance(position3, vector2);
			float num2 = Vector3.Distance(vector, vector2);
			this.ellipseA = num / 2f;
			this.ellipseC = num2 / 2f;
			this.ellipseB = Mathf.Sqrt(Mathf.Pow(this.ellipseA, 2f) - Mathf.Pow(this.ellipseC, 2f));
			Vector3 vector5 = (vector + vector2) / 2f;
			this.posList.Add(vector5);
			Vector3 position4 = position3 - vector5;
			Vector3 vector6 = vector2 - vector;
			this.baseAngle = Mathf.Atan2(vector6.y, vector6.x);
			Vector3 vector7 = position4.RotateAround(Vector3.zero, Vector3.forward, -this.baseAngle);
			float num3 = Mathf.Atan2(vector7.y * this.ellipseA, vector7.x * this.ellipseB);
			if (this.ellipseB < 0.001f && this.ellipseA > 0.001f)
			{
				num3 = Mathf.Acos(vector7.x / this.ellipseA);
			}
			this.revoAngle = num3;
			break;
		}
		}
		this.beginPos = this.GetPosition();
	}

	private void CalcRevolutionAngle()
	{
		if (this.revoAngle >= 6.2831855f)
		{
			this.revoAngle -= 6.2831855f;
		}
		if (this.revoAngle <= -6.2831855f)
		{
			this.revoAngle -= -6.2831855f;
		}
	}

	protected void UpdateRevolution(int frame, float dt)
	{
		switch (this.revoType)
		{
		case RevolutionType.RT_Circular:
		{
			if (this.isClockWise)
			{
				this.revoAngle += dt * this.RevoSpeed * 0.01f;
			}
			else
			{
				this.revoAngle -= dt * this.RevoSpeed * 0.01f;
			}
			this.CalcRevolutionAngle();
			Vector3 position = this.RotateVector3(this.beginPos, this.posList[this.curMoveIndex], this.revoAngle, this.isClockWise);
			this.entity.SetPosition(position);
			return;
		}
		case RevolutionType.RT_Triangle:
		{
			Vector3 vector = this.posList[this.curMoveIndex + 1];
			this.curAngle = this.RevoSpeed * dt * 0.05f;
			if (Vector3.Distance(this.entity.GetPosition(), vector) > this.curAngle)
			{
				this.entity.SetPosition(Vector3.MoveTowards(this.entity.GetPosition(), vector, this.curAngle));
				return;
			}
			this.entity.SetPosition(vector);
			this.curMoveIndex++;
			if (this.curMoveIndex >= 2)
			{
				this.curMoveIndex = -1;
				return;
			}
			break;
		}
		case RevolutionType.RT_Quadrilateral:
		{
			Vector3 vector2 = this.posList[this.curMoveIndex + 1];
			this.curAngle = this.RevoSpeed * dt * 0.05f;
			if (Vector3.Distance(this.entity.GetPosition(), vector2) > this.curAngle)
			{
				this.entity.SetPosition(Vector3.MoveTowards(this.entity.GetPosition(), vector2, this.curAngle));
				return;
			}
			this.entity.SetPosition(vector2);
			this.curMoveIndex++;
			if (this.curMoveIndex >= 3)
			{
				this.curMoveIndex = -1;
				return;
			}
			break;
		}
		case RevolutionType.RT_Ellipse:
		{
			if (this.isClockWise)
			{
				this.revoAngle += dt * this.RevoSpeed * 0.01f;
			}
			else
			{
				this.revoAngle -= dt * this.RevoSpeed * 0.01f;
			}
			this.CalcRevolutionAngle();
			Vector3 vector3 = Vector3.zero;
			vector3.x = this.ellipseA * Mathf.Cos(this.revoAngle);
			vector3.y = this.ellipseB * Mathf.Sin(this.revoAngle);
			vector3 = vector3.RotateAround(Vector3.zero, Vector3.forward, this.baseAngle);
			vector3 += this.posList[this.curMoveIndex];
			this.entity.SetPosition(vector3);
			return;
		}
		default:
			return;
		}
	}

	public float radPOX(float x, float y)
	{
		if (x == 0f && y == 0f)
		{
			return 0f;
		}
		if (y == 0f && x > 0f)
		{
			return 0f;
		}
		if (y == 0f && x < 0f)
		{
			return 3.1415927f;
		}
		if (x == 0f && y > 0f)
		{
			return 1.5707964f;
		}
		if (x == 0f && y < 0f)
		{
			return 4.712389f;
		}
		if (x > 0f && y > 0f)
		{
			return Mathf.Atan(y / x);
		}
		if (x < 0f && y > 0f)
		{
			return 3.1415927f - Mathf.Atan(y / -x);
		}
		if (x < 0f && y < 0f)
		{
			return 3.1415927f + Mathf.Atan(-y / -x);
		}
		if (x > 0f && y < 0f)
		{
			return 6.2831855f - Mathf.Atan(-y / x);
		}
		return 0f;
	}

	private Vector3 RotateVector3(Vector3 P, Vector3 A, float rad, bool isClockwise = true)
	{
		return P.RotateAround(A, Vector3.forward, rad);
	}

	public Vector3 GetNodeRunPosition(float runTime)
	{
		LoggerSystem.CodeComments("天体轨道计算by天凌喵---在这里飞船获取到天体的“预测位置”");
		if (this.revoType == RevolutionType.RT_Ellipse)
		{
			float num = runTime * this.RevoSpeed * 0.01f;
			if (this.isClockWise)
			{
				num = this.revoAngle + num;
			}
			else
			{
				num = this.revoAngle - num;
			}
			while (Mathf.Abs(num) >= 6.2831855f)
			{
				if (num >= 6.2831855f)
				{
					num -= 6.2831855f;
				}
				if (num <= -6.2831855f)
				{
					num -= -6.2831855f;
				}
			}
			Vector3 vector = Vector3.zero;
			vector.x = this.ellipseA * Mathf.Cos(num);
			vector.y = this.ellipseB * Mathf.Sin(num);
			vector = vector.RotateAround(Vector3.zero, Vector3.forward, this.baseAngle);
			return vector + this.posList[this.curMoveIndex];
		}
		Vector3 p = this.beginPos;
		float num2 = runTime * this.RevoSpeed * 0.01f;
		if (this.isClockWise)
		{
			num2 = this.revoAngle + num2;
		}
		else
		{
			num2 = this.revoAngle - num2;
		}
		if (num2 >= 6.2831855f)
		{
			num2 -= 6.2831855f;
		}
		if (num2 <= -6.2831855f)
		{
			num2 -= -6.2831855f;
		}
		return this.RotateVector3(p, this.posList[this.curMoveIndex], num2, this.isClockWise);
	}

	private void UpdateHideStatus()
	{
		if (this.currentTeam.hideFly && !this.nodeIsHide)
		{
			this.nodeIsHide = true;
			if (this.currentTeam.team == Solarmax.Singleton<BattleSystem>.Instance.battleData.currentTeam)
			{
				this.entity.SetAlpha(0.1f);
				this.entity.HideTitleAndHaloUI(false);
			}
			else
			{
				this.entity.SetAlpha(0f);
				this.entity.HideTitleAndHaloUI(false);
			}
		}
		if (this.nodeIsHide && !this.currentTeam.hideFly)
		{
			this.entity.SetAlpha(1f);
			this.nodeIsHide = false;
			this.entity.HideTitleAndHaloUI(true);
		}
	}

	protected void UpdateState(int frame, float dt)
	{
		this.temp = TEAM.Neutral;
		for (int i = 1; i < LocalPlayer.MaxTeamNum; i++)
		{
			if (this.GetShipCount(i) != 0)
			{
				if (this.temp != TEAM.Neutral)
				{
					if (!this.nodeManager.sceneManager.teamManager.GetTeam(this.temp).IsFriend(this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i).groupID))
					{
						this.state = NodeState.Battle;
						return;
					}
				}
				else
				{
					this.temp = (TEAM)i;
				}
			}
		}
		if (this.team != TEAM.Neutral)
		{
			if (this.temp == TEAM.Neutral)
			{
				this.state = NodeState.Idle;
				return;
			}
			if (this.temp == this.team || this.nodeManager.sceneManager.teamManager.GetTeam(this.team).IsFriend(this.nodeManager.sceneManager.teamManager.GetTeam(this.temp).groupID))
			{
				if (this.hp >= this.hpMax)
				{
					this.state = NodeState.Idle;
					return;
				}
				this.state = NodeState.Occupied;
				this.occupiedTeam = this.team;
				return;
			}
			else
			{
				if (this.nodeManager.sceneManager.teamManager.GetTeam(this.capturingTeam).IsFriend(this.nodeManager.sceneManager.teamManager.GetTeam(this.temp).groupID))
				{
					this.state = NodeState.Capturing;
					return;
				}
				this.capturingTeam = this.temp;
				this.state = NodeState.Capturing;
				return;
			}
		}
		else
		{
			if (this.temp == TEAM.Neutral)
			{
				this.state = NodeState.Idle;
				return;
			}
			if (this.temp == this.occupiedTeam || this.occupiedTeam == TEAM.Neutral)
			{
				this.state = NodeState.Occupied;
				this.occupiedTeam = this.temp;
				return;
			}
			if (this.nodeManager.sceneManager.teamManager.GetTeam(this.temp).IsFriend(this.nodeManager.sceneManager.teamManager.GetTeam(this.occupiedTeam).groupID))
			{
				if (this.GetShipCount((int)this.occupiedTeam) == 0)
				{
					this.occupiedTeam = this.temp;
				}
				this.state = NodeState.Occupied;
				return;
			}
			this.capturingTeam = this.temp;
			this.state = NodeState.Capturing;
			return;
		}
	}

	private void ModifyState(NodeState pre, NodeState cur)
	{
		if (pre == NodeState.Capturing && cur == NodeState.Idle)
		{
			if (this.hud != null)
			{
				this.hud.ShutHUD();
			}
		}
		else if (pre == NodeState.Occupied && cur == NodeState.Idle)
		{
			if (TouchHandler.currentNode == this && TouchHandler.currentNode != null && TouchHandler.currentSelect != null && TouchHandler.currentNode.type == NodeType.WarpDoor)
			{
				TouchHandler.SetWarning(0);
			}
			if (this.hud != null)
			{
				this.hud.ShutHUD();
			}
		}
		else if (pre == NodeState.Battle && cur == NodeState.Idle && this.hud != null)
		{
			this.hud.ShutHUD();
		}
		if (cur != NodeState.Idle && Node.onTeamValid != null)
		{
			Node.onTeamValid(false, this);
		}
		else if (this.currentTeam.Valid() && Node.onTeamValid != null)
		{
			Node.onTeamValid(false, this);
		}
		if (cur == NodeState.Idle && !this.currentTeam.Valid() && Node.onTeamValid != null)
		{
			Node.onTeamValid(true, this);
		}
	}

	private void InitNodeTouch()
	{
		NodeType type = this.type;
		if (type != NodeType.Barrier && type != NodeType.BarrierLine && type != NodeType.None && type != NodeType.Curse)
		{
			TouchHandler componentInChildren = this.entity.GetGO().GetComponentInChildren<TouchHandler>();
			if (null != componentInChildren)
			{
				componentInChildren.SetNode(this);
			}
		}
	}

	public int PredictedOppStrength(TEAM t, bool CanSeeFlight)
	{
		if (this.aiCache.predictedOppStrength[(int)t] > -1)
		{
			return this.aiCache.predictedOppStrength[(int)t];
		}
		int num = 0;
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(t);
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			if (i != (int)t)
			{
				Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				if (!team.IsFriend(team2.groupID))
				{
					int num2 = this.PredictedTeamStrength((TEAM)i, CanSeeFlight);
					if (num2 > num)
					{
						num = num2;
					}
				}
			}
		}
		this.aiCache.predictedOppStrength[(int)t] = num;
		return num;
	}

	public int PredictedTeamStrength(TEAM t, bool CanSeeFlight)
	{
		if (this.aiCache.predictedTeamStrength[(int)t] > -1)
		{
			return this.aiCache.predictedTeamStrength[(int)t];
		}
		Team team = this.nodeManager.sceneManager.teamManager.GetTeam(t);
		int num = this.GetShipFactCount((int)t);
		if (CanSeeFlight)
		{
			this.GetTransitShips(team);
			num += this.transitShips[(int)t];
		}
		for (int i = 0; i < this.shipArray.Length; i++)
		{
			if (this.shipArray[i] != null && i != (int)t)
			{
				Team team2 = this.nodeManager.sceneManager.teamManager.GetTeam((TEAM)i);
				if (team.IsFriend(team2.groupID))
				{
					num += this.GetShipFactCount(i);
					num += this.transitShips[(int)t];
				}
			}
		}
		this.aiCache.predictedTeamStrength[(int)t] = num;
		return num;
	}

	private static int SHIP_MAX = 64;

	public NodeType nodeType;

	public string nodePerfab = string.Empty;

	public float nodeAngle;

	private string nodeTag;

	public bool readyBomb;

	public int[] shipsRelifeTag = new int[LocalPlayer.MaxTeamNum];

	public List<int> aiActions = new List<int>();

	private int nodeAIStrategy = -1;

	private NodeState nodeState;

	private NodeState lastNodeState;

	public static Node.OnTeamValid onTeamValid;

	public NewSkillBuffLogic skillBuffLogic;

	private AttributeObject[] attribute = new AttributeObject[10];

	private List<Ship>[] shipArray = new List<Ship>[LocalPlayer.MaxTeamNum];

	private NodeHUD hud;

	public int[] numArray = new int[LocalPlayer.MaxTeamNum];

	public bool nodeIsHide;

	public bool imageRoate = true;

	public bool missileAttack;

	public bool choosedMissileAttack;

	public bool choosedBombTarget;

	public float aiValue;

	public float aiStrength;

	public int[] transitShips = new int[LocalPlayer.MaxTeamNum];

	public float[] aiTimers = new float[LocalPlayer.MaxTeamNum];

	private List<Node> oppLinks = new List<Node>();

	private AICalculateCache aiCache = new AICalculateCache();

	private float AttackTime;

	private float AttackSkillTime;

	private float SkillTime;

	private TEAM AttackTeam;

	private Animator ani;

	private static readonly int hash = Animator.StringToHash("Entity_Speedship_emission");

	private static readonly int hash1 = Animator.StringToHash("None");

	private GlowType glowType;

	private float alpha;

	private bool showRange;

	private static int testi = 0;

	private static int MULT_COUNT = 5;

	private static bool MULT_FLAG = false;

	private static float MULT_NUM = 2f;

	private List<BaseNewSkill> m_mapSkill = new List<BaseNewSkill>();

	public BaseNewSkill currentSkill;

	public List<Team> m_teamArray = new List<Team>();

	private List<float> m_HPArray = new List<float>();

	private float[] dmgs = new float[LocalPlayer.MaxTeamNum];

	private bool unknownFirst = true;

	private float cd;

	private float bombCD;

	private float effectCD;

	private Node attackNode;

	private TEAM useTeam;

	private float DefenseCDTime;

	private float DefenseActivateTime;

	private bool bCD = true;

	private bool bEffect = true;

	private bool occupied = true;

	private bool b3DEffect = true;

	private GameObject effect;

	private GameObject effect3D;

	private BlackHoleEffect holeEffect;

	public string perfab = string.Empty;

	private List<Ship> eyeShips = new List<Ship>();

	private List<Ship> eyeTemp = new List<Ship>();

	public TEAM initTEAM;

	private int curMoveIndex;

	private List<Vector3> posList = new List<Vector3>();

	public float RevoSpeed = 12f;

	private float curAngle;

	public bool isClockWise;

	public RevolutionType revoType;

	public string revoParam1;

	public string revoParam2;

	private Vector3 beginPos = Vector3.zero;

	private float revoAngle;

	private float ellipseA;

	private float ellipseB;

	private float ellipseC;

	public TEAM occupiedTeam;

	public TEAM capturingTeam;

	private TEAM temp;

	public float act;

	public bool isactive;

	public float blackHoleCD;

	public int shipProduceOverride;

	private float baseAngle;

	public delegate void OnTeamValid(bool visivle, Node node);
}
