using System;
using UnityEngine;

namespace SaintsField
{
    [Serializable]
    public class ShaderParam
    {
        [SerializeField] public string name;

        private bool _cached;
        private int _cachedId;

        public int Id
        {
            get
            {
                if (_cached)
                {
                    return _cachedId;
                }

                _cached = true;
                return _cachedId = Shader.PropertyToID(name);
            }
        }

        public static implicit operator int(ShaderParam sp) => sp.Id;
    }
}
