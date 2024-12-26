// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// namespace SaintsField
// {
//     public class SaintsEditor: ScriptableObject
//     {
//         private readonly HashSet<IEnumerator> _coroutines = new HashSet<IEnumerator>();
//
//         public virtual SaintsEditor CreateTarget()
//         {
//             return this;
//         }
//
//         public void OnEditorUpdateInternal()
//         {
//             HashSet<IEnumerator> toRemove = new HashSet<IEnumerator>();
//             // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
//             foreach (IEnumerator coroutine in _coroutines)
//             {
//                 if (!coroutine.MoveNext())
//                 {
//                     toRemove.Add(coroutine);
//                 }
//             }
//
//             _coroutines.ExceptWith(toRemove);
//
//             OnEditorUpdate();
//         }
//
//         protected virtual void OnEditorUpdate()
//         {
//
//         }
//
//         protected void StartEditorCoroutine(IEnumerator routine)
//         {
//             _coroutines.Add(routine);
//         }
//
//         protected void StopEditorCoroutine(IEnumerator routine)
//         {
//             _coroutines.Remove(routine);
//         }
//     }
// }
