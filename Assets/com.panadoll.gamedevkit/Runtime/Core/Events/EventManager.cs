using Panadoll;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*
 * funcA(Action func){}
 * funcB(){}
 * void Update()
 * {
 *     funcA(funcB)
 * }
 * 每帧都会创建一个新的委托实例，这会导致频繁的内存分配和固定的垃圾回收（GC）。
 * 为了避免这种情况，应该尽量避免在每帧中创建委托。可以将委托实例保存在类的字段中，
 * 以便在需要时重复使用相同的委托实例，而不是不断地创建新的委托。另外，尽量避免将方法传递给需要委托作为参数的方法，
 * 直接在需要的地方调用相应的方法。这样可以减少内存分配和降低垃圾回收的频率，提高性能和效率
 * 
 */
public static class EventManager 
{
    private static readonly Dictionary<DelegateCallback, LinkedListNode<DelegateCallback>> lookup;
    private static readonly Dictionary<GameEventType, LinkedList<DelegateCallback>> events;
    static EventManager()
    {
        events = new Dictionary<GameEventType, LinkedList<DelegateCallback>>();
        lookup = new Dictionary<DelegateCallback, LinkedListNode<DelegateCallback>>();
        foreach (var gameEventType in Enum.GetValues(typeof(GameEventType)))
        {
            events.Add((GameEventType)gameEventType, new LinkedList<DelegateCallback>());
        }
    }

    public static void Add(GameEventType evt, DelegateCallback handler)
    {
        if (lookup.ContainsKey(handler)) return;

        lookup[handler] = events[evt].AddLast(handler);
    }

    public static void Send(GameEventType evt)
    {
        var node = events[evt].First;
        while (node != null)
        {
            node.Value.Invoke();
            node = node.Next;
        }
    }

    public static void Send<A>(GameEventType eventType, A a)
    {
        var node = events[eventType].First;
        while (node != null)
        {
            node.Value.DynamicInvoke(a);
            node = node.Next;
        }
    }

    public static void Cleanup()
    {

    }

}

public enum GameEventType
{
    EnterGame,
}