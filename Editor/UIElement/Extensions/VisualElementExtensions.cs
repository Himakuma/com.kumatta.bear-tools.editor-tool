using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;



namespace Kumatta.BearTools.Editor
{

    public static class VisualElementExtensions
    {

        public static T FindChildByName<T>(this VisualElement ve, string name) where T : VisualElement
        {
            foreach (var child in ve.Children())
            {
                if (child.name == name)
                {
                    return (T)child;
                }

                if (0 < child.childCount)
                {
                    var findVe = child.FindChildByName<T>(name);
                    if (findVe != null)
                    {
                        return findVe;
                    }
                }
            }
            return null;
        }






    }
}
