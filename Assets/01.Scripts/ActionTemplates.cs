using System;

public class ActionTemplate
{
    private Action action = null;

    public void SetAction(Action action)
    {
        this.action = action;
    }
    public void RegistAction(Action action)
    {
        this.action -= action;
        this.action += action;
    }
    public void RemoveAction(Action action)
    {
        this.action -= action;
    }

    public void Action()
    {
        if (action == null) return;
        action();
    }
}

public class ActionTemplate<T>
{
    private Action<T> action = null;

    public void SetAction(Action<T> action)
    {
        this.action = action;
    }
    public void RegistAction(Action<T> action)
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

public class ActionTemplate<T1, T2>
{
    private Action<T1, T2> action = null;

    public void SetAction(Action<T1, T2> action)
    {
        this.action = action;
    }
    public void RegistAction(Action<T1, T2> action)
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
