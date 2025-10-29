using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace GameOption
{
    public class InputKeySettingScrollController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        [SerializeField] private KeySettingScroller scroller;
        //[SerializeField] private 

        public void Initialize()
        {
            scroller.Delegate = this;
            Todo
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            throw new System.NotImplementedException();
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            throw new System.NotImplementedException();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            throw new System.NotImplementedException();
        }
    }
}