using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using System;
public interface IHeapItem<T> where T : struct
{
    float GetHeapValue();
    int HeapIndex { get; set; }
}
[System.Serializable]
public struct NativeMinHeap<T> : IDisposable where T : unmanaged, IHeapItem<T>
{
    public NativeList<T> items;
    public int Count => items.Length;
    public bool IsCreated => items.IsCreated;
    public NativeMinHeap(int initialCapacity, Allocator allocator)
    {
        items = new NativeList<T>(initialCapacity, allocator);
    }
    public void Dispose()
    {
        if (items.IsCreated)
        {
            items.Dispose();
            items = default;
        }
    }
    public void Add(T item)
    {
        HeapOperations.Add(ref items, item);
    }
    public void UpdateItem(T item)
    {
        // HeapIndex를 통해 해당 아이템의 현재 위치를 찾습니다.
        // 그리고 F 값의 변경에 따라 HeapifyUp 또는 HeapifyDown을 호출합니다.
        // G값이 낮아지면 F값도 낮아지므로 주로 HeapifyUp이 필요합니다.
        // 하지만 만약을 위해 둘 다 호출하는 것이 안전합니다.
        // (하나만 필요하다면 if (item.GetHeapValue() < items[item.HeapIndex].GetHeapValue()) HeapifyUp; else HeapifyDown;)
        HeapOperations.HeapifyUp(ref items, item.HeapIndex);
        HeapOperations.HeapifyDown(ref items, item.HeapIndex);
    }
    public T RemoveMin()
    {
        if (items.Length == 0)
        {
            throw new InvalidOperationException("Heap is empty.");
        }
        return HeapOperations.RemoveMin(ref items);
    }
    public T GetMin()
    {
        if (items.Length == 0)
        {
            throw new InvalidOperationException("Heap is empty.");
        }
        return items[0];
    }
    // 4. 힙 연산들을 Burst 컴파일 가능한 정적 클래스로 분리 (내부 구현)
    [BurstCompile]
    private static class HeapOperations
    {
        public static void Add<TItem>(ref NativeList<TItem> heapArray, TItem item)
            where TItem : unmanaged, IHeapItem<TItem>
        {
            item.HeapIndex = heapArray.Length; // 새로 추가될 아이템의 현재 인덱스 설정
            heapArray.Add(item); // NativeList의 맨 끝에 아이템 추가

            HeapifyUp(ref heapArray, item.HeapIndex); // 상향 조정 시작
        }
        public static TItem RemoveMin<TItem>(ref NativeList<TItem> heapArray)
            where TItem : unmanaged, IHeapItem<TItem>
        {
            TItem firstItem = heapArray[0]; // 힙의 최솟값 (루트)
            // 힙의 마지막 아이템을 루트 위치로 이동
            heapArray[0] = heapArray[heapArray.Length - 1];
            // 새 루트 아이템의 HeapIndex를 0으로 업데이트
            TItem newRoot = heapArray[0]; // 먼저 읽어와서
            newRoot.HeapIndex = 0;        // 수정하고
            heapArray[0] = newRoot;       // 다시 할당
            heapArray.RemoveAtSwapBack(heapArray.Length - 1); // NativeList의 마지막 요소 제거
            if (heapArray.Length > 0)
            {
                HeapifyDown(ref heapArray, 0); // 루트에서부터 하향 조정 시작
            }
            return firstItem;
        }
        // 상향 조정 (Heapify Up): 새로 추가된(또는 변경된) 노드를 부모와 비교하며 올림
        public static void HeapifyUp<TItem>(ref NativeList<TItem> heapArray, int currentIndex)
            where TItem : unmanaged, IHeapItem<TItem>
        {
            while (currentIndex > 0)
            {
                int parentIndex = (currentIndex - 1) / 2; // 부모 노드 인덱스
                if (heapArray[currentIndex].GetHeapValue() < heapArray[parentIndex].GetHeapValue())
                {
                    Swap(ref heapArray, currentIndex, parentIndex);
                    currentIndex = parentIndex; // 부모 위치로 이동하여 계속 상향 조정
                }
                else
                {
                    break; // 힙 속성 만족, 정지
                }
            }
        }
        // 하향 조정 (Heapify Down): 노드를 자식과 비교하며 내림
        public static void HeapifyDown<TItem>(ref NativeList<TItem> heapArray, int currentIndex)
            where TItem : unmanaged, IHeapItem<TItem>
        {
            int length = heapArray.Length;
            while (true)
            {
                int childIndexLeft = currentIndex * 2 + 1;
                int childIndexRight = currentIndex * 2 + 2;
                int swapIndex = currentIndex;
                if (childIndexLeft < length && heapArray[childIndexLeft].GetHeapValue() < heapArray[swapIndex].GetHeapValue())
                {
                    swapIndex = childIndexLeft;
                }
                if (childIndexRight < length && heapArray[childIndexRight].GetHeapValue() < heapArray[swapIndex].GetHeapValue())
                {
                    swapIndex = childIndexRight;
                }
                if (swapIndex != currentIndex)
                {
                    Swap(ref heapArray, currentIndex, swapIndex);
                    currentIndex = swapIndex;
                }
                else
                {
                    break;
                }
            }
        }
        // 두 노드를 스왑하는 헬퍼 메서드 (오류 수정됨)
        private static void Swap<TItem>(ref NativeList<TItem> heapArray, int indexA, int indexB)
            where TItem : unmanaged, IHeapItem<TItem>
        {
            // 1. 두 아이템을 로컬 변수로 읽어옵니다. (여기서 복사본이 생성됩니다.)
            TItem itemA = heapArray[indexA];
            TItem itemB = heapArray[indexB];
            // 2. 로컬 변수에 있는 아이템의 HeapIndex를 업데이트합니다.
            //    (itemA는 indexB로, itemB는 indexA로 이동할 것이므로)
            itemA.HeapIndex = indexB;
            itemB.HeapIndex = indexA;
            // 3. 수정된 로컬 변수를 NativeList의 새로운 위치에 다시 할당합니다.
            //    이렇게 하면 NativeList 내부의 원본 struct가 업데이트됩니다.
            heapArray[indexA] = itemB; // itemB를 indexA 위치에 넣고
            heapArray[indexB] = itemA; // itemA를 indexB 위치에 넣습니다.
        }
    }
}