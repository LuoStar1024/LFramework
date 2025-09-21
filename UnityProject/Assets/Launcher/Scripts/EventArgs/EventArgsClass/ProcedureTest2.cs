// 自动生成于：2025/8/28 17:38:04

using LFramework;

namespace Launcher
{

	public class ProcedureTest2 : GameEventArgs
	{
		public static readonly int EventId = typeof(ProcedureTest2).GetHashCode();

		public ProcedureTest2 ()
		{
			Name = default(string);
		}

		public override int Id
		{
			get
			{
				return EventId;
			}
		}

		public string Name
		{
			get;
			private set;
		}

		public static ProcedureTest2 Create(string name)
		{
			ProcedureTest2 procedureTest2 = ReferencePool.Acquire<ProcedureTest2>();
			procedureTest2.Name = name;
			return procedureTest2;
		}
		public override void Clear()
		{
			Name = default(string);
		}
	}
}