using HarmonyLib;
using SRF;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AdvanceGMBlocks.GMBlockData;

namespace AdvanceGMBlocksPlugin.SystemMessageExtension
{
    public static class SystemMessageCallbackExtension
    {
        private static SystemMessageCallbackField _callbackField;

        public static void AddCallbackInput(SystemMessage instance, string title, string description, CallbackData callbackData, Action<CallbackData> onSubmit, Action onAcceptAction = null, Action onCancelAction = null)
        {
            Debug.Log($"_callbackField is null = {_callbackField == null}");
            if (_callbackField == null) {
                Debug.Log($"Fetching original folder field and stop All Input Object");
                SystemMessageFolderField _folderField = new Traverse(instance).Field("_folderField").GetValue<SystemMessageFolderField>();
                GameObject _stopAllInputObject = new Traverse(instance).Field("_stopAllInputObject").GetValue<GameObject>();
                Debug.Log($"cloning field");
                GameObject clone = GameObject.Instantiate(_folderField.gameObject,_stopAllInputObject.transform);
                clone.name = "CallbackPanel";
                Debug.Log($"removing original component");
                clone.RemoveComponentIfExists<SystemMessageFolderField>();
                Debug.Log($"Fetching original folder field");
                _callbackField = clone.AddComponent<SystemMessageCallbackField>();
                Debug.Log($"Adjust Rect transform size");
                var rectTransform = clone.GetComponent<RectTransform>();
                _callbackField.collapsedHeight = rectTransform.sizeDelta.y;
                _callbackField.expandedHeight = rectTransform.sizeDelta.y + 40;

                Debug.Log($"Initialize");
                _callbackField.Initialize();
            }

            // Open Instance
            Debug.Log($"Open");
            instance.GetType().GetMethod("Open", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(instance, new object[0]);
            Debug.Log($"Setup Message");
            _callbackField.SetupMessage(callbackData, onSubmit, onAcceptAction, onCancelAction);
        }
    }


    public class SystemMessageCallbackField : SystemMessagePanel
    {
        internal Bounce.Localization.UiText Title;
        internal Bounce.Localization.UiText Message;

        internal TMP_InputField _Endpoint;
        internal Bounce.Localization.UiDropdown _CallbackType;
        internal TMP_InputField _Payload;

        internal Action<CallbackData> _onSubmit;

        internal float collapsedHeight;
        internal float expandedHeight;

        IEnumerator coroutine;

        private void Expand()
        {
            if (!base.gameObject.activeInHierarchy) return;

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = LerpPosition(true);
            StartCoroutine(coroutine);
        }
        private void Collapse()
        {
            if (!base.gameObject.activeInHierarchy) return;

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = LerpPosition(false);
            StartCoroutine(coroutine);
        }

        IEnumerator LerpPosition(bool expand)
        {
            float targetPosition = expand ? expandedHeight : collapsedHeight;
            float fromPosition = expand ? collapsedHeight : expandedHeight;

            float lerpDuration = 0.2f;

            RectTransform rectTransform = this.GetComponent<RectTransform>();
            float startValue = rectTransform.sizeDelta.y;

            if (startValue != targetPosition)
            {
                float ratio = Mathf.InverseLerp(collapsedHeight, expandedHeight, startValue);
                if (!expand) ratio = 1 - ratio;

                float timeElapsed = ratio * lerpDuration;

                while (timeElapsed < lerpDuration)
                {
                    float valueToLerp = Mathf.Lerp(fromPosition, targetPosition, timeElapsed / lerpDuration);
                    rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, valueToLerp);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, targetPosition);
            }
        }

        public void Initialize()
        {
            new Traverse(this).Field("_systemMessage").SetValue(SystemMessage.Instance);

            Title = this.transform.GetChild(0).GetChild(0).GetComponent<Bounce.Localization.UiText>();

            var panel = this.transform.GetChild(1);
            var buttonPanel = this.transform.GetChild(2);

            Message = panel.GetChild(0).GetComponent<Bounce.Localization.UiText>();
            _Endpoint = panel.GetChild(1).GetComponent<TMP_InputField>();
            _CallbackType = panel.GetChild(2).GetComponent<Bounce.Localization.UiDropdown>();
            _Payload = GameObject.Instantiate(_Endpoint.gameObject, panel).GetComponent<TMP_InputField>();
            _Payload.transform.position += new Vector3(0,-75,0);

            Message.text = "This is for 3rd party integration/callback";
            Title.text = "Set Callback Process";

            buttonPanel.GetChild(0).GetComponent<Button>().onClick.AddListener(OnAccept);
            buttonPanel.GetChild(1).GetComponent<Button>().onClick.AddListener(OnCancel);
        }

        public void CallbackTypeChanged(int v)
        {
            if (v == 2 || v == 3)
            {
                Expand();
            }
            else
            {
                Collapse();
            }
        }

        public void SetupMessage(CallbackData callbackData, Action<CallbackData> onSubmit, Action onAcceptAction = null, Action onCancelAction = null)
        {
            Debug.Log($"SetActive");
            base.gameObject.SetActive(value: true);
            base.transform.SetAsLastSibling();

            Debug.Log($"AddEventActions");
            onAccept = onAcceptAction;
            onCancel = onCancelAction;
            _onSubmit = onSubmit;

            Debug.Log($"Load string Data");
            _Endpoint.text = callbackData.Endpoint;
            _Payload.text = callbackData.Payload;

            Debug.Log($"Setup Drown down list");
            SetupStandardDropdownList(callbackData);

            Debug.Log($"Resize");
            var rectTransform = GetComponent<RectTransform>();

            if (callbackData.MethodType == CallbackType.Put || callbackData.MethodType == CallbackType.Post) {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, expandedHeight);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, collapsedHeight);
            }

            _Endpoint.Select();
            _Endpoint.ActivateInputField();
        }

        public override void OnAccept()
        {
            onAccept?.Invoke();
            _onSubmit?.Invoke(new CallbackData
            {
                Endpoint = _Endpoint.text,
                Payload = _Payload.text,
                MethodType = (CallbackType)_CallbackType.value
            });
            CloseCallback();
        }

        public override void OnCancel()
        {
            CloseCallback();
        }

        private void CloseCallback()
        {
            _Endpoint.text = string.Empty;
            _Payload.text = string.Empty;
            Close();
        }

        private void SetupStandardDropdownList(CallbackData initialData)
        {
            _CallbackType.onValueChanged.RemoveAllListeners();
            // Set DropDown Options and initial Value
            List<TMP_Dropdown.OptionData> startingOptions = new List<TMP_Dropdown.OptionData>
            {
                new TMP_Dropdown.OptionData("Desktop App"),
                new TMP_Dropdown.OptionData("HTTP GET"),
                new TMP_Dropdown.OptionData("HTTP POST"),
                new TMP_Dropdown.OptionData("HTTP PUT"),
                new TMP_Dropdown.OptionData("HTTP DELETE"),
            };
            _CallbackType.ClearOptions();
            _CallbackType.AddOptions(startingOptions);
            _CallbackType.onValueChanged.AddListener(CallbackTypeChanged);
            _CallbackType.value = (int)initialData.MethodType;
        }
    }
}
