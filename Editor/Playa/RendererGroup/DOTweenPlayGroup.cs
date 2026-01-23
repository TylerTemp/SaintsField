using System.Linq;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Renderer.EmptyFakeRenderer;
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEngine;
#endif

// ReSharper disable once EmptyNamespace
namespace SaintsField.Editor.Playa.RendererGroup
{
#if DOTWEEN && !SAINTSFIELD_DOTWEEN_DISABLED
    // ReSharper disable once InconsistentNaming
    public partial class DOTweenPlayGroup: ISaintsRendererGroup
    {
        public bool InDirectHorizontalLayout { get; set; }
        public bool InAnyHorizontalLayout { get; set; }
        // public bool NoLabel { get; set; }

        private readonly List<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)> _doTweenMethods = new List<(MethodInfo methodInfo, DOTweenPlayAttribute attribute)>();
        private readonly object _target;

        // ReSharper disable once InconsistentNaming
        private class DOTweenState
        {
            public bool AutoPlay;
            public Tween Tween;
            public ETweenStop Stop;
        }

        // ReSharper disable once InconsistentNaming
        private DOTweenState[] _imGuiDOTweenStates;

        private readonly Texture2D _playIcon;
        private readonly Texture2D _pauseIcon;
        private readonly Texture2D _resumeIcon;
        private readonly Texture2D _stopIcon;

        public DOTweenPlayGroup(object target)
        {
            _target = target;
            _playIcon = Util.LoadResource<Texture2D>("play.png");
            _pauseIcon = Util.LoadResource<Texture2D>("pause.png");
            _resumeIcon = Util.LoadResource<Texture2D>("resume.png");
            _stopIcon = Util.LoadResource<Texture2D>("stop.png");
        }

        ~DOTweenPlayGroup()
        {
            foreach (DOTweenState imGuiDoTweenState in _imGuiDOTweenStates)
            {
                StopTween(imGuiDoTweenState);
            }

            UnityEngine.Object.DestroyImmediate(_playIcon);
            UnityEngine.Object.DestroyImmediate(_pauseIcon);
            UnityEngine.Object.DestroyImmediate(_resumeIcon);
            UnityEngine.Object.DestroyImmediate(_stopIcon);
        }

        private static void StopTween(DOTweenState doTweenState)
        {
            if (doTweenState.Tween == null)
            {
                return;
            }

            switch (doTweenState.Stop)
            {
                case ETweenStop.None:
                    doTweenState.Tween.Kill();
                    break;
                case ETweenStop.Complete:
                    doTweenState.Tween.Complete();
                    break;
                case ETweenStop.Rewind:
                    doTweenState.Tween.Rewind();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doTweenState.Stop), doTweenState.Stop, null);
            }

            doTweenState.Tween = null;
        }

        public void Add(string groupPath, ISaintsRenderer renderer)
        {
            if (renderer is EmptyRenderer)
            {
                return;
            }

            DOTweenPlayRenderer methodRenderer = renderer as DOTweenPlayRenderer;
            Debug.Assert(methodRenderer != null, $"You can NOT nest {renderer} in {this}");

            DOTweenPlayAttribute doTweenPlayAttribute = methodRenderer.FieldWithInfo.PlayaAttributes.OfType<DOTweenPlayAttribute>().FirstOrDefault();
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (doTweenPlayAttribute == null)
            {
                doTweenPlayAttribute = new DOTweenPlayAttribute();
            }
            // if (doTweenPlayAttribute != null)
            // {
            //     _doTweenMethods.Add((methodRenderer.FieldWithInfo.MethodInfo, doTweenPlayAttribute));
            // }
            _doTweenMethods.Add((methodRenderer.FieldWithInfo.MethodInfo, doTweenPlayAttribute));
        }
    }
#endif
}
