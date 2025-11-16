using System.Collections.Generic;
using System;
using UnityEngine;

public class SpriteController : MonoBehaviour
{
    private Sprite _lookRightSprite, _lookLeftSprite; 
    private SpriteRenderer _spriteRenderer;
    private PlayerController _playerController;

    private Dictionary<Type, Color> _stateColors = new Dictionary<Type, Color>
    {
        { typeof(IdleState), Color.white },
        { typeof(RunningState), Color.blue },
        { typeof(JumpState), Color.green },
        { typeof(FallingState), Color.red },
        { typeof(LevitatingState), Color.cyan },
        { typeof(SlidingSlopeState), Color.magenta },
        { typeof(SlidingWallState), Color.gray },
        { typeof(GrabAngleState), Color.yellow },
        { typeof(ClimbingAngleState), Color.yellow }
    };

    public void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerController = GetComponentInParent<PlayerController>();
        if (_playerController == null)
        {
            Debug.LogError("PlayerController is not assigned in SpriteController.");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        HandleDebugSpriteColor();
    }


    private void HandleDebugSpriteColor()
    {
        if (_playerController.MovementConfig.DebugMode)
        {
            if (_playerController.CurrentMovementState != null)
            {
                if (_stateColors.TryGetValue(_playerController.CurrentMovementState.GetType(), out Color stateColor))
                {
                    _spriteRenderer.color = stateColor;
                }
                else
                {
                    _spriteRenderer.color = Color.white; // Default color if state not found
                }
            }
        }
    }
}
