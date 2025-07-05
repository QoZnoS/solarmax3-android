using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GameCore.Loader;
using UnityEngine;

namespace Solarmax
{
	public class MapConfig
	{
		public MapConfig(string p_name) : base()
		{
			this.mlcList = new List<MapLineConfig>();
			this.mbcList = new List<MapBuildingConfig>();
			this.mpcList = new List<MapPlayerConfig>();
			this.defaultai = "-1";
			this.teamAiType = new List<int>();
			//base..ctor();
			this.name = p_name;
		}

		public bool IsXML()
		{
			return true;
		}

		public string Path()
		{
			return "data/mapconfig/";
		}

		public void Load()
		{
			this.mlcList.Clear();
			this.mbcList.Clear();
			this.mpcList.Clear();
			try
			{
				string text = LoadResManager.LoadCustomTxt(this.Path() + this.name + ".xml");
				if (!string.IsNullOrEmpty(text))
				{
					XElement xelement = XDocument.Parse(text).Element("map");
					if (xelement != null)
					{
						this.id = xelement.GetAttribute("id", string.Empty);
						this.player_count = Convert.ToInt32(xelement.GetAttribute("player_count", "0"));
						this.vertical = Convert.ToBoolean(xelement.GetAttribute("vertical", string.Empty));
						this.audio = xelement.GetAttribute("audio", string.Empty);
						this.defaultai = xelement.GetAttribute("defaultAIStrategy", "-1");
						this.teamAITypes = xelement.GetAttribute("teamAITypes", string.Empty);
						if (this.teamAITypes != string.Empty)
						{
							foreach (string value in this.teamAITypes.Split(new char[]
							{
								','
							}))
							{
								this.teamAiType.Add(Convert.ToInt32(value));
							}
						}
						for (int j = this.teamAiType.Count; j < LocalPlayer.MaxTeamNum; j++)
						{
							this.teamAiType.Add(-1);
						}
						this.teamFriend = xelement.GetAttribute("teamFriend", string.Empty);
						if (this.teamFriend != string.Empty)
						{
							foreach (string value2 in this.teamFriend.Split(new char[]
							{
								','
							}))
							{
								this.teamFriendList.Add(Convert.ToInt32(value2));
							}
						}
						IEnumerable<XElement> enumerable = xelement.Elements("mapbuildings");
						if (enumerable != null)
						{
							foreach (XElement xelement2 in enumerable)
							{
								foreach (XElement element in xelement2.Elements("mapbuilding"))
								{
									MapBuildingConfig mapBuildingConfig = new MapBuildingConfig();
									if (mapBuildingConfig.Load(element))
									{
										this.mbcList.Add(mapBuildingConfig);
									}
								}
							}
						}
						IEnumerable<XElement> enumerable2 = xelement.Elements("maplines");
						if (enumerable2 != null)
						{
							foreach (XElement xelement3 in enumerable2)
							{
								foreach (XElement element2 in xelement3.Elements("mapline"))
								{
									MapLineConfig mapLineConfig = new MapLineConfig();
									if (mapLineConfig.Load(element2))
									{
										this.mlcList.Add(mapLineConfig);
									}
								}
							}
						}
						IEnumerable<XElement> enumerable3 = xelement.Elements("mapplayers");
						if (enumerable3 != null)
						{
							foreach (XElement xelement4 in enumerable3)
							{
								foreach (XElement element3 in xelement4.Elements("mapplayer"))
								{
									MapPlayerConfig mapPlayerConfig = new MapPlayerConfig();
									if (mapPlayerConfig.Load(element3))
									{
										this.mpcList.Add(mapPlayerConfig);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
                Solarmax.Singleton<LoggerSystem>.Instance.Error("data/maplist/" + this.name + ".xml resource failed " + ex.ToString(), new object[0]);
			}
		}

		public void LoadExter(string filePath)
		{
			this.mlcList.Clear();
			this.mbcList.Clear();
			this.mpcList.Clear();
			try
			{
				WWW www = new WWW(filePath);
				while (!www.isDone)
				{
				}
				if (!string.IsNullOrEmpty(www.text))
				{
					XElement xelement = XDocument.Parse(www.text).Element("map");
					if (xelement != null)
					{
						this.id = xelement.GetAttribute("id", string.Empty);
						this.player_count = Convert.ToInt32(xelement.GetAttribute("player_count", "0"));
						this.vertical = Convert.ToBoolean(xelement.GetAttribute("vertical", string.Empty));
						this.audio = xelement.GetAttribute("audio", string.Empty);
						this.defaultai = xelement.GetAttribute("defaultAIStrategy", "-1");
						this.teamAITypes = xelement.GetAttribute("teamAITypes", string.Empty);
						if (this.teamAITypes != string.Empty)
						{
							foreach (string value in this.teamAITypes.Split(new char[]
							{
								','
							}))
							{
								this.teamAiType.Add(Convert.ToInt32(value));
							}
						}
						for (int j = this.teamAiType.Count; j < LocalPlayer.MaxTeamNum; j++)
						{
							this.teamAiType.Add(-1);
						}
						this.teamFriend = xelement.GetAttribute("teamFriend", string.Empty);
						if (this.teamFriend != string.Empty)
						{
							foreach (string value2 in this.teamFriend.Split(new char[]
							{
								','
							}))
							{
								this.teamFriendList.Add(Convert.ToInt32(value2));
							}
						}
						IEnumerable<XElement> enumerable = xelement.Elements("mapbuildings");
						if (enumerable != null)
						{
							foreach (XElement xelement2 in enumerable)
							{
								foreach (XElement element in xelement2.Elements("mapbuilding"))
								{
									MapBuildingConfig mapBuildingConfig = new MapBuildingConfig();
									if (mapBuildingConfig.Load(element))
									{
										this.mbcList.Add(mapBuildingConfig);
									}
								}
							}
						}
						IEnumerable<XElement> enumerable2 = xelement.Elements("maplines");
						if (enumerable2 != null)
						{
							foreach (XElement xelement3 in enumerable2)
							{
								foreach (XElement element2 in xelement3.Elements("mapline"))
								{
									MapLineConfig mapLineConfig = new MapLineConfig();
									if (mapLineConfig.Load(element2))
									{
										this.mlcList.Add(mapLineConfig);
									}
								}
							}
						}
						IEnumerable<XElement> enumerable3 = xelement.Elements("mapplayers");
						if (enumerable3 != null)
						{
							foreach (XElement xelement4 in enumerable3)
							{
								foreach (XElement element3 in xelement4.Elements("mapplayer"))
								{
									MapPlayerConfig mapPlayerConfig = new MapPlayerConfig();
									if (mapPlayerConfig.Load(element3))
									{
										this.mpcList.Add(mapPlayerConfig);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
                Solarmax.Singleton<LoggerSystem>.Instance.Error("data/maplist/" + this.name + ".xml resource failed " + ex.ToString(), new object[0]);
			}
		}

		public bool Delete()
		{
			bool result = false;
			try
			{
				string path = string.Empty;
				path = Application.streamingAssetsPath + this.Path();
				if (File.Exists(path))
				{
					File.Delete(path);
					result = true;
				}
			}
			catch (Exception ex)
			{
                Solarmax.Singleton<LoggerSystem>.Instance.Error("data/maplist/" + this.name + ".xml delete failed " + ex.ToString(), new object[0]);
			}
			return result;
		}

		public bool Save()
		{
			bool result = false;
			try
			{
				string empty = string.Empty;
				XmlTextWriter xmlTextWriter = new XmlTextWriter(Application.dataPath + "/Res/" + this.Path(), new UTF8Encoding(false));
				xmlTextWriter.Formatting = Formatting.Indented;
				xmlTextWriter.WriteStartDocument();
				xmlTextWriter.WriteStartElement("map");
				xmlTextWriter.WriteAttributeString("id", this.name);
				xmlTextWriter.WriteAttributeString("player_count", this.player_count.ToString());
				xmlTextWriter.WriteAttributeString("vertical", this.vertical.ToString());
				xmlTextWriter.WriteAttributeString("audio", this.audio);
				xmlTextWriter.WriteAttributeString("defaultAIStrategy", this.defaultai);
				xmlTextWriter.WriteAttributeString("teamAITypes", this.teamAITypes);
				xmlTextWriter.WriteAttributeString("teamFriend", this.teamFriend);
				if (this.mlcList.Count > 0)
				{
					xmlTextWriter.WriteStartElement("maplines");
					foreach (MapLineConfig mapLineConfig in this.mlcList)
					{
						xmlTextWriter.WriteStartElement("mapline");
						xmlTextWriter.WriteAttributeString("point1", mapLineConfig.point1);
						xmlTextWriter.WriteAttributeString("point2", mapLineConfig.point2);
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
				}
				if (this.mbcList.Count > 0)
				{
					xmlTextWriter.WriteStartElement("mapbuildings");
					foreach (MapBuildingConfig mapBuildingConfig in this.mbcList)
					{
						xmlTextWriter.WriteStartElement("mapbuilding");
						xmlTextWriter.WriteAttributeString("id", mapBuildingConfig.id);
						xmlTextWriter.WriteAttributeString("type", mapBuildingConfig.type);
						xmlTextWriter.WriteAttributeString("size", mapBuildingConfig.size.ToString());
						xmlTextWriter.WriteAttributeString("x", mapBuildingConfig.x.ToString());
						xmlTextWriter.WriteAttributeString("y", mapBuildingConfig.y.ToString());
						xmlTextWriter.WriteAttributeString("camption", mapBuildingConfig.camption.ToString());
						xmlTextWriter.WriteAttributeString("tag", mapBuildingConfig.tag);
						xmlTextWriter.WriteAttributeString("orbit", mapBuildingConfig.orbit.ToString());
						xmlTextWriter.WriteAttributeString("orbitParam1", mapBuildingConfig.orbitParam1);
						xmlTextWriter.WriteAttributeString("orbitParam2", mapBuildingConfig.orbitParam2);
						xmlTextWriter.WriteAttributeString("orbitClockWise", mapBuildingConfig.orbitClockWise.ToString());
						xmlTextWriter.WriteAttributeString("fAngle", mapBuildingConfig.fAngle.ToString());
						if (mapBuildingConfig.orbitRevoSpeed != 12)
						{
							xmlTextWriter.WriteAttributeString("orbitRevoSpeed", mapBuildingConfig.orbitRevoSpeed.ToString());
						}
						if (mapBuildingConfig.shipProduceOverride != -1)
						{
							xmlTextWriter.WriteAttributeString("shipProduceOverride", mapBuildingConfig.shipProduceOverride.ToString());
						}
						if (mapBuildingConfig.lasergunRange != 0f)
						{
							xmlTextWriter.WriteAttributeString("lasergunAngle", mapBuildingConfig.lasergunAngle.ToString());
							xmlTextWriter.WriteAttributeString("lasergunRange", mapBuildingConfig.lasergunRange.ToString());
							xmlTextWriter.WriteAttributeString("lasergunRotateSkip", mapBuildingConfig.lasergunRotateSkip.ToString());
						}
						xmlTextWriter.WriteAttributeString("transformBulidingID", mapBuildingConfig.transformBulidingID);
						xmlTextWriter.WriteAttributeString("curseDelay", mapBuildingConfig.curseDelay.ToString());
						if (!string.IsNullOrEmpty(mapBuildingConfig.aistrategy) && mapBuildingConfig.aistrategy != "-1")
						{
							xmlTextWriter.WriteAttributeString("aistrategy", mapBuildingConfig.aistrategy);
						}
						if (mapBuildingConfig.type == "BarrierPoint")
						{
							xmlTextWriter.WriteAttributeString("bpRange", mapBuildingConfig.fbpRange.ToString());
						}
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
				}
				if (this.mpcList.Count > 0)
				{
					xmlTextWriter.WriteStartElement("mapplayers");
					foreach (MapPlayerConfig mapPlayerConfig in this.mpcList)
					{
						xmlTextWriter.WriteStartElement("mapplayer");
						xmlTextWriter.WriteAttributeString("id", mapPlayerConfig.id);
						xmlTextWriter.WriteAttributeString("tag", mapPlayerConfig.tag);
						xmlTextWriter.WriteAttributeString("ship", mapPlayerConfig.ship.ToString());
						xmlTextWriter.WriteAttributeString("camption", mapPlayerConfig.camption.ToString());
						xmlTextWriter.WriteEndElement();
					}
					xmlTextWriter.WriteEndElement();
				}
				xmlTextWriter.WriteEndElement();
				xmlTextWriter.WriteEndDocument();
				xmlTextWriter.Close();
			}
			catch (Exception ex)
			{
                Solarmax.Singleton<LoggerSystem>.Instance.Error("data/maplist/" + this.name + ".xml save failed " + ex.ToString(), new object[0]);
			}
			return result;
		}

		public bool Verify()
		{
			return true;
		}

		public void LoadExtraData()
		{
		}

		public List<MapLineConfig> mlcList;

		public List<MapBuildingConfig> mbcList;

		public List<MapPlayerConfig> mpcList;

		public string name;

		public const string root = "map";

		public int player_count;

		public bool vertical;

		public string audio;

		public string defaultai;

		public string id;

		public string teamAITypes;

		public List<int> teamAiType;

		public string teamFriend;

		public List<int> teamFriendList = new List<int>();
	}
}
