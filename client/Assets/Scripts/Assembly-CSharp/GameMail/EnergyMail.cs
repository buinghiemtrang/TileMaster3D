using System;
using System.Collections.Generic;
using BestHTTP;
using GameTeam;
using Newtonsoft.Json;

namespace GameMail
{
	public class EnergyMail : MailBase
	{
		public int GetCanReceiveEnergyMaxNum()
		{
			if (DB.Bag.JudgeUnlimitedEnergy())
			{
				return 0;
			}
			int prop = DB.Bag.GetProp(PropID.Energy);
			int maxEnergyRecover = DB.Bag.GetMaxEnergyRecover();
			if (maxEnergyRecover - prop != 0 && prop <= maxEnergyRecover)
			{
				return maxEnergyRecover - prop;
			}
			return 0;
		}

		public void GetEnergy(Action callback)
		{
			if (!DataManager.I.bLogined)
			{
				return;
			}
			RequestMgr.Headers.Clear();
			OwnHttp.CheckRepeatRequestsMgr.I.Creat("Mail_GetEnergy", HTTPMethods.Get, "https://service.fuqiangame.com/team/GetLives", delegate(OwnHttp.ResultInfo info)
			{
				OwnHttp.ServerDataListRoot<EnergysInfo> serverDataListRoot = OwnHttp.ParsingServerData.SendListInfo<EnergysInfo>(info);
				if (serverDataListRoot != null && serverDataListRoot.code == 1 && serverDataListRoot.Data != null && serverDataListRoot.Data.Count != 0)
				{
					AnalysisEnergy(serverDataListRoot.Data[0].Lives);
					callback.InvokeSafe();
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddCustomTimeout(15f)
				?.Send();
		}

		public void AnalysisEnergy(Dictionary<string, EnergysInfo.EnergyInfo> infos)
		{
			if (infos != null)
			{
				foreach (EnergysInfo.EnergyInfo value in infos.Values)
				{
					if (!DB.Mail.EnergyMails.ContainsKey(value.GUID))
					{
						MailData data = new MailData(value);
						DB.Mail.Save(value.GUID, data);
					}
				}
			}
		}

		public void ReceiveEnergy(List<MailData> mails, Action<bool> callback)
		{
			if (!DataManager.I.bLogined)
			{
				callback.InvokeSafe(false);
				return;
			}
			List<string> list = new List<string>();
			mails.ForEach(delegate(MailData m)
			{
				list.Add(m.Id);
			});
			RequestMgr.Headers.Clear();
			OwnHttp.CheckRepeatRequestsMgr.I.Creat("Mail_ReceiveEnergy", HTTPMethods.Post, "https://service.fuqiangame.com/team/ReceiveLive", delegate(OwnHttp.ResultInfo info)
			{
				OwnHttp.ServerDataRootBase serverDataRootBase = OwnHttp.ParsingServerData.SendInfo(info);
				if (serverDataRootBase != null)
				{
					if (serverDataRootBase.code == 1)
					{
						Receive(mails);
						callback.InvokeSafe(true);
					}
					else
					{
						callback.InvokeSafe(false);
					}
				}
			})
				?.AddHeader("Authorization", "Bearer " + DataManager.I.Token)
				?.AddHeader("Content-Type", "application/json")
				?.AddParameter(JsonConvert.SerializeObject(new
				{
					GUIDS = list
				}))
				?.AddCustomTimeout(5f)
				?.Send();
		}
	}
}
