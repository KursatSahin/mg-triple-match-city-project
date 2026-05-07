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
        private const string DeckFrontSortingLayer = "DeckFront";

        private readonly DeckView _deckView;
        private readonly IItemFactory _itemFactory;
        private readonly DeckConfigSO _deckConfig;
        private readonly List<DeckSlotData> _slots;

        public IReadOnlyList<DeckSlotData> Slots => _slots;
        public int SlotCount => _slots.Count;
        public bool IsFull => CountFilled() >= _slots.Count;

        public DeckManager(DeckView deckView, IItemFactory itemFactory, DeckConfigSO deckConfig)
        {
            _deckView = deckView;
            _itemFactory = itemFactory;
            _deckConfig = deckConfig;

            int count = deckView != null ? deckView.SlotCount : 0;

            _slots = new List<DeckSlotData>(count);

            for (int index = 0; index < count; index++)
            {
                _slots.Add(new DeckSlotData());
            }
        }

        public InsertItemData InsertData(CollectibleItemView itemView)
        {
            if (itemView == null) return InsertItemData.Empty;

            if (_deckView == null || _slots.Count == 0)
            {
                Debug.LogError("[DeckManager] Deck view is missing or has no slots.");
                return InsertItemData.Empty;
            }

            if (IsFull)
            {
                Debug.LogWarning("[DeckManager] Deck is full, cannot insert.");
                return InsertItemData.Empty;
            }

            int targetIndex = FindInsertIndex(itemView.ItemData);
            
            if (targetIndex < 0) return InsertItemData.Empty;

            var shiftRightMoves = new List<ShiftItemData>();
            
            int firstEmpty = -1;
            
            for (int index = targetIndex; index < _slots.Count; index++)
            {
                if (_slots[index].IsEmpty)
                {
                    firstEmpty = index; break;
                }
            }
            
            if (firstEmpty >= 0)
            {
                for (int index = firstEmpty; index > targetIndex; index--)
                {
                    var moved = _slots[index - 1].Item;
                    
                    _slots[index].Item = moved;
                    _slots[index - 1].Item = null;
                    
                    if (moved != null)
                    {
                        shiftRightMoves.Add(new ShiftItemData(moved, index));
                    }
                }
            }

            _slots[targetIndex].Item = itemView;

            return new InsertItemData
            {
                IsValid = true,
                ItemView = itemView,
                TargetSlotIndex = targetIndex,
                ShiftRightMoves = shiftRightMoves
            };
        }

        public MatchItemData ResolveMatchData()
        {
            int matchStart = FindMatchIndex();
            if (matchStart < 0) return MatchItemData.Empty;

            var cleared = new List<CollectibleItemView>(MatchTreshold);
            
            for (int index = 0; index < MatchTreshold; index++)
            {
                var slot = _slots[matchStart + index];
                
                if (slot.Item != null)
                {
                    cleared.Add(slot.Item);
                }
                
                slot.Item = null;
            }

            // collapse remaining filled slots to the left
            var shiftLeftMoves = new List<ShiftItemData>();
            int emptyCount = 0;
            
            for (int index = 0; index < _slots.Count; index++)
            {
                if (_slots[index].IsEmpty)
                {
                    emptyCount++; 
                    continue;
                }
                
                if (emptyCount <= 0) continue;

                var moved = _slots[index].Item;
                int targetIndex = index - emptyCount;
                
                _slots[targetIndex].Item = moved;
                _slots[index].Item = null;
                
                if (moved != null)
                {
                    shiftLeftMoves.Add(new ShiftItemData(moved, targetIndex));
                }
            }

            return new MatchItemData
            {
                HasMatch = true,
                Cleared = cleared,
                ShiftLeftMoves = shiftLeftMoves
            };
        }

        public void AnimateInsert(InsertItemData insertItemData)
        {
            if (insertItemData == null || !insertItemData.IsValid) return;

            Transform insertPos = _deckView.GetSlotPos(insertItemData.TargetSlotIndex);
            
            if (insertPos == null)
            {
                Debug.LogError($"[DeckManager] Slot pos at index {insertItemData.TargetSlotIndex} is null.");
                return;
            }

            float moveStepValue = _deckConfig != null ? _deckConfig.MoveStepValue : 12f;
            
            ChangeSortingLayerToDeckFront(insertItemData.ItemView);
            
            Vector3 deckScale = Vector3.one * CalculateTargetScale(insertItemData.ItemView);
            insertItemData.ItemView.StartFlightToSlot(insertPos, deckScale, moveStepValue);

            // Existing shifted items retarget to new pos.
            for (int index = 0; index < insertItemData.ShiftRightMoves.Count; index++)
            {
                Transform anchor = _deckView.GetSlotPos(insertItemData.ShiftRightMoves[index].TargetSlotIndex);
                
                if (anchor == null)
                {
                    Debug.LogError($"[DeckManager] Slot pos at index {insertItemData.ShiftRightMoves[index].TargetSlotIndex} is null.");
                    continue;
                }
                
                CollectibleItemView item = insertItemData.ShiftRightMoves[index].Item;
                item.StartFlightToSlot(anchor, item.transform.localScale, moveStepValue);
            }
        }

        public void AnimateMatch(MatchItemData matchItemData)
        {
            if (matchItemData == null || !matchItemData.HasMatch) return;
            
            CollectibleItemView triggerItem = FindTriggerItem(matchItemData.Cleared);

            if (triggerItem != null)
            {
                triggerItem.SetOnArrivedCallback(() => ClearThenShiftLeft(matchItemData));
            }
            else
            {
                ClearThenShiftLeft(matchItemData);
            }
        }

        private static CollectibleItemView FindTriggerItem(List<CollectibleItemView> cleared)
        {
            for (int index = 0; index < cleared.Count; index++)
            {
                if (cleared[index].CurrentAnimState == CollectibleItemView.AnimState.MovingToSlot)
                    return cleared[index];
            }
            return null;
        }

        private void ClearThenShiftLeft(MatchItemData itemData)
        {
            float clearDuration = _deckConfig != null ? _deckConfig.MatchClearDuration : 0.15f;

            int remaining = itemData.Cleared.Count;
            if (remaining == 0)
            {
                StartShiftLeftMoves(itemData.ShiftLeftMoves);
                return;
            }

            for (int index = 0; index < itemData.Cleared.Count; index++)
            {
                itemData.Cleared[index].StartClear(clearDuration, item => {
                    _itemFactory.Despawn(item);
                    remaining--;
                    
                    if (remaining == 0)
                    {
                        StartShiftLeftMoves(itemData.ShiftLeftMoves);
                    }
                });
            }
        }

        private void StartShiftLeftMoves(List<ShiftItemData> moves)
        {
            float smoothness = _deckConfig != null ? _deckConfig.MoveStepValue : 12f;

            for (int index = 0; index < moves.Count; index++)
            {
                ShiftItemData itemData = moves[index];
                
                if (itemData.Item == null) continue;
                if (itemData.TargetSlotIndex < 0 || itemData.TargetSlotIndex >= _slots.Count) continue;
                
                // The newer command's positioning overwrites
                if (_slots[itemData.TargetSlotIndex].Item != itemData.Item) continue;

                Transform anchor = _deckView.GetSlotPos(itemData.TargetSlotIndex);
                
                if (anchor == null)
                {
                    Debug.LogError($"[DeckManager] Slot anchor at index {itemData.TargetSlotIndex} is null.");
                    continue;
                }
                
                itemData.Item.StartFlightToSlot(anchor, itemData.Item.transform.localScale, smoothness);
            }
        }

        private static void ChangeSortingLayerToDeckFront(CollectibleItemView itemView)
        {
            var itemViewSpriteRenderer = itemView.SpriteRenderer;
            
            if (itemViewSpriteRenderer == null) return;
            
            itemViewSpriteRenderer.sortingLayerName = DeckFrontSortingLayer;
        }

        private float CalculateTargetScale(CollectibleItemView itemView)
        {
            var itemViewSpriteRenderer = itemView.SpriteRenderer;
            if (itemViewSpriteRenderer == null || itemViewSpriteRenderer.sprite == null) return 1f;

            Vector2 size = itemViewSpriteRenderer.sprite.bounds.size;
            float maxDim = Mathf.Max(size.x, size.y);
            
            if (maxDim <= 0f) return 1f;

            float slotSize = _deckConfig != null ? _deckConfig.SlotSize : 1f;
            
            return slotSize / maxDim;
        }

        private int FindMatchIndex()
        {
            int upperBound = _slots.Count - 2;

            for (int index = 0; index < upperBound; index++)
            {
                if (_slots[index].IsEmpty) break;

                CollectibleItemData type = _slots[index].Type;
                if (_slots[index + 1].Type == type && _slots[index + 2].Type == type)
                    return index;
            }

            return -1;
        }

        // Snap-only match clear (Step 2B fallback). Replaced by AnimateClearMatch.
        /*
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
        */

        // Snap-only compact (Step 2B fallback). Replaced by AnimateCompactLeft.
        /*
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
                if (emptySlotCount > 0)
                {
                    var moved = _slots[index].Item;
                    _slots[index - emptySlotCount].Item = moved;
                    _slots[index].Item = null;
                    if (moved != null) MoveToSlot(moved, index - emptySlotCount);
                }
            }
        }
        */

        /// <summary>
        /// Finds first available -empty- index (left-most)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private int FindInsertIndex(CollectibleItemData type)
        {
            int lastSameType = -1;
            
            for (int index = 0; index < _slots.Count; index++)
            {
                if (!_slots[index].IsEmpty && _slots[index].Type == type)
                {
                    lastSameType = index;
                }
            }

            if (lastSameType >= 0) return lastSameType + 1;

            for (int index = 0; index < _slots.Count; index++)
            {
                if (_slots[index].IsEmpty) return index;
            }

            return -1;
        }

        // Snap-only shift right (Step 2A/2B fallback). Replaced by PrepareShiftRight + tween.
        /*
        private void ShiftRight(int targetIndex)
        {
            int lastIndex = _slots.Count - 1;

            int firstEmpty = -1;
            for (int i = targetIndex; i <= lastIndex; i++)
            {
                if (_slots[i].IsEmpty) { firstEmpty = i; break; }
            }
            if (firstEmpty < 0) return;

            for (int i = firstEmpty; i > targetIndex; i--)
            {
                var moved = _slots[i - 1].Item;
                _slots[i].Item = moved;
                _slots[i - 1].Item = null;
                if (moved != null) MoveToSlot(moved, i);
            }
        }
        */

        [Obsolete]
        private void MoveToSlot(CollectibleItemView item, int slotIndex)
        {
            Transform anchor = _deckView.GetSlotPos(slotIndex);

            if (anchor == null)
            {
                Debug.LogError($"[DeckManager] Slot anchor at index {slotIndex} is null.");
                return;
            }
            
            var itemTransform = item.transform;

            itemTransform.SetParent(anchor, worldPositionStays: false);

            itemTransform.localPosition = Vector3.zero;
            itemTransform.localRotation = Quaternion.identity;
        }

        private int CountFilled()
        {
            int filled = 0;
            
            for (int index = 0; index < _slots.Count; index++)
            {
                if (!_slots[index].IsEmpty)
                {
                    filled++;
                }
            }
            
            return filled;
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
