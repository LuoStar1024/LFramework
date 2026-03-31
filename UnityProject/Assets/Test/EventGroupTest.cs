using GameLogic;
using LFramework;

public class EventGroupTest
{
    private IEventManager _eventComponent;
    
    public EventGroupTest()
    {
        _eventComponent = LFrameworkEntry.GetModule<IEventManager>();
        _eventComponent.RegisterGroup(this);
    }
        
    public static readonly int Test0Id = EventRuntimeId.ToRuntimeId("EventGroupTest.Test0Id");
    public void Test0()
    {
        _eventComponent.Fire(Test0Id);
    }
    
    public static readonly int TestIntId = EventRuntimeId.ToRuntimeId("EventGroupTest.TestIntId");
    public void TestInt(int value)
    {
        _eventComponent.Fire(TestIntId, value);
    }
    
    public static readonly int TestFloatId = EventRuntimeId.ToRuntimeId("EventGroupTest.TestFloatId");
    public void TestFloat(float value)
    {
        _eventComponent.Fire(TestFloatId, value);
    }
    
    public static readonly int TestStrId = EventRuntimeId.ToRuntimeId("EventGroupTest.TestStrId");
    public void TestStr(string value)
    {
        _eventComponent.Fire(TestStrId, value);
    }
    
    public static readonly int TestObjId = EventRuntimeId.ToRuntimeId("EventGroupTest.TestObjId");
    public void TestObj(Student value)
    {
        _eventComponent.Fire(TestObjId, value);
    }
}
