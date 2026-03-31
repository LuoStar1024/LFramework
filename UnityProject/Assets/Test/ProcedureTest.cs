using System.Collections;
using System.Collections.Generic;
using LFramework;
using UnityEngine;

public class ProcedureTest : LFramework.ProcedureBase
{
    private float _timer = 0;
    private EventGroupTest _test;
    
    protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
    {
        base.OnEnter(procedureOwner);
        
        _test = new EventGroupTest();
        
        var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
        eventComponent.Subscribe(EventGroupTest.Test0Id, OnTest0);
        eventComponent.Subscribe<int>(EventGroupTest.TestIntId, OnTestInt);
        eventComponent.Subscribe<float>(EventGroupTest.TestFloatId, OnTestFloat);
        eventComponent.Subscribe<string>(EventGroupTest.TestStrId, OnTestStr);
        eventComponent.Subscribe<Student>(EventGroupTest.TestObjId, OnTestObj);
    }

    protected override void OnLeave(IFsm<IProcedureManager> procedureOwner, bool isShutdown)
    {
        base.OnLeave(procedureOwner, isShutdown);
        
        var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
        eventComponent.Unsubscribe(EventGroupTest.Test0Id, OnTest0);
        eventComponent.Unsubscribe<int>(EventGroupTest.TestIntId, OnTestInt);
        eventComponent.Unsubscribe<float>(EventGroupTest.TestFloatId, OnTestFloat);
        eventComponent.Unsubscribe<string>(EventGroupTest.TestStrId, OnTestStr);
        eventComponent.Unsubscribe<Student>(EventGroupTest.TestObjId, OnTestObj);

        _test = null;
    }

    protected override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

        _timer += elapseSeconds;
        if (_timer > 2)
        {
            _timer -= 2;
            Debug.LogError("EventFireStart");
            var eventComponent = LFrameworkEntry.GetModule<IEventManager>();
            eventComponent.FireNow(EventGroupTest.Test0Id);
            eventComponent.FireNow(EventGroupTest.TestIntId, 2);
            eventComponent.FireNow(EventGroupTest.TestFloatId, 3.0f);
            eventComponent.FireNow(EventGroupTest.TestStrId, "4");
            eventComponent.FireNow(EventGroupTest.TestObjId, new Student(){name = "5"});
            
            eventComponent.Fire(EventGroupTest.Test0Id);
            eventComponent.Fire(EventGroupTest.TestIntId, 2);
            eventComponent.Fire(EventGroupTest.TestFloatId, 3.0f);
            eventComponent.Fire(EventGroupTest.TestStrId, "4");
            eventComponent.Fire(EventGroupTest.TestObjId, new Student(){name = "5"});
            
            eventComponent.FireGroup<EventGroupTest>().Test0();
            eventComponent.FireGroup<EventGroupTest>().TestInt(2);
            eventComponent.FireGroup<EventGroupTest>().TestFloat(3.0f);
            eventComponent.FireGroup<EventGroupTest>().TestStr("4");
            eventComponent.FireGroup<EventGroupTest>().TestObj(new Student(){name = "5"});
            
            Debug.LogError("EventFireEnd");
        }
    }

    private void OnTest0()
    {
        
    }

    private void OnTestInt(int value)
    {
        Debug.LogError(value);
    }
    
    private void OnTestFloat(float value)
    {
        Debug.LogError(value);
    }
    
    private void OnTestStr(string value)
    {
        Debug.LogError(value);
    }
    
    private void OnTestObj(Student value)
    {
        Debug.LogError(value.name);
    }
}

public class Student
{
    public string name;
}
