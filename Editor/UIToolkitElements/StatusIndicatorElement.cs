using System.Collections.Generic;
using SaintsField.Editor.Utils;
using UnityEngine;
using UnityEngine.UIElements;

namespace SaintsField.Editor.UIToolkitElements
{
    public class StatusIndicatorElement: VisualElement
    {
        private static VisualTreeAsset _treeRowTemplate;
        private readonly VisualElement _loading;
        private readonly VisualElement _ok;
        private readonly VisualElement _error;
        private readonly VisualElement _progressBar;

        public StatusIndicatorElement()
        {
            _treeRowTemplate = Util.LoadResource<VisualTreeAsset>("UIToolkit/StatusIndicator/StatusIndicator.uxml");
            TemplateContainer root = _treeRowTemplate.CloneTree();
            _loading = root.Q<VisualElement>("Loading");
            _ok = root.Q<VisualElement>("OK");
            _error = root.Q<VisualElement>("Error");
            _progressBar = root.Q<VisualElement>("ProgressBar");

            _pendingTasks[_loading] = new List<IVisualElementScheduledItem>();
            _pendingTasks[_ok] = new List<IVisualElementScheduledItem>();
            _pendingTasks[_error] = new List<IVisualElementScheduledItem>();
            _displayStatus[_loading] = false;
            _displayStatus[_ok] = false;
            _displayStatus[_error] = false;

            hierarchy.Add(root);

            UIToolkitUtils.HelpKeepRotate(_loading);

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                schedule.Execute(() => UIToolkitUtils.TriggerRotate(_loading)).StartingIn(200);
            });
        }

        // private bool _isLoading;

        public void EnsureLoading(bool needLoading, float progress)
        {
            if (_displayStatus[_loading] == needLoading)
            {
                if (needLoading && progress >= 0)
                {
                    _progressBar.style.width = Length.Percent(progress * 100);
                }
                else
                {
                    _progressBar.style.width = 0;
                }

                return;
            }

            _displayStatus[_loading] = needLoading;
            // Debug.Log($"set to {needLoading}");
            if(needLoading)
            {
                EnsureHide(_ok);
                EnsureHide(_error);
                PlayShow(_loading);
                if (progress >= 0)
                {
                    _progressBar.style.width = Length.Percent(progress * 100);
                }
            }
            else
            {
                EnsureHide(_loading);
                _progressBar.style.width = 0;
            }
        }

        public void PlayLoading()
        {
            bool curDisplaying = _displayStatus[_loading];

            EnsureHide(_ok);
            EnsureHide(_error);

            if(curDisplaying)
            {
                // Debug.Log("play loading hide current");
                EnsureHide(_loading);
                _pendingTasks[_loading].Add(schedule.Execute(() => PlayShow(_loading)).StartingIn(100));
            }
            else
            {
                // Debug.Log("play loading now");
                PlayShow(_loading);
            }

            _displayStatus[_loading] = true;
        }

        public void PlayOk()
        {
            EnsureHide(_loading);
            EnsureHide(_error);
            if (_displayStatus[_ok])
            {
                EnsureHide(_ok);
                _pendingTasks[_ok].Add(schedule.Execute(() =>
                {
                    PlayShow(_ok);
                    TimeoutHide(_ok);
                }).StartingIn(100));
            }
            else
            {
                PlayShow(_ok);
                TimeoutHide(_ok);
            }
            _displayStatus[_ok] = true;
        }

        public void PlayError()
        {
            EnsureHide(_loading);
            EnsureHide(_ok);
            if (_displayStatus[_error])
            {
                EnsureHide(_error);
                _pendingTasks[_error].Add(schedule.Execute(() =>
                {
                    PlayShow(_error);
                    TimeoutHide(_error);
                }).StartingIn(100));
            }
            else
            {
                PlayShow(_error);
                TimeoutHide(_error);
            }
            _displayStatus[_error] = true;
        }

        // public void DoPlay()
        // {
        //     Play(_loading);
        // }

        // private readonly List<IVisualElementScheduledItem> _pendingTasks = new List<IVisualElementScheduledItem>();
        private readonly Dictionary<VisualElement, List<IVisualElementScheduledItem>> _pendingTasks =
            new Dictionary<VisualElement, List<IVisualElementScheduledItem>>();
        private readonly Dictionary<VisualElement, bool> _displayStatus =
            new Dictionary<VisualElement, bool>();

        // private void CleanTasks()
        // {
        //     foreach (IVisualElementScheduledItem pt in _pendingTasks)
        //     {
        //         pt.Pause();
        //     }
        //     _pendingTasks.Clear();
        // }


        private void CleanTasks(VisualElement el)
        {
            List<IVisualElementScheduledItem> pending = _pendingTasks[el];
            foreach (IVisualElementScheduledItem pt in pending)
            {
                pt.Pause();
            }
            pending.Clear();
        }

        private void PlayShow(VisualElement el)
        {
            _displayStatus[el] = true;
            CleanTasks(el);

            el.RemoveFromClassList("show");

            IVisualElementScheduledItem showTask = el.schedule.Execute(() =>
                {
                    el.AddToClassList("show");
                });
            showTask.ExecuteLater(1);
            _pendingTasks[el].Add(showTask);
        }

        private void TimeoutHide(VisualElement el)
        {
            IVisualElementScheduledItem hideTask = el.schedule.Execute(() =>
            {
                _displayStatus[el] = false;
                // Debug.Log("Hide");
                el.RemoveFromClassList("show");
                // el.AddToClassList("hide");
            });
            hideTask.ExecuteLater(2000);
            _pendingTasks[el].Add(hideTask);
        }

        public void EnsureHide(VisualElement el)
        {
            _displayStatus[el] = false;
            CleanTasks(el);
            el.RemoveFromClassList("show");
        }
    }
}
