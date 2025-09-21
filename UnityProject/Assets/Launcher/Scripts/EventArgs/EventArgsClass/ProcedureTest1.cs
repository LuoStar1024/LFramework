// 自动生成于：2025/8/28 17:40:42

using LFramework;

namespace Launcher
{

	public class ProcedureTest1 : GameEventArgs
	{
		public static readonly int EventId = typeof(ProcedureTest1).GetHashCode();

		public ProcedureTest1 ()
		{
			TestId = default(int);
		}

		public override int Id
		{
			get
			{
				return EventId;
			}
		}

		public int TestId
		{
			get;
			private set;
		}

		public static ProcedureTest1 Create(int testId)
		{
			ProcedureTest1 procedureTest1 = ReferencePool.Acquire<ProcedureTest1>();
			procedureTest1.TestId = testId;
			return procedureTest1;
		}
		public override void Clear()
		{
			TestId = default(int);
		}
	}
}
