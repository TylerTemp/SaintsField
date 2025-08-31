using System.Collections.Generic;
using UnityEngine;

namespace SaintsField.ComponentHeader
{
    public readonly struct HeaderArea
    {
        /// <summary>
        /// Rect.y for drwaing
        /// </summary>
        public readonly float Y;
        /// <summary>
        /// Rect.height for drawing
        /// </summary>
        public readonly float Height;
        /// <summary>
        /// the x value where the title (component name) started
        /// </summary>
        public readonly float TitleStartX;
        /// <summary>
        /// the x value where the title (component name) ended
        /// </summary>
        public readonly float TitleEndX;
        /// <summary>
        /// the x value where the empty space start. You may want to start draw here
        /// </summary>
        public readonly float SpaceStartX;
        /// <summary>
        /// the x value where the empty space ends. When drawing from right, you need to backward drawing starts here
        /// </summary>
        public readonly float SpaceEndX;

        /// <summary>
        /// The x drawing position. It's recommend to use this as your start drawing point, as SaintsField will
        /// change this value accordingly everytime an item is drawn.
        /// </summary>
        public readonly float GroupStartX;
        /// <summary>
        /// When using `GroupBy`, you can see the vertical rect which already used by others in this group
        /// </summary>
        public readonly IReadOnlyList<Rect> GroupUsedRect;

        public float TitleWidth => TitleEndX - TitleStartX;
        public float SpaceWidth => SpaceEndX - SpaceStartX;

        /// <summary>
        /// A quick way to make a rect
        /// </summary>
        /// <param name="x">where to start</param>
        /// <param name="width">width of the rect</param>
        /// <returns>rect space you want to draw</returns>
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
