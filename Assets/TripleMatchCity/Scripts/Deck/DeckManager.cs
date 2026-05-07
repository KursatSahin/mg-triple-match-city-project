using System.Collections.Generic;
using TripleMatch.Board;
using TripleMatch.Core;
using TripleMatch.Data;
using UnityEngine;

namespace TripleMatch.Deck
{
    /// <summary>
    /// Manages the deck slots, item placement and merge logic
    /// </summary>
    
    // TODO : Rules ...
    public class DeckManager : IDeckManager
    {
        private const int MatchTreshold = 3;
        private const string DeckFrontSortingLayer = "DeckFront";

        private readonly DeckView _deckView;
        private readonly IItemFactory _itemFactory;
        private readonly DeckConfigSO _deckConfig;
        private readonly IEventBus _eventBus;
        private readonly List<DeckSlotData> _slots;

        public IReadOnlyList<DeckSlotData> Slots => _slots;
        public int SlotCount => _slots.Count;
        public bool IsFull => CountFilled() >= _slots.Count;

        public DeckManager(DeckView deckView, IItemFactory itemFactory, DeckConfigSO deckConfig, IEventBus eventBus)
        {
            _deckView = deckView;
            _itemFactory = itemFactory;
            _deckConfig = deckConfig;
            _eventBus = eventBus;

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

            // Notify gameplay listeners that a matched items were removed.
            if (cleared.Count > 0 && cleared[0].ItemData != null)
            {
                _eventBus.Raise(new MatchCompletedEvent
                {
                    ItemData = cleared[0].ItemData,
                    Count = cleared.Count
                });
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
                
                // The newer command's positoning overwrites
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

        /// <summary>
        /// Finds first available empty index (left-most)
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
        
    }
}
