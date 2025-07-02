using System;
using System.Collections.Generic;

namespace Solarmax
{
	public class DataHandlerSystem : Singleton<DataHandlerSystem>, Lifecycle
	{
		public bool Init()
		{
			Singleton<LoggerSystem>.Instance.Debug("DataHandlerSystem   init   begin", new object[0]);
			this.RegistDataHandler(Singleton<FriendDataHandler>.Instance);
			this.RegistDataHandler(Singleton<LevelDataHandler>.Instance);
			this.RegistDataHandler(Singleton<UserSyncSysteam>.Instance);
			this.RegistDataHandler(Singleton<OrderManager>.Instance);
			this.RegistDataHandler(Singleton<ReconnectHandler>.Instance);
			this.RegistDataHandler(Singleton<ItemDataHandler>.Instance);
			bool result = this.doInit();
			Singleton<LoggerSystem>.Instance.Debug("DataHandlerSystem   init   end", new object[0]);
			return result;
		}

		public void Tick(float interval)
		{
			int i = 0;
			int count = this.mDataHandler.Count;
			while (i < count)
			{
				this.mDataHandler[i].Tick(interval);
				i++;
			}
		}

		public void Destroy()
		{
			this.mDataHandler.Clear();
		}

		private bool doInit()
		{
			bool flag = true;
			int i = 0;
			int count = this.mDataHandler.Count;
			while (i < count)
			{
				flag &= this.mDataHandler[i].Init();
				i++;
			}
			return flag;
		}

		private void RegistDataHandler(IDataHandler dataHandler)
		{
			this.mDataHandler.Add(dataHandler);
		}

		private List<IDataHandler> mDataHandler = new List<IDataHandler>();
	}
}
