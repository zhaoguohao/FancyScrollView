﻿using System.Collections.Generic;
using UnityEngine;

namespace FancyScrollView
{
    public abstract class FancyScrollView<TCellData, TContext> : MonoBehaviour where TContext : class, new()
    {
        [SerializeField, Range(float.Epsilon, 1f)] float cellSpacing;
        [SerializeField, Range(0f, 1f)] float scrollOffset;
        [SerializeField] bool loop;
        [SerializeField] Transform cellContainer;

        readonly IList<FancyScrollViewCell<TCellData, TContext>> cells =
            new List<FancyScrollViewCell<TCellData, TContext>>();

        float currentPosition;

        protected abstract GameObject CellPrefab { get; }
        protected IList<TCellData> CellData { get; set; } = new List<TCellData>();
        protected TContext Context { get; } = new TContext();

        /// <summary>
        /// Updates the contents.
        /// </summary>
        /// <param name="cellData">Cell data.</param>
        protected void UpdateContents(IList<TCellData> cellData) 
        {
            CellData = cellData;
            UpdateContents();
        }

        /// <summary>
        /// Updates the contents.
        /// </summary>
        protected void UpdateContents() => UpdatePosition(currentPosition, true);

        /// <summary>
        /// Updates the scroll position.
        /// </summary>
        /// <param name="position">Position.</param>
        protected void UpdatePosition(float position) => UpdatePosition(position, false);

        void UpdatePosition(float position, bool forceUpdateContents)
        {
            currentPosition = position;

            var p = position - scrollOffset / cellSpacing;
            var firstPosition = (Mathf.Ceil(p) - p) * cellSpacing;
            var firstDataIndex = Mathf.CeilToInt(p);

            if (firstPosition + cells.Count * cellSpacing <= 1f)
            {
                FillCells(firstPosition);
            }

            UpdateCells(firstPosition, firstDataIndex, forceUpdateContents);
        }

        void FillCells(float firstPosition)
        {
            for (var (count, p) = (0, firstPosition); p <= 1f; p += cellSpacing, count++)
            {
                if (count < cells.Count)
                {
                    continue;
                }

                var cell = Instantiate(CellPrefab, cellContainer)
                        .GetComponent<FancyScrollViewCell<TCellData, TContext>>();
                cell.SetContext(Context);
                cell.SetVisible(false);

                cells.Add(cell);
            }
        }

        void UpdateCells(float firstPosition, int firstDataIndex, bool forceUpdateContents)
        {
            var count = 0;

            for (var p = firstPosition; p <= 1f; p += cellSpacing, count++)
            {
                var dataIndex = firstDataIndex + count;
                var cell = cells[GetCircularIndex(dataIndex, cells.Count)];

                UpdateCell(cell, dataIndex, forceUpdateContents);

                if (cell.gameObject.activeSelf)
                {
                    cell.UpdatePosition(p);
                }
            }

            while (count < cells.Count)
            {
                cells[GetCircularIndex(firstDataIndex + count, cells.Count)].SetVisible(false);
                count++;
            }
        }

        void UpdateCell(FancyScrollViewCell<TCellData, TContext> cell, int dataIndex, bool forceUpdateContents)
        {
            if (loop)
            {
                dataIndex = GetCircularIndex(dataIndex, CellData.Count);
            }
            else if (dataIndex < 0 || dataIndex > CellData.Count - 1)
            {
                cell.SetVisible(false);
                return;
            }

            if (!forceUpdateContents && cell.DataIndex == dataIndex && cell.IsVisible)
            {
                return;
            }

            cell.DataIndex = dataIndex;
            cell.SetVisible(true);
            cell.UpdateContent(CellData[dataIndex]);
        }

        int GetCircularIndex(int index, int maxSize) =>
            index < 0 ? maxSize - 1 + (index + 1) % maxSize : index % maxSize;

#if UNITY_EDITOR
        bool cachedLoop;
        float cachedCellSpacing, cachedScrollOffset;

        void LateUpdate()
        {
            if ((cachedLoop, cachedScrollOffset, cachedCellSpacing) != (loop, scrollOffset, cellSpacing))
            {
                (cachedLoop, cachedScrollOffset, cachedCellSpacing) = (loop, scrollOffset, cellSpacing);
                UpdatePosition(currentPosition);
            }
        }
#endif
    }

    public sealed class FancyScrollViewNullContext
    {
    }

    public abstract class FancyScrollView<TCellData> : FancyScrollView<TCellData, FancyScrollViewNullContext>
    {
    }
}
