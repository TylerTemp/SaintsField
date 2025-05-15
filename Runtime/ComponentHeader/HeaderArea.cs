using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.ComponentHeader
{
    public readonly struct HeaderArea
    {
        public readonly float Y;
        public readonly float Height;
        public readonly float TitleStartX;
        public readonly float TitleEndX;
        public readonly float SpaceStartX;
        public readonly float SpaceEndX;

        public readonly float GroupStartX;
        public readonly IReadOnlyList<Rect> GroupUsedRect;

        public float TitleWidth => TitleEndX - TitleStartX;
        public float SpaceWidth => SpaceEndX - SpaceStartX;

        public Rect MakeXWidthRect(float x, float width) => new Rect(x, Y, width, Height);

        public HeaderArea EditorWrap(float groupStart, IReadOnlyList<Rect> groupUsedRect) => new HeaderArea(
                Y,
                Height,
                TitleStartX,
                TitleEndX,
                SpaceStartX,
                SpaceEndX,

                groupStart,
                groupUsedRect
            );

        public HeaderArea(float y, float height, float titleStartX, float titleEndX, float spaceStartX, float spaceEndX, float groupStartX, IReadOnlyList<Rect> groupUsedRect)
        {
            Y = y;
            Height = height;
            TitleStartX = titleStartX;
            TitleEndX = titleEndX;
            SpaceStartX = spaceStartX;
            SpaceEndX = spaceEndX;
            GroupStartX = groupStartX;
            GroupUsedRect = groupUsedRect;
        }
    }
}
