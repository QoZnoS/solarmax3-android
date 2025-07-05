using System;
using System.Collections.Generic;
using Solarmax;

public class DiffusionSkill : BaseNewSkill
{
	public List<T> RandomSortList<T>(List<T> ListT)
	{
		Random random = new Random();
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

	public override void Destroy()
	{
		base.Destroy();
		foreach (int key in this.delayDoKey)
		{
			Coroutine.CancelDelayDo(key);
		}
	}

	public override bool OnCast(Team castTeam)
	{
		DiffusionSkill.teams = this.sceneManager.teamManager.GetFriendTeam(this.owerNode.currentTeam.team);
		if (DiffusionSkill.teams.Count == 0)
		{
			return base.OnCast(castTeam);
		}
		Team team = DiffusionSkill.teams[0];
		for (int i = 0; i < DiffusionSkill.teams.Count; i++)
		{
			if (this.owerNode.GetShipCount((int)DiffusionSkill.teams[i].team) > 0)
			{
				team = DiffusionSkill.teams[i];
				break;
			}
		}
		if (this.owerNode.GetShipCount((int)castTeam.team) > 0)
		{
			team = castTeam;
		}
		if (this.owerNode.GetShipCount((int)team.team) == 0)
		{
			return base.OnCast(castTeam);
		}
		int num = castTeam.currentMax - castTeam.current;
		if (num <= 0)
		{
			return base.OnCast(castTeam);
		}
		num++;
		this.delayDoKey.Clear();
		List<Node> list = this.sceneManager.nodeManager.GetUsefulNodeList();
		list = this.RandomSortList<Node>(list);
		bool flag = false;
		int j = 0;
		int count = list.Count;
		while (j < count)
		{
			Node node = list[j];
			if (node != this.owerNode)
			{
				float num2 = this.owerNode.GetWidth() * this.owerNode.GetAttackRage();
				if ((this.owerNode.GetPosition() - node.GetPosition()).magnitude <= num2)
				{
					Solarmax.Singleton<EffectManager>.Get().PlayDiffusionEffect(this.owerNode, this.owerNode.GetPosition(), node.GetPosition());
					node.AddShip((int)castTeam.team, 1, true, true);
					if (!flag)
					{
						flag = true;
					}
					num--;
					if (num <= 0)
					{
						break;
					}
				}
			}
			j++;
		}
		if (flag)
		{
			this.owerNode.BombShipNumWithoutScaling(team.team, 1);
		}
		Solarmax.Singleton<AudioManger>.Get().PlayEffect("HitLightning2");
		return base.OnCast(castTeam);
	}

	private List<int> delayDoKey = new List<int>();

	private static List<Team> teams;
}
