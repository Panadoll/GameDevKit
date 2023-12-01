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
 * ÿ֡���ᴴ��һ���µ�ί��ʵ������ᵼ��Ƶ�����ڴ����͹̶����������գ�GC����
 * Ϊ�˱������������Ӧ�þ���������ÿ֡�д���ί�С����Խ�ί��ʵ������������ֶ��У�
 * �Ա�����Ҫʱ�ظ�ʹ����ͬ��ί��ʵ���������ǲ��ϵش����µ�ί�С����⣬�������⽫�������ݸ���Ҫί����Ϊ�����ķ�����
 * ֱ������Ҫ�ĵط�������Ӧ�ķ������������Լ����ڴ����ͽ����������յ�Ƶ�ʣ�������ܺ�Ч��
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