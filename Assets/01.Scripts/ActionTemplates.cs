using System;

public class ActionTemplates<T>
{
    private Action<T> action = null;

    public void AddAction(Action<T> action)
    {
        this.action -= action;
        this.action += action;
    }
    public void RemoveAction(Action<T> action)
    {
        this.action -= action;
    }

    public void Action(T v)
    {
        if (action == null) return;
        action(v);
    }
}

public class ActionTemplates<T1, T2>
{
    private Action<T1, T2> action = null;
    
    public void AddAction(Action<T1, T2> action)
    {
        this.action -= action;
        this.action += action;
    }
    public void RemoveAction(Action<T1, T2> action)
    {
        this.action -= action;
    }

    public void Action(T1 v1, T2 v2)
    {
        if (action == null) return;
        action(v1, v2);
    }
}
