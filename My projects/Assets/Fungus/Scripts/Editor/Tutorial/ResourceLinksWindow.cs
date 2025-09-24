using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fungus.Tutorial
{
    public class ResourceLinksWindow : EditorWindow
    {
        [MenuItem("Tools/Fungus/Tutorial/Online Resources")]
        public static ResourceLinksWindow ShowWindow()
        {
            var w = GetWindow(typeof(ResourceLinksWindow), true, "Online Resources", true);
            w.Show();
            return w as ResourceLinksWindow;
        }
    }
}
