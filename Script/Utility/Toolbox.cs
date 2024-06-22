using Godot;
using System;
using System.Collections.Generic;

public static class Toolbox
{
    public static void FindAndPopulate<T>(Node node, List<T> list) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T item)
            {
                list.Add(item);
            }

            FindAndPopulate(child, list);
        }
    }
}

