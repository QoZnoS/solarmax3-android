using System;
using System.Collections.Generic;
using System.Xml.Linq;
using GameCore.Loader;

namespace Solarmax
{
	public class MapBuildingConfig : ICfgEntry
	{
		public bool Load(XElement element)
		{
			this.id = element.GetAttribute("id", string.Empty);
			this.type = element.GetAttribute("type", string.Empty);
			this.size = Convert.ToInt32(element.GetAttribute("size", "1"));
			this.x = Convert.ToSingle(element.GetAttribute("x", "0.0"));
			this.y = Convert.ToSingle(element.GetAttribute("y", "0.0"));
			this.camption = Convert.ToInt32(element.GetAttribute("camption", "0"));
			this.tag = element.GetAttribute("tag", string.Empty);
			this.orbit = Convert.ToInt32(element.GetAttribute("orbit", "0"));
			this.orbitParam1 = element.GetAttribute("orbitParam1", string.Empty);
			this.orbitParam2 = element.GetAttribute("orbitParam2", string.Empty);
			this.orbitClockWise = Convert.ToBoolean(element.GetAttribute("orbitClockWise", "false"));
			this.fAngle = Convert.ToSingle(element.GetAttribute("fAngle", "0.0"));
			this.lasergunAngle = Convert.ToSingle(element.GetAttribute("lasergunAngle", "0.0"));
			this.lasergunRange = Convert.ToSingle(element.GetAttribute("lasergunRange", "0.0"));
			this.lasergunRotateSkip = Convert.ToSingle(element.GetAttribute("lasergunRotateSkip", "30.0"));
			this.transformBulidingID = element.GetAttribute("transformBulidingID", string.Empty);
			this.curseDelay = Convert.ToSingle(element.GetAttribute("curseDelay", "0.0"));
			this.aistrategy = element.GetAttribute("aistrategy", "-1");
			this.fbpRange = Convert.ToSingle(element.GetAttribute("bpRange", "1.0"));
			this.shipProduceOverride = Convert.ToInt32(element.GetAttribute("shipProduceOverride", "-1"));
			this.orbitRevoSpeed = Convert.ToInt32(element.GetAttribute("orbitRevoSpeed", "12"));
			if (this.transformBulidingID != string.Empty)
			{
				foreach (string value in this.transformBulidingID.Split(new char[]
				{
					','
				}))
				{
					this.buildIds.Add(Convert.ToInt32(value));
				}
			}
			return true;
		}

		public string id;

		public string type;

		public int size;

		public float x;

		public float y;

		public int camption;

		public string tag;

		public int orbit;

		public string orbitParam1;

		public string orbitParam2;

		public bool orbitClockWise;

		public float fAngle;

		public float lasergunAngle;

		public float lasergunRange;

		public float lasergunRotateSkip;

		public string transformBulidingID;

		public float curseDelay;

		public string aistrategy;

		public float fbpRange;

		public List<int> buildIds = new List<int>();

		public int shipProduceOverride;

		public int orbitRevoSpeed;
	}
}
