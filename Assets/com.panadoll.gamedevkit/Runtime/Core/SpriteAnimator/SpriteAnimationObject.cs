using System;
using System.Collections.Generic;
using UnityEngine;

namespace Panadoll
{
    [CreateAssetMenu]
    public class SpriteAnimationObject : ScriptableObject
    {
        public List<SpriteAnimation> SpriteAnimations = new List<SpriteAnimation>(5);
    }

    [Serializable]
    public enum SpriteAnimationType
    {
        Looping = 0,
        PlayOnce = 1
    }

    [Serializable]
    public class SpriteAnimation
    {
        public string Name = "";
        public int FPS = 0;
        public List<SpriteAnimationFrame> Frames = new();
        public SpriteAnimationType SpriteAnimationType = SpriteAnimationType.Looping;
    }

    [Serializable]
    public class SpriteAnimationFrame
    {
        public Sprite sprite;
        public string action = "";
    }
}
