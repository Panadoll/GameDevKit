using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Panadoll
{
    [CustomEditor(typeof(SpriteAnimationObject))]
    public class SpriteAnimationCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SpriteAnimationObject spriteAnimation = (SpriteAnimationObject)target;
        }
    }
}