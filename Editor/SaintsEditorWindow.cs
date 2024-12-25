using System.Collections;
using System.Collections.Generic;
using UnityEditor;


namespace SaintsField.Editor
{
    public partial class SaintsEditorWindow: EditorWindow
    {
        #region LifeCircle
        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
            if (_saintsEditorWindowSpecialEditor != null)
            {
                DestroyImmediate(_saintsEditorWindowSpecialEditor);
                _saintsEditorWindowSpecialEditor = null;
            }
            OnEditorDestroy();
        }

        public virtual void OnEditorDestroy()
        {

        }

        private void OnEnable()
        {
            OnEditorEnable();
        }

        public virtual void OnEditorEnable()
        {

        }

        private void OnDisable()
        {
            OnEditorDisable();
        }

        public virtual void OnEditorDisable()
        {

        }

        #endregion

        private void OnPlayModeStateChange(PlayModeStateChange stateChange)
        {
            if (stateChange != PlayModeStateChange.EnteredEditMode &&
                stateChange != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }
#if UNITY_2021_3_OR_NEWER && !SAINTSFIELD_UI_TOOLKIT_DISABLE
            OnPlayModeStateRebindUIToolkit();
#endif
        }

        #region Update

        private readonly HashSet<IEnumerator> _coroutines = new HashSet<IEnumerator>();

        private void OnEditorUpdateInternal()
         {
             HashSet<IEnumerator> toRemove = new HashSet<IEnumerator>();
             // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
             foreach (IEnumerator coroutine in _coroutines)
             {
                 if (!coroutine.MoveNext())
                 {
                     toRemove.Add(coroutine);
                 }
             }

             _coroutines.ExceptWith(toRemove);

             OnEditorUpdate();
         }

        #endregion

        // ReSharper disable once MemberCanBeProtected.Global
        public virtual void OnEditorUpdate()
        {
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public void StartEditorCoroutine(IEnumerator routine)
        {
            _coroutines.Add(routine);
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public void StopEditorCoroutine(IEnumerator routine)
        {
            _coroutines.Remove(routine);
        }
    }
}
