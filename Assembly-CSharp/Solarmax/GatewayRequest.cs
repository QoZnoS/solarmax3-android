using System;

namespace Solarmax
{
	public static class GatewayRequest
	{
		public static GatewayResponse Response
		{
			get
			{
				return (GatewayRequest.mRequestor != null) ? GatewayRequest.mRequestor.Response : null;
			}
		}

		public static void GetGateway(ServerListItemConfig serverConfig, Action onResponseDelegate)
		{
			onResponseDelegate();
		}

		private static WebRequestor<GatewayResponse> mRequestor;
	}
}
