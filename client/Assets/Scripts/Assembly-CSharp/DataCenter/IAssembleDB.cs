using System;

namespace DataCenter
{
	public interface IAssembleDB
	{
		string GetName();

		Type GetClsType();

		object Create();

		object GetData();

		void UploadDataAllToRemote();

		void SetData(object value);

		void InitLocalData();

		void ReadLocalData();

		void DelLocalData();

		void WriteLocalData();

		void ReadRemoteData(string sBody);

		string GetRemoteDataToString(string sBody);

		void InitQueueDispatch(string userid);

		void DequeueEnabled();

		void DequeueDisable();

		void Write(string key, object value);

		void Send(string key, object value);

		void Save(string key, object value);

		void SaveForced(string key, object value);

		void SetValue<T1>(T1 cls, string key, object value);

		void SetValue(string key, object value);
	}
}
