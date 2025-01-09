using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SaintsField.Editor.Core;
using SaintsField.Editor.Playa.Renderer;
using SaintsField.Editor.Playa.Utils;
using SaintsField.Editor.Utils;
using SaintsField.Playa;
using UnityEditor;
using UnityEngine;

namespace SaintsField.Editor.Playa.RendererGroup
{
    public partial class SaintsRendererGroup
    {
        private static int _iMGUINeedIndentFixCounter;
        public static bool IMGUINeedIndentFix => _iMGUINeedIndentFixCounter > 0;

        public class SaintsRendererGroupIMGUINeedIndentFixScoop : IDisposable
        {
            private readonly bool _needFix;

            public SaintsRendererGroupIMGUINeedIndentFixScoop(bool need)
            {
                if(need)
                {
                    _iMGUINeedIndentFixCounter++;
                }

                _needFix = need;
            }

            public void Dispose()
            {
                if(_needFix)
                {
                    _iMGUINeedIndentFixCounter--;
                }
            }
        }

        // private static Texture2D _outlineBg;
        private static GUIStyle _fancyBoxLeftIconButtonStyle;

        private static GUIStyle GetFancyBoxLeftIconButtonStyle()
        {
            if (_fancyBoxLeftIconButtonStyle != null)
            {
                return _fancyBoxLeftIconButtonStyle;
            }

            _fancyBoxLeftIconButtonStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(15, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 13,
            };

            return _fancyBoxLeftIconButtonStyle;
        }

        // TODO: dispose it, but... does it really matters
        private static Texture2D _dropdownIcon;
        private static Texture2D _dropdownRightIcon;

        private static (Texture2D dropdownIcon, Texture2D dropdownRightIcon) GetDropdownIcons()
        {
            if (_dropdownIcon != null)
            {
                return (_dropdownIcon, _dropdownRightIcon);
            }

            _dropdownIcon = Util.LoadResource<Texture2D>("classic-dropdown-gray.png");
            _dropdownRightIcon = Util.LoadResource<Texture2D>("classic-dropdown-right-gray.png");

            return (_dropdownIcon, _dropdownRightIcon);
        }


        private IEnumerable<ISaintsRenderer> GetRenderer()
        {
            return _eLayout.HasFlag(ELayout.Tab)
                ? _groupIdToRenderer[_orderedKeys[_curSelected]]
                : _renderers.Select(each => each.renderer);
        }



    }
}
