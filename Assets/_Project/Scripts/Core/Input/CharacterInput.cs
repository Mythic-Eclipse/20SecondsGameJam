using UnityEngine;
using UnityEngine.InputSystem;

public class InputVectorProvider
{
    private Vector2 _inputVector;
    private PlayerInput _playerInput;
    private InputAction _moveAction => _playerInput.actions["Move"];

    public Vector2 InputVector => _inputVector;


    public InputVectorProvider(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public void Refresh()
    {
        HandleInputVector();
    }
    
    // Handle the input vector based on the current input device
    private void HandleInputVector()
    {
        if (_moveAction == null || _moveAction.activeControl == null)
        {
            _inputVector = Vector2.zero;
            return;
        }
        if (_moveAction.activeControl.device is Keyboard)
        {
            Vector2 rawInputVector = _moveAction.ReadValue<Vector2>();
            _inputVector = new Vector2(Mathf.Round(rawInputVector.x), Mathf.Round(rawInputVector.y));
        }
        else
        {
            _inputVector = _moveAction.ReadValue<Vector2>();
            if (_inputVector.x < 0.5f && _inputVector.x > -0.5f)
            {
                _inputVector.x = 0;
            }
        }
    }
}
