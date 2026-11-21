namespace DataCenter
{
	public abstract class AbsDataModel
	{
		public IAssembleDB Asse;

		public abstract void InitData();

		public virtual void ReadLocalData()
		{
		}

		public virtual void DelLocalData()
		{
		}

		public virtual void WriteLocalData()
		{
		}
	}
}
