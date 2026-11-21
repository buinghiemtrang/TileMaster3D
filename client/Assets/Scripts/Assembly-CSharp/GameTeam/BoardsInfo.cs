using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameTeam
{
	public class BoardsInfo
	{
		public class BoardInfo
		{
			public string _t;

			public string GUID;

			public string Type;

			public string UserId;

			public string UserName;

			public DateTime Created;

			public int? Medal;

			public string Content;

			public int LifeReceived;

			public int LifeMax;

			public string[] LifeSenders;

			public DateTime EndTime;

			public string Extension;

			public string[] Receivers;

			public BoardInfoType GetInfoType()
			{
				string type = Type;
				if (type == "JoinTeam")
				{
					return BoardInfoType.Join;
				}
				if (type == "LeaveTeam")
				{
					return BoardInfoType.Leave;
				}
				if (type == "RequestingLife")
				{
					return BoardInfoType.RequestingEnergy;
				}
				if (type == "RoyalPassActivity")
				{
					return BoardInfoType.RoyalPassActivity;
				}
				if (type == "ShortBattlePass")
				{
					return BoardInfoType.ShortBattlePass;
				}
				return BoardInfoType.Chat;
			}

			public Dictionary<string, string> GetExtension()
			{
				if (string.IsNullOrEmpty(Extension))
				{
					return null;
				}
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(Extension);
			}
		}

		public Dictionary<string, BoardInfo> Boards;
	}
}
