using UnityEngine;

namespace SaintsField.Interfaces
{
    public interface ISeparatorAttribute
    {
        string Title { get; }
        Color Color { get; }
        EAlign EAlign { get; }
        bool IsCallback { get; }
        int Space { get; }
        bool Below { get; }
    }
}
