using System;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour, IDisposable
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private Transform _hoursArrow;
    [SerializeField] private Transform _minutesArrow;
    [SerializeField] private Transform _secondsArrow;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TextMeshProUGUI _errorText;

    private ITimeModel _timeModel;
    private ITimeSynchronizer _timeSynchronizer;
    private Action _synchronize;
    private Transform _draggingArrow;

    public int ServerIndex => _dropdown.value;

    public void Init(ITimeModel timeModel, ITimeSynchronizer timeSynchronizer, Action synchronize)
    {
        _timeModel = timeModel;
        _timeSynchronizer = timeSynchronizer;
        _synchronize = synchronize;

        _dropdown.options.Clear();
        _dropdown.AddOptions(timeSynchronizer.Servers);

        _timeSynchronizer.Finish += OnSynchronizerFinished;
        _dropdown.onValueChanged.AddListener(DropdownOnValueChanged);
        _inputField.onValueChanged.AddListener(InputFieldOnValueChanged);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hitCollider = Physics2D.OverlapPoint(Input.mousePosition);
            if (hitCollider != null)
            {
                if (hitCollider.gameObject == _hoursArrow.gameObject)
                    _draggingArrow = _hoursArrow;
                else if (hitCollider.gameObject == _minutesArrow.gameObject)
                    _draggingArrow = _minutesArrow;
                else if (hitCollider.gameObject == _secondsArrow.gameObject)
                    _draggingArrow = _secondsArrow;
            }
        }

        var minutesRaw = _timeModel.Seconds / 60;
        var hours = minutesRaw / 60;
        var minutes = minutesRaw % 60;
        var seconds = _timeModel.Seconds % 60;

        if (_draggingArrow)
        {
            var direction = (Input.mousePosition - _draggingArrow.position).normalized;
            var angle = (450 - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) % 360;

            var part = angle / 360;
            if (_draggingArrow == _hoursArrow)
                hours = 12 * part;
            else if (_draggingArrow == _minutesArrow)
                minutes = 60 * part;
            else
                seconds = 60 * part;

            if (Input.GetMouseButtonUp(0))
            {
                _draggingArrow = null;
                _timeModel.SetTime(((int)hours * 60 + (int)minutes) * 60 + seconds);
            }
        }

        SetRotationZ(_hoursArrow, -30 * (hours % 12));
        SetRotationZ(_minutesArrow, -6 * minutes);
        SetRotationZ(_secondsArrow, -6 * seconds);

        _inputField.text = $"{(int)hours:00} : {(int)minutes:00} : {(int)seconds:00}";
    }

    private void OnSynchronizerFinished(string result)
    {
        _errorText.text = result;
    }

    private static void SetRotationZ(Transform transform, double z)
    {
        var eulerAngles = transform.localRotation.eulerAngles;
        transform.localRotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, (float)z);
    }

    private void DropdownOnValueChanged(int index)
    {
        _synchronize.Invoke();
    }

    private void InputFieldOnValueChanged(string text)
    {
        if (DateTime.TryParse(text, out var dateTime))
            _timeModel.SetTime(dateTime);
    }

    public void Dispose()
    {
        _timeSynchronizer.Finish -= OnSynchronizerFinished;
        _dropdown.onValueChanged.RemoveListener(DropdownOnValueChanged);
        _inputField.onValueChanged.RemoveListener(InputFieldOnValueChanged);
    }
}