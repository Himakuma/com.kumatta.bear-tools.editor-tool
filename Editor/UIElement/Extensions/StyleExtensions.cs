using UnityEngine;
using UnityEngine.UIElements;




namespace Kumatta.BearTools.Editor
{

    public static class StyleExtensions
    {

        public static void BorderColor(this IStyle style, Color color)
        {
            var styleColor = new StyleColor(color);
            style.borderBottomColor = styleColor;
            style.borderTopColor = styleColor;
            style.borderLeftColor = styleColor;
            style.borderRightColor = styleColor;
        }

        public static void BorderSize(this IStyle style, float size)
        {
            var styleSize = new StyleFloat(size);
            style.borderBottomWidth = styleSize;
            style.borderTopWidth = styleSize;
            style.borderLeftWidth = styleSize;
            style.borderRightWidth = styleSize;
        }

    }
}
