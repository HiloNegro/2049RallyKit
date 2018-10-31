using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimatedImage : MonoBehaviour
{
    /// <summary>
    /// The list of sprites to include in this animation.
    /// </summary>
    public Sprite[] SpriteNames;

    /// <summary>
    /// The list of frames of this animation.
    /// </summary>
    public int[] Frames;

    /// <summary>
    /// 
    /// </summary>
    public bool AutoStart = true;

    /// <summary>
    /// Will the animation loop?
    /// </summary>
    public bool Loop = true;

    /// <summary>
    /// Frames per second of the animation.
    /// </summary> 
    public float FPS = 30;

    /// <summary>
    /// Should the sprite be pixel perfect?
    /// </summary>
    public bool PixelPerfect = true;

    public bool PreserveAspect = false;

    /// <summary>
    /// The current frame position
    /// </summary>
    private int _position = 0;
    /// <summary>
    /// 
    /// </summary>
    private float _delta = 0.0f;

    /// <summary>
    /// Used to signal if the animation is currently playing.
    /// </summary>
    private bool _active = true;

    /// <summary>
    /// The sprite to animate.
    /// </summary>
    public Image spriteRenderer;

    /// <summary>
    /// 
    /// </summary>
    private Vector2 _scaleRatio = Vector2.one;

    private void Start()
    {
        ResetAnimation();
    }

    public void ResetAnimation()
    {
        _active = AutoStart;

        if (SpriteNames == null || SpriteNames.Length <= 0)
        {
            _active = false;
            return;
        }

        _scaleRatio = spriteRenderer.rectTransform.sizeDelta;
        _scaleRatio.x *= (1.0f / SpriteNames[0].texture.width);
        _scaleRatio.y *= (1.0f / SpriteNames[0].texture.height);

        if (Frames == null || Frames.Length == 0)
        {
            Frames = new int[SpriteNames.Length];
            for (int i = 0; i < SpriteNames.Length; i++)
                Frames[i] = i;
        }
    }

    void Update()
    {
        if (!_active || SpriteNames == null || Frames == null)
            return;

        if (SpriteNames.Length <= Frames.Length && Frames.Length > 1 && Application.isPlaying && FPS > 0)
        {

            _delta += Time.deltaTime;

            float rate = 1f / FPS;

            if (rate < _delta)
            {

                _delta = (rate > 0f) ? _delta - rate : 0f;

                spriteRenderer.sprite = SpriteNames[Frames[_position]];

                if (PixelPerfect)
                {
                    spriteRenderer.SetNativeSize();
                }
                else
                {

                    Vector2 spriteDimensions = new Vector2(SpriteNames[Frames[_position]].texture.width, SpriteNames[Frames[_position]].texture.height);
                    spriteRenderer.rectTransform.sizeDelta = Vector2.Scale(spriteDimensions, _scaleRatio);
                }

                if (PreserveAspect)
                    spriteRenderer.preserveAspect = PreserveAspect;

                if (++_position >= Frames.Length)
                {
                    _position = 0;
                    _active = Loop;
                }
            }
        }
    }

    public bool IsPlaying()
    {
        return _active;
    }

    public void Play()
    {
        if (SpriteNames == null || Frames == null)
            return;

        if (SpriteNames.Length <= Frames.Length && Frames.Length > 1 && FPS > 0 && spriteRenderer != null)
        {
            _active = true;
            _position = 0;

            spriteRenderer.sprite = SpriteNames[Frames[_position]];

            if (PixelPerfect)
            {
                spriteRenderer.SetNativeSize();
            }
            else
            {

                Vector2 spriteDimensions = new Vector2(SpriteNames[Frames[_position]].texture.width, SpriteNames[Frames[_position]].texture.height);
                spriteRenderer.rectTransform.sizeDelta = Vector2.Scale(spriteDimensions, _scaleRatio);
            }
        }
    }

    public void Pause(bool pause)
    {
        _active = pause;
    }

    public void Resume()
    {
        _active = true;
    }

    public void Stop()
    {
        _active = false;
    }
}