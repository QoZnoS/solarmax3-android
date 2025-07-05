using System;
using System.Collections.Generic;
using Plugin;
using Solarmax;
using UnityEngine;

public class SceneManager : Lifecycle2
{
	public SceneManager(BattleData bd)
	{
		this.battleData = bd;
		this.teamManager = new TeamManager(this);
		this.nodeManager = new NodeManager(this);
		this.shipManager = new ShipManager(this);
		this.aiManager = new AIManager(this);
		this.warpManager = new WarpManager();
		this.newSkillManager = new NewSkillManager(this);
	}

	public WarpManager warpManager { get; set; }

	public TeamManager teamManager { get; set; }

	public NodeManager nodeManager { get; set; }

	public ShipManager shipManager { get; set; }

	public AIManager aiManager { get; set; }

	public NewSkillManager newSkillManager { get; set; }

	public BattleData battleData { get; set; }

	public bool Init()
	{
		if (this.teamManager.Init())
		{
			if (this.nodeManager.Init())
			{
				if (this.shipManager.Init())
				{
					if (this.aiManager.Init())
					{
						if (this.warpManager.Init())
						{
							if (!this.newSkillManager.Init())
							{
								this.battleScaleSpeed = this.produceRates[this.curProcduceIndex].produceRate;
							}
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public void Tick(int frame, float interval)
	{
		this.teamManager.Tick(frame, interval);
		this.nodeManager.Tick(frame, interval);
		this.shipManager.Tick(frame, interval);
		this.aiManager.Tick(frame, interval);
		this.warpManager.Tick(frame, interval);
		this.newSkillManager.Tick(frame, interval);
		this.m_BattleTime += interval;
		if (Solarmax.Singleton<BattleSystem>.Instance.battleData.gameType == GameType.PVP && this.curProcduceIndex < this.produceRates.Count - 1 && this.m_BattleTime > this.produceRates[this.curProcduceIndex + 1].startTime)
		{
			this.curProcduceIndex++;
			this.battleScaleSpeed = this.produceRates[this.curProcduceIndex].produceRate;
		}
	}

	public void UpdateRender(float interval)
	{
		this.shipManager.UpdateRender(interval);
	}

	public void Destroy()
	{
		this.teamManager.Destroy();
		this.nodeManager.Destroy();
		this.shipManager.Destroy();
		this.aiManager.Destroy();
		this.warpManager.Destroy();
		this.newSkillManager.Destroy();
		this.m_BattleTime = 0f;
		this.curProcduceIndex = 0;
		this.battleScaleSpeed = this.produceRates[this.curProcduceIndex].produceRate;
	}

	public void Release()
	{
	}

	public void Reset()
	{
		this.m_BattleTime = 0f;
	}

	public bool GetBattleRate()
	{
		return this.m_BattleTime >= 180f;
	}

	public float GetBattleTime()
	{
		return this.m_BattleTime;
	}

	public void RunFramePacket(FrameNode node)
	{
		if (node.msgList == null || node.msgList.Length == 0)
		{
			return;
		}
		for (int i = 0; i < node.msgList.Length; i++)
		{
			this.ExcutePacket(node.msgList[i] as global::Packet);
		}
	}

	private void ExcutePacket(global::Packet packet)
	{
		if (packet.packet.type == 0)
		{
			if (packet.packet.move.team == TEAM.Neutral)
			{
				this.nodeManager.MoveTo(packet.packet.move, packet.team);
			}
			else
			{
				this.nodeManager.MoveTo(packet.packet.move, packet.packet.move.team);
			}
		}
		else if (packet.packet.type == 1)
		{
			this.newSkillManager.OnCast(packet.packet.skill);
		}
		else if (packet.packet.type == 2)
		{
			Solarmax.Singleton<BattleSystem>.Instance.OnPlayerGiveUp(packet.packet.giveup.team);
			Solarmax.Singleton<BattleSystem>.Instance.sceneManager.DestroyTeam(packet.packet.giveup.team, TEAM.Neutral);
		}
		else if (packet.packet.type == 3)
		{
			Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.SetAllTeamMoveSpeed(1f);
			Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.SetAllTeamProduceSpeed(1f);
			Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.SetAllOccupySpeed(1f);
		}
		else if (packet.packet.type == 4)
		{
			Node node = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.bomb.tag);
			if (node != null)
			{
				if (TouchHandler.currentNode == node)
				{
					TouchHandler.HideOperater();
				}
				node.ReturnToNeutral();
				Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.RemoveNode(node.tag);
				Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.RemoveUnusedBarrier();
				Solarmax.Singleton<BattleSystem>.Instance.sceneManager.shipManager.DestroyAllShipToTargetNode(node);
			}
		}
		else if (packet.packet.type == 6)
		{
			Node node2 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.effect.tag);
			if (node2 != null)
			{
				if (packet.packet.effect.effect == "EFF_XJ_Boom_1")
				{
					Solarmax.Singleton<AudioManger>.Get().PlayPlanetExplosion(node2.GetPosition());
					Solarmax.Singleton<EffectManager>.Get().PlaySyncEffect(node2, packet.packet.effect.effect, packet.packet.effect.time, packet.packet.effect.scale);
				}
				else
				{
					Solarmax.Singleton<EffectManager>.Get().PlaySyncEffect(node2, packet.packet.effect.effect, packet.packet.effect.time, packet.packet.effect.scale);
				}
			}
		}
		else if (packet.packet.type == 7)
		{
			Node node3 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.attack.tag);
			if (node3 != null)
			{
				node3.AttackToShip(0, node3.AttackSpeed);
			}
		}
		else if (packet.packet.type == 8)
		{
			Node node4 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.addattack.tag);
			if (node4 != null)
			{
				node4.AttackToAddShip(0, node4.AttackSpeed);
			}
		}
		else if (packet.packet.type == 9)
		{
			Node node5 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.gunattack.tag);
			if (node5 != null)
			{
				node5.GunturretAttack(0, node5.AttackSpeed);
			}
		}
		else if (packet.packet.type == 10)
		{
			Node node6 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.laserattack.tag);
			if (node6 != null)
			{
				node6.LasergunAttack(0, node6.AttackSpeed);
			}
		}
		else if (packet.packet.type == 11)
		{
			Node node7 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.twistattack.tag);
			if (node7 != null)
			{
				node7.TwistShip(0, node7.AttackSpeed);
			}
		}
		else if (packet.packet.type == 12)
		{
			Node node8 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.team.tag);
			if (node8 != null)
			{
				node8.ReturnToNeutral();
				node8.hp = node8.hpMax;
				node8.currentTeam = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.teamManager.GetTeam(packet.packet.team.t);
				Solarmax.Singleton<EffectManager>.Get().AddHalo(node8, node8.currentTeam.color, true, false);
				Solarmax.Singleton<AudioManger>.Get().PlayCapture(node8.GetPosition());
				node8.currentTeam.teamManager.sceneManager.newSkillManager.OnOccupied(node8, node8.currentTeam);
				node8.currentSkill = null;
				node8.OccupiedHUD();
			}
		}
		else if (packet.packet.type == 13)
		{
			Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.RemoveALLBarriers();
		}
		else if (packet.packet.type == 14)
		{
			Node node9 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.skill.tag);
			Team team = this.teamManager.GetTeam(packet.packet.team.t);
			BaseNewSkill baseNewSkill = this.newSkillManager.AddSkillEX(packet.packet.skill.skillID, team);
			if (baseNewSkill != null)
			{
				baseNewSkill.owerNode = node9;
			}
			baseNewSkill.OnCast(this.teamManager.GetTeam(packet.packet.skill.from));
		}
		else if (packet.packet.type == 15)
		{
			Team team2 = this.teamManager.GetTeam(packet.team);
			Node node10 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.unknown.tag);
			BaseNewSkill baseNewSkill2 = this.newSkillManager.AddSkillEX(packet.packet.unknown.skillID, team2);
			if (baseNewSkill2 != null)
			{
				baseNewSkill2.owerNode = node10;
				baseNewSkill2.scriptMisc = packet.packet.unknown.transformId;
			}
			baseNewSkill2.OnCast(this.teamManager.GetTeam(packet.packet.unknown.from));
		}
		else if (packet.packet.type == 16)
		{
			Node node11 = Solarmax.Singleton<BattleSystem>.Instance.sceneManager.nodeManager.GetNode(packet.packet.effect.tag);
			if (node11 != null)
			{
				node11.choosedBombTarget = true;
				Solarmax.Singleton<EffectManager>.Get().PlaySyncEffect(node11, packet.packet.effect.effect, packet.packet.effect.time, 1f);
			}
		}
	}

	public void CreateScene(IList<int> usr, bool isEditer = false, bool isHaveRandom = false)
	{
		MapConfig data = Solarmax.Singleton<MapConfigProvider>.Instance.GetData(this.battleData.matchId);
		if (data == null)
		{
			Debug.LogErrorFormat("CreateScene-Load table is error!!! {0}", new object[]
			{
				this.battleData.matchId
			});
			this.Release();
			Solarmax.Singleton<UISystem>.Get().HideAllWindow();
			Solarmax.Singleton<UISystem>.Get().ShowWindow("LogoWindow");
			return;
		}
		this.CreateScene(isEditer, data, usr, isHaveRandom);
	}

	public void CreateScene(bool isEditer, MapConfig table, IList<int> usr = null, bool isHaveRandom = false)
	{
		this.battleData.currentTable = table;
		this.CreateNode(table.mbcList);
		this.CreateLines(table.mlcList);
		this.CreateBarrierPoints(table.mbcList);
		this.CreateShip(table.mpcList);
		for (int i = 0; i < table.player_count; i++)
		{
			Team team = this.teamManager.GetTeam((TEAM)(i + 1));
			this.newSkillManager.OnBorn(team);
		}
		if (!isEditer)
		{
			for (int j = 3; j < 8; j++)
			{
				global::Coroutine.DelayDo((float)j, new EventDelegate(delegate()
				{
					foreach (Node node in this.nodes)
					{
						if (node != null)
						{
							node.updateHUD(true);
						}
					}
				}));
			}
		}
		if (!isEditer)
		{
			LevelConfig data = Solarmax.Singleton<LevelConfigConfigProvider>.Instance.GetData(this.battleData.matchId);
			if (data != null)
			{
				if (!string.IsNullOrEmpty(data.musicName))
				{
					Solarmax.Singleton<AudioManger>.Get().PlayAudioBG(data.musicName, 0.5f);
				}
				else
				{
					Solarmax.Singleton<AudioManger>.Get().PlayAudioBG("Wandering", 0.5f);
				}
			}
			else
			{
				Solarmax.Singleton<AudioManger>.Get().PlayAudioBG("Wandering", 0.5f);
			}
		}
	}

	public void CreateLines(List<MapLineConfig> lines)
	{
		if (lines == null)
		{
			return;
		}
		foreach (MapLineConfig mapLineConfig in lines)
		{
			this.AddBarrierLines(mapLineConfig.point1, mapLineConfig.point2);
		}
	}

	private void CreateBarrierPoints(List<MapBuildingConfig> building)
	{
		if (building == null)
		{
			return;
		}
		foreach (MapBuildingConfig mapBuildingConfig in building)
		{
			if (mapBuildingConfig.type == "BarrierPoint")
			{
				this.nodeManager.dynamicBarriers.Add(mapBuildingConfig);
			}
		}
		if (this.nodeManager.dynamicBarriers.Count < 2)
		{
			this.nodeManager.dynamicBarriers.Clear();
			return;
		}
		for (int i = 0; i < this.nodeManager.dynamicBarriers.Count; i++)
		{
			MapBuildingConfig point1 = this.nodeManager.dynamicBarriers[i];
			for (int j = i + 1; j < this.nodeManager.dynamicBarriers.Count; j++)
			{
				MapBuildingConfig point2 = this.nodeManager.dynamicBarriers[j];
				MapPlayerConfig mapPlayerConfig = Solarmax.Singleton<BattleSystem>.Instance.battleData.currentTable.mpcList.Find((MapPlayerConfig n) => n.tag == point1.tag);
				MapPlayerConfig mapPlayerConfig2 = Solarmax.Singleton<BattleSystem>.Instance.battleData.currentTable.mpcList.Find((MapPlayerConfig n) => n.tag == point2.tag);
				bool show;
				bool flag2;
				bool flag = flag2 = (show = false);
				if (mapPlayerConfig != null && mapPlayerConfig.ship > 0)
				{
					flag2 = true;
				}
				if (mapPlayerConfig2 != null && mapPlayerConfig2.ship > 0)
				{
					flag = true;
				}
				Vector3 zero;
				Vector3 b = zero = Vector3.zero;
				zero.x = point1.x;
				zero.y = point1.y;
				b.x = point2.x;
				b.y = point2.y;
				float num = Vector3.Distance(zero, b);
				if (flag2 == flag && point1.fbpRange >= num && point2.fbpRange >= num)
				{
					show = true;
				}
				this.nodeManager.AddDynamicBarrierLines(point1.tag, point2.tag, show);
			}
		}
	}

	private void CreateShip(List<MapPlayerConfig> players)
	{
		if (players == null)
		{
			return;
		}
		foreach (MapPlayerConfig mapPlayerConfig in players)
		{
			if (mapPlayerConfig != null)
			{
				int ship = mapPlayerConfig.ship;
				this.AddShip(mapPlayerConfig.tag, ship, mapPlayerConfig.camption, true);
			}
		}
	}

	private void CreateNode(List<MapBuildingConfig> building)
	{
		if (building == null)
		{
			return;
		}
		this.nodes.Clear();
		foreach (MapBuildingConfig mapBuildingConfig in building)
		{
			MapNodeConfig data = Solarmax.Singleton<MapNodeConfigProvider>.Instance.GetData(mapBuildingConfig.type, mapBuildingConfig.size);
			if (data == null)
			{
				break;
			}
			if (string.IsNullOrEmpty(mapBuildingConfig.aistrategy))
			{
				mapBuildingConfig.aistrategy = "-1";
			}
			Node node = this.AddNode(data.id, mapBuildingConfig.camption, data.typeEnum, data.aiWeight, data.aiUnitLost, mapBuildingConfig.x, mapBuildingConfig.y, data.size, mapBuildingConfig.tag, data.createshipnum, data.createship, (float)data.hp, data.food, data.attackrange, data.attackspeed, data.attackpower, 0f, mapBuildingConfig.orbit, mapBuildingConfig.orbitParam1, mapBuildingConfig.orbitParam2, mapBuildingConfig.orbitClockWise, data.nodesize, data.perfab, data.skills, mapBuildingConfig.fAngle, int.Parse(mapBuildingConfig.aistrategy), mapBuildingConfig.lasergunAngle, mapBuildingConfig.lasergunRange);
			node.shipProduceOverride = mapBuildingConfig.shipProduceOverride;
			node.RevoSpeed = (float)mapBuildingConfig.orbitRevoSpeed;
			if (node != null && Solarmax.Singleton<BattleSystem>.Instance.battleData.winType == "occupy" && (Solarmax.Singleton<BattleSystem>.Instance.battleData.winTypeParam1 == node.tag || Solarmax.Singleton<BattleSystem>.Instance.battleData.winTypeParam2 == node.tag))
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Solarmax.Singleton<AssetManager>.Get().GetResources("Effect_Aims")) as GameObject;
				gameObject.transform.SetParent(node.entity.GetGO().transform);
				float num = 0.4f / data.size;
				gameObject.transform.localScale = new Vector3(0.03f * num, 0.03f * num, 0.03f * num);
				gameObject.transform.localPosition = new Vector3(-1.7f * num, 1.3f * num, 0f);
			}
			this.nodes.Add(node);
		}
	}

	public Node AddNode(int id, int team, int kind, int weight, int unitlost, float x, float y, float scale, string tag, int buildNum, float buildTimer, float hpMax, int pop, float rage, float speed, float power, float skillspeed, int orbit, string orbitParam1, string orbitParam2, bool orbitclockwise, float nodesize, string perfab, string skills, float fAngle, int aistrategy, float lasergunAngle = 0f, float lasergunRange = 0f)
	{
		float x2 = (float)Math.Round((double)x, 2);
		float y2 = (float)Math.Round((double)y, 2);
		Node node = this.nodeManager.AddNode(id, team, kind, weight, unitlost, x2, y2, scale, tag, buildNum, buildTimer, hpMax, pop, rage, speed, power, skillspeed, this.battleData.mapEdit, orbit, orbitParam1, orbitParam2, orbitclockwise, nodesize, perfab, skills, fAngle, aistrategy);
		if (node != null)
		{
			node.sceneManager = this;
			node.nodePerfab = perfab;
			Team team2 = this.teamManager.GetTeam((TEAM)team);
			node.InitSkills(skills, team2);
			if (node.nodeType == NodeType.Mirror)
			{
				node.RotateNodeImage(fAngle);
			}
			else if (node.nodeType == NodeType.Gunturret)
			{
				node.SetNodeAngle(0f);
				node.RotateNodeImage(fAngle);
			}
			else
			{
				node.SetNodeAngle(fAngle);
			}
			if (node.nodeType == NodeType.FixedWarpDoor)
			{
				this.nodeManager.AddFixedPortal(node.GetPosition(), fAngle, nodesize);
			}
			if (node.nodeType == NodeType.Lasergun && lasergunRange > 0f && lasergunAngle > 0f)
			{
				node.RotateNodeImage(lasergunAngle);
			}
		}
		return node;
	}

	public int GetNodeCount()
	{
		return this.nodeManager.GetNodeCount();
	}

	public Node AddNode(string tag, int kind, float dx, float dy, string perfab)
	{
		if (this.battleData.mapEdit)
		{
			if (this.battleData.currentTable == null)
			{
				Solarmax.Singleton<BattleSystem>.Instance.battleData.currentTable = new MapConfig("newmap1");
			}
			if (this.battleData.currentTable.mbcList == null)
			{
				this.battleData.currentTable.mbcList = new List<MapBuildingConfig>();
			}
			MapBuildingConfig mapBuildingConfig = new MapBuildingConfig();
			this.battleData.currentTable.mbcList.Add(mapBuildingConfig);
			mapBuildingConfig.tag = tag;
			mapBuildingConfig.x = dx;
			mapBuildingConfig.y = dy;
			mapBuildingConfig.type = this.GetType(kind);
			mapBuildingConfig.size = 1;
		}
		return this.nodeManager.AddNode(tag, kind, dx, dy, NodeAttribute.GetScale(kind, 1), this.battleData.mapEdit, perfab);
	}

	public Node GetNode(string tag)
	{
		return this.nodeManager.GetNode(tag);
	}

	public string GetType(int kind)
	{
		string[] array = new string[]
		{
			string.Empty,
			"star",
			"castle",
			"teleport",
			"tower",
			"barrier",
			"barrierline",
			"master",
			"defense",
			"power",
			"BlackHole",
			"House",
			"Arsenal",
			"AircraftCarrier",
			"Lasercannon",
			"Attackship",
			"Lifeship",
			"Speedship",
			"Captureship",
			"AntiAttackship",
			"AntiLifeship",
			"AntiSpeedship",
			"AntiCaptureship",
			"Magicstar",
			"Hiddenstar",
			"FixedWarpDoor",
			"Clouds",
			"Inhibit",
			"Twist",
			"AddTower",
			"Sacrifice",
			"OverKill",
			"CloneTo",
			"Treasure",
			"UnknownStar",
			"Lasergun",
			"Mirror",
			"BarrierPoint",
			"Gunturret",
			"Diffusion",
			"Curse",
			"Cannon"
		};
		return array[kind];
	}

	public void RemoveNode(string tag)
	{
		this.nodeManager.RemoveNode(tag);
		if (this.battleData.mapEdit)
		{
			MapBuildingConfig item = this.battleData.currentTable.mbcList.Find((MapBuildingConfig s) => tag.Equals(s.tag));
			this.battleData.currentTable.mbcList.Remove(item);
			if (this.battleData.currentTable.mpcList != null)
			{
				MapPlayerConfig mapPlayerConfig = this.battleData.currentTable.mpcList.Find((MapPlayerConfig i) => tag.Equals(i.tag));
				if (mapPlayerConfig != null)
				{
					this.battleData.currentTable.mpcList.Remove(mapPlayerConfig);
				}
			}
			if (this.battleData.currentTable.mlcList != null)
			{
				for (;;)
				{
					MapLineConfig mapLineConfig = this.battleData.currentTable.mlcList.Find((MapLineConfig s) => s.point1 == tag || s.point2 == tag);
					if (mapLineConfig == null)
					{
						break;
					}
					this.battleData.currentTable.mlcList.Remove(mapLineConfig);
					this.nodeManager.DelBarrierLines(mapLineConfig.point1, mapLineConfig.point2);
				}
			}
		}
	}

	public void AddShip(Node node, int count, int team, bool noAnim = true, bool normal = true)
	{
		if (node == null)
		{
			return;
		}
		node.AddShip(team, count, noAnim, normal);
	}

	public void AddShip(string tag, int count, int team, bool noAnim = true)
	{
		Node node = this.nodeManager.GetNode(tag);
		if (node == null)
		{
			return;
		}
		this.AddShip(node, count, team, noAnim, true);
	}

	public void AddBarrierLines(string barrierX, string barrierY)
	{
		this.nodeManager.AddBarrierLines(barrierX, barrierY);
	}

	public bool GetIntersection(Vector3 v3X, Vector3 v3Y)
	{
		return this.nodeManager.IntersectBarrierLien(v3X, v3Y);
	}

	public bool IsFixedPortal(Vector3 v3X, Vector3 v3Y)
	{
		return this.nodeManager.IsFixedPortal(v3X, v3Y);
	}

	public void AddMapInfo(string name, string num)
	{
		if (!this.MapInfo.ContainsKey(name))
		{
			this.MapInfo.Add(name, num);
		}
	}

	public Dictionary<string, string> GetMapInfo()
	{
		return this.MapInfo;
	}

	public void AddProduce(float startTime, float rateProduce)
	{
		ShipProduce item = default(ShipProduce);
		item.startTime = startTime;
		item.produceRate = rateProduce;
		this.produceRates.Add(item);
	}

	public float GetbattleScaleSpeed()
	{
		return this.battleScaleSpeed;
	}

	public void DestroyTeam(TEAM team, TEAM win)
	{
		List<Node> usefulNodeList = this.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < usefulNodeList.Count; i++)
		{
			Node node = usefulNodeList[i];
			if (node.team == team)
			{
				node.BombShip(team, win, 1f);
				node.currentTeam = this.teamManager.GetTeam(TEAM.Neutral);
				if (node.capturingTeam != TEAM.Neutral && node.capturingTeam != team)
				{
					node.hp = 0f;
				}
				if (node.capturingTeam == TEAM.Neutral && node.occupiedTeam == TEAM.Neutral)
				{
					node.hp = 0f;
				}
				if (Solarmax.Singleton<GameTimeManager>.Get().CheckTimer(TimerType.T_Maker))
				{
					Solarmax.Singleton<EffectManager>.Get().AddHalo(node, node.currentTeam.color, true, false);
					Solarmax.Singleton<AudioManger>.Get().PlayCapture(node.GetPosition());
				}
			}
			else if (node.GetShipCount((int)team) > 0)
			{
				node.BombShip(team, win, 1f);
			}
		}
		List<Ship> flyShip = this.shipManager.GetFlyShip(team);
		int j = flyShip.Count - 1;
		while (j >= 0)
		{
			if (flyShip[j].num == 1)
			{
				flyShip[j].Bomb(NodeType.None);
				j--;
			}
			else
			{
				flyShip[j].Bomb(NodeType.None);
			}
		}
	}

	public void DestroyTeamTemporary(TEAM team)
	{
		List<Node> usefulNodeList = this.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < usefulNodeList.Count; i++)
		{
			Node node = usefulNodeList[i];
			int tempShipCount = node.GetTempShipCount((int)team);
			if (node.GetTempShipCount((int)team) > 0)
			{
				node.BombTempShipNum(team, tempShipCount);
			}
		}
		List<Ship> flyShip = this.shipManager.GetFlyShip(team);
		int j = flyShip.Count - 1;
		while (j >= 0)
		{
			if (flyShip[j].temNum == 0)
			{
				j--;
			}
			else
			{
				flyShip[j].Bomb(NodeType.None);
			}
		}
	}

	public void FadePlanet(bool fadeIn, float duration)
	{
		List<Node> usefulNodeList = this.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < usefulNodeList.Count; i++)
		{
			Node node = usefulNodeList[i];
			if (node.entity != null)
			{
				node.entity.FadeEntity(fadeIn, duration);
			}
		}
		List<Node> barrierNodeList = this.nodeManager.GetBarrierNodeList();
		for (int j = 0; j < barrierNodeList.Count; j++)
		{
			Node node2 = barrierNodeList[j];
			if (node2.entity != null)
			{
				node2.entity.FadeEntity(fadeIn, duration);
			}
		}
	}

	public void ShowWinEffect(Team win, Color color)
	{
		List<Node> usefulNodeList = this.nodeManager.GetUsefulNodeList();
		for (int i = 0; i < usefulNodeList.Count; i++)
		{
			Node node = usefulNodeList[i];
			if (node.type != NodeType.Barrier && node.type != NodeType.BarrierLine)
			{
				Solarmax.Singleton<EffectManager>.Get().AddHalo(node, color, true, false);
				node.entity.SetColor(color);
				node.entity.SetAlpha(1f);
			}
			for (int j = 0; j < LocalPlayer.MaxTeamNum; j++)
			{
				TEAM team = (TEAM)j;
				Team team2 = this.teamManager.GetTeam(team);
				if (win != team2 && !win.IsFriend(team2.groupID))
				{
					node.BombShip(team, win.team, 1f);
				}
			}
		}
	}

	public void SilentMode(bool status)
	{
		Solarmax.Singleton<LoggerSystem>.Instance.Info(string.Format("========SilentMode : {0},, frame:{1}", status, Solarmax.Singleton<BattleSystem>.Instance.GetCurrentFrame()), new object[0]);
		this.battleData.silent = status;
		this.nodeManager.SilentMode(status);
		this.shipManager.SilentMode(status);
	}

	public Dictionary<string, string> MapInfo = new Dictionary<string, string>();

	private float m_BattleTime;

	public List<ShipProduce> produceRates = new List<ShipProduce>();

	public int curProcduceIndex;

	public float battleScaleSpeed = 1f;

	private List<Node> nodes = new List<Node>();
}
