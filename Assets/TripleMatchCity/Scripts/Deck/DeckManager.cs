using System;
using System.Collections.Generic;
using TripleMatch.Board;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Deck
{
    /// <summary>
    /// Manages the deck slots, item placement and merge logic
    /// Rules:
    /// - ...
    /// - ...
    /// - ...
    /// - ...
    /// </summary>
    public class DeckManager : IDeckManager
    {
        private const int MatchTreshold = 3;

        private readonly DeckView _view;
        private readonly IItemFactory _itemFactory;
        private readonly List<DeckSlotData> _slots;

        public IReadOnlyList<DeckSlotData> Slots => _slots;
        public int SlotCount => _slots.Count;
        public bool IsFull => CountFilled() >= _slots.Count;

        public DeckManager(DeckView view, IItemFactory itemFactory)
        {
            _view = view;
            _itemFactory = itemFactory;
            
            int count = view != null ? view.SlotCount : 0;
            
            _slots = new List<DeckSlotData>(count);
            
            for (int i = 0; i < count; i++)
            {
                _slots.Add(new DeckSlotData());
            }
        }

        public bool TryInsert(CollectibleItemView item)
        {
            if (item == null) return false;
            
            if (_view == null || _slots.Count == 0)
            {
                Debug.LogError("[DeckManager] Deck view is missing or has no slots.");
                return false;
            }
            
            if (IsFull)
            {
                Debug.LogWarning("[DeckManager] Deck is full, cannot insert.");
                return false;
            }

            CollectibleItemData type = item.ItemData;
            
            int targetIndex = FindInsertIndex(type);
            
            if (targetIndex < 0) return false;

            ShiftRight(targetIndex);
            
            PlaceInSlot(item, targetIndex);

            FindMatches();
            
            return true;
        }

        private void FindMatches()
        {
            int matchStartingIndex = FindMatchIndex();
            if (matchStartingIndex < 0) return;

            ClearMatch(matchStartingIndex);
            
            //ShiftLeft();
            OptimizedShiftLeft();
        }

        private int FindMatchIndex()
        {
            int upperBound = _slots.Count - 2;

            for (int i = 0; i < upperBound; i++)
            {
                if (_slots[i].IsEmpty) break;

                CollectibleItemData type = _slots[i].Type;
                if (_slots[i + 1].Type == type && _slots[i + 2].Type == type)
                    return i;
            }

            return -1;
        }

        private void ClearMatch(int startIndex)
        {
            for (int i = 0; i < MatchTreshold; i++)
            {
                var slot = _slots[startIndex + i];
                var view = slot.Item;
                slot.Item = null;

                if (view != null)
                    _itemFactory.Despawn(view);
            }
        }

        private void OptimizedShiftLeft()
        {
            int emptySlotCount = 0;

            for (int index = 0; index < _slots.Count; index++)
            {
                if (_slots[index].IsEmpty)
                {
                    emptySlotCount++;
                    continue;
                }
                else
                {
                    if (emptySlotCount > 0)
                    {
                        var moved = _slots[index].Item;
                        
                        _slots[index - emptySlotCount].Item = moved;
                        _slots[index].Item = null;
                        
                        if (moved != null)
                        {
                            MoveToSlot(moved, index - emptySlotCount);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Finds first available -empty- index (left-most)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int FindInsertIndex(CollectibleItemData type)
        {
            int lastSameType = -1;
            
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsEmpty && _slots[i].Type == type)
                {
                    lastSameType = i;
                }
            }

            if (lastSameType >= 0) return lastSameType + 1;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].IsEmpty) return i;
            }

            return -1;
        }

        /// <summary>
        /// Shifts items starting from targetIndex one slot to the right, stopping at the first empty slot.
        /// </summary>
        /// <param name="targetIndex"></param>
        private void ShiftRight(int targetIndex)
        {
            int lastIndex = _slots.Count - 1;

            // Find the first empty slot at or after targetIndex
            int firstEmpty = -1;
            
            for (int i = targetIndex; i <= lastIndex; i++)
            {
                if (_slots[i].IsEmpty)
                {
                    firstEmpty = i;
                    break;
                }
            }
            if (firstEmpty < 0) return; // nothing to shift

            for (int i = firstEmpty; i > targetIndex; i--)
            {
                var moved = _slots[i - 1].Item;
                
                _slots[i].Item = moved;
                _slots[i - 1].Item = null;

                if (moved != null)
                {
                    MoveToSlot(moved, i);
                }
            }
        }

        private void PlaceInSlot(CollectibleItemView item, int slotIndex)
        {
            _slots[slotIndex].Item = item;
            MoveToSlot(item, slotIndex);
        }

        private void MoveToSlot(CollectibleItemView item, int slotIndex)
        {
            Transform anchor = _view.GetSlotAnchor(slotIndex);
            
            if (anchor == null)
            {
                Debug.LogError($"[DeckManager] Slot anchor at index {slotIndex} is null.");
                return;
            }

            var t = item.transform;
            
            t.SetParent(anchor, worldPositionStays: false);
            
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        private int CountFilled()
        {
            int n = 0;
            
            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    n++;
                }
            }
            
            return n;
        }
        
        /*[Obsolete]
        private void ShiftLeft()
        {
            int write = 0;

            for (int read = 0; read < _slots.Count; read++)
            {
                if (_slots[read].IsEmpty) continue;

                if (write != read)
                {
                    var moved = _slots[read].Item;

                    _slots[write].Item = moved;
                    _slots[read].Item = null;

                    if (moved != null)
                    {
                        SnapToSlot(moved, write);
                    }
                }

                write++;
            }
        }*/
        
        /*
        [Obsolete]
        private bool ClearMatchedItems()
        {
            bool anyCleared = false;
            int i = 0;

            while (i < _slots.Count)
            {
                var slot = _slots[i];

                if (slot.IsEmpty)
                {
                    i++;
                    continue;
                }

                int runEnd = i + 1;

                while (runEnd < _slots.Count
                       && !_slots[runEnd].IsEmpty
                       && _slots[runEnd].Type == slot.Type)
                {
                    runEnd++;
                }

                int runLength = runEnd - i;

                if (runLength >= MatchRunLength)
                {
                    for (int k = 0; k < MatchRunLength; k++)
                    {
                        var view = _slots[i + k].Item;

                        _slots[i + k].Item = null;

                        if (view != null)
                        {
                            _itemFactory.Despawn(view);
                        }
                    }

                    anyCleared = true;
                    i = i + MatchRunLength;
                }
                else
                {
                    i = runEnd;
                }
            }
            return anyCleared;
        }
        */
    }
}
