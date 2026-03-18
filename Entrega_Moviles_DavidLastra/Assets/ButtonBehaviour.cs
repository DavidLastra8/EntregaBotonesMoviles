using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviour : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float DOUBLE_CLICK_WINDOW = 0.3f;
    private const float LONG_PRESS_THRESHOLD = 0.4f;

    private enum ButtonState { Idle, WaitingForRelease, WaitingForDoubleClick }
    private ButtonState _state = ButtonState.Idle;
    private bool _isSecondPress = false;

    private CancellationTokenSource _longPressCts;
    private CancellationTokenSource _doubleClickCts;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_state == ButtonState.Idle)
        {
            _isSecondPress = false;
            _state = ButtonState.WaitingForRelease;

            _longPressCts = new CancellationTokenSource();
            WaitForLongPress(_longPressCts.Token).Forget();
        }
        else if (_state == ButtonState.WaitingForDoubleClick)
        {
            CancelDoubleClickTimer();
            _isSecondPress = true;
            _state = ButtonState.WaitingForRelease;

            _longPressCts = new CancellationTokenSource();
            WaitForLongPress(_longPressCts.Token).Forget();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_state != ButtonState.WaitingForRelease) return;

        CancelLongPressTimer();

        if (_isSecondPress)
        {
            _isSecondPress = false;
            TriggerDoubleClick();
        }
        else
        {
            _state = ButtonState.WaitingForDoubleClick;

            _doubleClickCts = new CancellationTokenSource();
            WaitForDoubleClickWindow(_doubleClickCts.Token).Forget();
        }
    }

    private async UniTaskVoid WaitForLongPress(CancellationToken token)
    {
        bool cancelled = await UniTask
            .Delay(System.TimeSpan.FromSeconds(LONG_PRESS_THRESHOLD), cancellationToken: token)
            .SuppressCancellationThrow();

        if (cancelled) return;

        CancelDoubleClickTimer();
        _isSecondPress = false;
        TriggerLongPress();
    }

    private async UniTaskVoid WaitForDoubleClickWindow(CancellationToken token)
    {
        bool cancelled = await UniTask
            .Delay(System.TimeSpan.FromSeconds(DOUBLE_CLICK_WINDOW), cancellationToken: token)
            .SuppressCancellationThrow();

        if (cancelled) return;

        TriggerShortClick();
    }


    private void TriggerShortClick()
    {
        Debug.Log("Action A — Short Click");
        ResetToIdle();
    }

    private void TriggerLongPress()
    {
        Debug.Log("Action B — Long Press");
        ResetToIdle();
    }

    private void TriggerDoubleClick()
    {
        Debug.Log("Action C — Double Click");
        ResetToIdle();
    }


    private void ResetToIdle()
    {
        _state = ButtonState.Idle;
        _isSecondPress = false;
    }

    private void CancelLongPressTimer()
    {
        if (_longPressCts == null) return;
        _longPressCts.Cancel();
        _longPressCts.Dispose();
        _longPressCts = null;
    }

    private void CancelDoubleClickTimer()
    {
        if (_doubleClickCts == null) return;
        _doubleClickCts.Cancel();
        _doubleClickCts.Dispose();
        _doubleClickCts = null;
    }

    private void OnDestroy()
    {
        CancelLongPressTimer();
        CancelDoubleClickTimer();
    }
}
