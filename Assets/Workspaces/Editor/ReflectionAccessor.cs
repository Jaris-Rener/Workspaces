namespace Howl.Workspaces
{
    using System;
    using System.Linq;
    using UnityEditor;
    using System.Reflection;
    using UnityEditor.UIElements;
    using UnityEngine;

    public static class ReflectionAccessor
    {
        public static void ShowColorPicker(Action<Color> onColorChanged, Color color, bool showAlpha = true, bool hdr = false)
        {
            var assembly = Assembly.GetAssembly(typeof(ColorField));
            var type = assembly.GetType("UnityEditor.ColorPicker");
            if (type == null)
                return;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var showMethod = methods.FirstOrDefault(x => x.Name == "Show" && x.GetParameters()[0].ParameterType == typeof(Action<Color>));
            if (showMethod == null)
                return;

            showMethod.Invoke(null, new object[]{
                onColorChanged,
                color,
                showAlpha,
                hdr});

            // ColorPicker.Show(c =>
            // {
            //     this.showMixedValue = false;
            //     this.value = c;
            // }, this.value, this.m_ShowAlpha, this.m_HDR);
        }
    }
}