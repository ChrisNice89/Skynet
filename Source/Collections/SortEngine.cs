using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Skynet.Memory;
using Skynet.Predicates;

namespace Skynet.Source.Collections.Memory;

internal static class SortEngine
{
    private const int MAX_INSERTION_SORT_THRESHOLD = 24;
    private const int ADVANCED_PIVOT_SELECTION_THRESHOLD = 128;
    private const int MAX_SHIFTLIMIT = 8;
    private const int BLOCK_SIZE = 128;

    private static readonly int[] m_OffsetL = new int[BLOCK_SIZE];
    private static readonly int[] m_OffsetR = new int[BLOCK_SIZE];
internal static RunOrder IsOrdered<T, TMemory, TOrder>(
    TMemory memory,
    int start,
    int count,
    TOrder order)
    where TMemory : IMemoryAccess<T>
    where TOrder : struct, IStrictOrder<T>
{
    if (count <= 1)
        return RunOrder.Ascending;

    ref T first = ref memory.At(0);

    return DetectOrder(
        ref first,
        start,
        start + count - 1,
        order);
}

    // Validierung erfolgt im Aufrufer
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int BinarySearchLeft<T, TMemory, TOrder>(
    TMemory memory,
    int start,
    int count,
    T element,
    TOrder order)
    where TMemory : IMemoryAccess<T>
    where TOrder : struct, IStrictOrder<T>
{
    if (count == 0)
        return ~start;

    ref T first = ref memory.At(start);

    int result = SearchLeft(ref first, count, element, order);

    return result >= 0
        ? start + result
        : ~(start + ~result);
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static int SearchLeft<T, TOrder>(
    ref T first,
    int count,
    T element,
    TOrder order)
    where TOrder : struct, IStrictOrder<T>
{
    int lower = 0;
    int upper = count - 1;

    while (lower < upper)
    {
        int i = lower + ((upper - lower) >> 1);

        if (order.IsLess(Unsafe.Add(ref first, i), element))
            lower = i + 1;
        else
            upper = i;
    }

    ref T candidate = ref Unsafe.Add(ref first, lower);

    if (order.IsLess(candidate, element))
        return ~(lower + 1);

    if (order.IsLess(element, candidate))
        return ~lower;

    return lower;
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
public static int BinarySearchRight<T, TMemory, TOrder>(
    TMemory memory,
    int start,
    int count,
    T element,
    TOrder order)
    where TMemory : IMemoryAccess<T>
    where TOrder : struct, IStrictOrder<T>
{
    if (count == 0)
        return ~start;

    ref T first = ref memory.At(start);

    int result = SearchRight(ref first, count, element, order);

    return result >= 0
        ? start + result
        : ~(start + ~result);
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
private static int SearchRight<T, TOrder>(
    ref T first,
    int count,
    T element,
    TOrder order)
    where TOrder : struct, IStrictOrder<T>
{
    int lower = 0;
    int upper = count - 1;

    while (lower <= upper)
    {
        int i = lower + ((upper - lower) >> 1);

        if (order.IsLess(element, Unsafe.Add(ref first, i)))
            upper = i - 1;
        else
            lower = i + 1;
    }

    if (upper < 0)
        return ~lower;

    if (!order.IsLess(element, Unsafe.Add(ref first, upper)))
        return upper;

    return ~lower;
}
    
    public static void Sort<T, TMemory, TOrder>(
        TMemory memory,
        int start,
        int count,
        TOrder order)
        where TMemory : Skynet.Memory.IMemoryAccess<T>
        where TOrder : struct, IStrictOrder<T>
    {
        if (count < 2)
        {
            return;
        }

        ref T first = ref memory.At(start);
        int right = count - 1;
        RunOrder detected = DetectOrder(ref first, 0, right, order);

        if (detected == RunOrder.Ascending)
        {
            return;
        }

        if (detected != RunOrder.None)
        {
            ReverseRange(ref first, 0, right);
            return;
        }

        int log2 = FloorLog2(count);
        HybridBlockSort(ref first, 0, right, log2, 2 * log2, false, order);
    }

    private static void HybridBlockSort<T, TOrder>(
        ref T keys, int left, int right, int badAllowed, int recursionLimit,
        bool hasPredecessor, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        bool isPartitioned = true;
        bool wasUnbalanced = false;
        int partitionSize = right - left + 1;

        while (partitionSize > 1)
        {
            if (partitionSize <= MAX_INSERTION_SORT_THRESHOLD)
            {
                BinaryInsertionSort(ref keys, left, right, order);
                return;
            }

            if (badAllowed == 0 || recursionLimit == 0)
            {
                HeapSort(ref keys, left, partitionSize, order);
                return;
            }

            recursionLimit--;

            int mid = SelectPivot(ref keys, left, partitionSize, out RunOrder hint, order);

            if (hint == RunOrder.Descending)
            {
                ReverseRange(ref keys, left, right);
                mid = right - (mid - left);
                hint = RunOrder.Ascending;
            }

            if (!wasUnbalanced && isPartitioned && hint == RunOrder.Ascending)
            {
                if (InsertionSortPartial(ref keys, left, right, ref mid, order) > right)
                    return;
            }

            if (hasPredecessor)
            {
                if (!order.IsLess(Unsafe.Add(ref keys, left - 1), Unsafe.Add(ref keys, mid)))
                {
                    left = PartitionEqualAsc(ref keys, left, right, mid, order);
                    HybridBlockSort(ref keys, left + 1, right, badAllowed, recursionLimit, true, order);
                    return;
                }
            }

            mid = PartitionAsc(ref keys, left, right, mid, out isPartitioned, order);

            int balanceThreshold = partitionSize >> 3;
            int leftSize = mid - left;
            int rightSize = right - mid;
            wasUnbalanced = (leftSize < balanceThreshold) | (rightSize < balanceThreshold);
            badAllowed -= ToInt(wasUnbalanced);

            HybridBlockSort(ref keys, left, mid - 1, badAllowed, recursionLimit, hasPredecessor, order);

            partitionSize = rightSize;
            left = mid + 1;
            hasPredecessor = true;
        }
    }

    private static void BinaryInsertionSort<T, TOrder>(ref T keys, int left, int right, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        int run = CountRun(ref keys, left, right, out RunOrder hint, order);
        if (hint == RunOrder.Descending)
            ReverseRange(ref keys, left, left + run - 1);

        for (int i = left + run; i <= right; i++)
        {
            T pivot = Unsafe.Add(ref keys, i);
            int lo = left;
            int hi = i;

            do
            {
                int mid = lo + ((hi - lo) >> 1);
                if (order.IsLess(pivot, Unsafe.Add(ref keys, mid))) hi = mid; else lo = mid + 1;
            }
            while (lo < hi);

            switch (i - lo)
            {
                case 0:
                    break;
                case 1:
                    Unsafe.Add(ref keys, i) = Unsafe.Add(ref keys, lo);
                    Unsafe.Add(ref keys, lo) = pivot;
                    break;
                case 2:
                    Unsafe.Add(ref keys, i) = Unsafe.Add(ref keys, i - 1);
                    Unsafe.Add(ref keys, i - 1) = Unsafe.Add(ref keys, lo);
                    Unsafe.Add(ref keys, lo) = pivot;
                    break;
                case 3:
                    Unsafe.Add(ref keys, i) = Unsafe.Add(ref keys, i - 1);
                    Unsafe.Add(ref keys, i - 1) = Unsafe.Add(ref keys, i - 2);
                    Unsafe.Add(ref keys, i - 2) = Unsafe.Add(ref keys, lo);
                    Unsafe.Add(ref keys, lo) = pivot;
                    break;
                default:
                    for (int j = i; j > lo; j--) Unsafe.Add(ref keys, j) = Unsafe.Add(ref keys, j - 1);
                    Unsafe.Add(ref keys, lo) = pivot;
                    break;
            }
        }
    }

    // Liefert die LÃ¤nge des Runs ab left. Duplikate gehÃ¶ren zum Run.
    // hint: Ascending = nicht fallend, Descending = nicht steigend, None = alle gleich
    private static int CountRun<T, TOrder>(ref T keys, int left, int right, out RunOrder hint, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        int head;
        int runEnd;

        hint = RunOrder.None;

        // Phase 1: fÃ¼hrende Duplikate Ã¼berspringen, Richtung bestimmen
        for (head = left + 1; head <= right; head++)
        {
            if (order.IsLess(Unsafe.Add(ref keys, head), Unsafe.Add(ref keys, head - 1)))
            {
                hint = RunOrder.Descending;
                break;
            }
            if (order.IsLess(Unsafe.Add(ref keys, head - 1), Unsafe.Add(ref keys, head)))
            {
                hint = RunOrder.Ascending;
                break;
            }
        }

        // Phase 2: Run in dieser Richtung verlÃ¤ngern, Duplikate eingeschlossen
        runEnd = head;
        if (hint == RunOrder.Ascending)
        {
            for (runEnd = head + 1; runEnd <= right; runEnd++)
                if (order.IsLess(Unsafe.Add(ref keys, runEnd), Unsafe.Add(ref keys, runEnd - 1))) break;
        }
        else if (hint == RunOrder.Descending)
        {
            for (runEnd = head + 1; runEnd <= right; runEnd++)
                if (order.IsLess(Unsafe.Add(ref keys, runEnd - 1), Unsafe.Add(ref keys, runEnd))) break;
        }

        return runEnd - left;
    }

    private static void HeapSort<T, TOrder>(ref T keys, int left, int partitionSize, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        if (partitionSize < 2) return;

        for (int i = partitionSize >> 1; i >= 1; i--)
            MaxHeap(ref keys, i, partitionSize, left, order);

        for (int i = partitionSize - 1; i >= 2; i--)
        {
            Swap(ref Unsafe.Add(ref keys, left), ref Unsafe.Add(ref keys, left + i));
            MaxHeap(ref keys, 1, i, left, order);
        }

        Swap(ref Unsafe.Add(ref keys, left), ref Unsafe.Add(ref keys, left + 1));
    }

    // Voraussetzung: 2 * i <= n
    private static void MaxHeap<T, TOrder>(ref T keys, int i, int n, int lo, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        int bas = lo - 1;
        T pivot = Unsafe.Add(ref keys, bas + i);
        int child = 2 * i;

        do
        {
            if (child < n)
                child += ToInt(order.IsLess(Unsafe.Add(ref keys, bas + child), Unsafe.Add(ref keys, bas + child + 1)));

            if (order.IsLess(Unsafe.Add(ref keys, bas + child), pivot)) break;

            Unsafe.Add(ref keys, bas + i) = Unsafe.Add(ref keys, bas + child);
            i = child;
            child = 2 * i;
        }
        while (child <= n);

        Unsafe.Add(ref keys, bas + i) = pivot;
    }

    private static int SelectPivot<T, TOrder>(ref T keys, int left, int partitionSize, out RunOrder hint, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        if (partitionSize > ADVANCED_PIVOT_SELECTION_THRESHOLD)
        {
            int q = partitionSize >> 3;
            int q1 = Median3(ref keys, left, left + q, left + 2 * q, out int s1, order);
            int q2 = Median3(ref keys, left + 3 * q, left + 4 * q, left + 5 * q, out int s2, order);
            int q3 = Median3(ref keys, left + 6 * q, left + 7 * q, left + partitionSize - 1, out int s3, order);

            int result = Median3(ref keys, q1, q2, q3, out int s, order);

            switch (s + s1 + s2 + s3)
            {
                case 0: hint = RunOrder.Ascending; break;
                case 12: hint = RunOrder.Descending; break;
                default: hint = RunOrder.None; break;
            }

            return result;
        }

        hint = RunOrder.None;
        return Median3(ref keys, left, left + (partitionSize >> 1), left + partitionSize - 1, out _, order);
    }

    private static int Median3<T, TOrder>(ref T keys, int a, int b, int c, out int swaps, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        ref T va = ref Unsafe.Add(ref keys, a);
        int x = ToInt(order.IsLess(Unsafe.Add(ref keys, b), va));
        int y = ToInt(order.IsLess(Unsafe.Add(ref keys, c), va));
        int z = 0;

        if (x == y)
        {
            z = ToInt(order.IsLess(Unsafe.Add(ref keys, c), Unsafe.Add(ref keys, b)));
            a = ((y ^ z) == 1) ? c : b;
        }

        swaps = x + y + z;
        return a;
    }

    private static void ReverseRange<T>(ref T keys, int i, int j)
    {
        int count = (j - i + 1) >> 1;

        if (count > 7)
        {
            int fence = i + (count & ~7);
            do
            {
                Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
                Swap(ref Unsafe.Add(ref keys, i + 1), ref Unsafe.Add(ref keys, j - 1));
                Swap(ref Unsafe.Add(ref keys, i + 2), ref Unsafe.Add(ref keys, j - 2));
                Swap(ref Unsafe.Add(ref keys, i + 3), ref Unsafe.Add(ref keys, j - 3));
                Swap(ref Unsafe.Add(ref keys, i + 4), ref Unsafe.Add(ref keys, j - 4));
                Swap(ref Unsafe.Add(ref keys, i + 5), ref Unsafe.Add(ref keys, j - 5));
                Swap(ref Unsafe.Add(ref keys, i + 6), ref Unsafe.Add(ref keys, j - 6));
                Swap(ref Unsafe.Add(ref keys, i + 7), ref Unsafe.Add(ref keys, j - 7));
                i += 8; j -= 8;
            }
            while (i < fence);
        }

        if ((count & 4) != 0)
        {
            Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
            Swap(ref Unsafe.Add(ref keys, i + 1), ref Unsafe.Add(ref keys, j - 1));
            Swap(ref Unsafe.Add(ref keys, i + 2), ref Unsafe.Add(ref keys, j - 2));
            Swap(ref Unsafe.Add(ref keys, i + 3), ref Unsafe.Add(ref keys, j - 3));
            i += 4; j -= 4;
        }

        if ((count & 2) != 0)
        {
            Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
            Swap(ref Unsafe.Add(ref keys, i + 1), ref Unsafe.Add(ref keys, j - 1));
            i += 2; j -= 2;
        }

        if ((count & 1) != 0)
            Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
    }

    // Liefert right + 1 bei vollstÃ¤ndiger Sortierung, sonst den Abbruchindex.
    // mid wird mitgefÃ¼hrt: zeigt danach wieder auf dasselbe Pivot-Element.
    private static int InsertionSortPartial<T, TOrder>(ref T keys, int left, int right, ref int mid, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        int i;
        int shifts = 0;

        for (i = left + 1; i <= right; i++)
        {
            if (order.IsLess(Unsafe.Add(ref keys, i), Unsafe.Add(ref keys, i - 1)))
            {
                T pivot = Unsafe.Add(ref keys, i);
                int sift = i;
                do
                {
                    Unsafe.Add(ref keys, sift) = Unsafe.Add(ref keys, sift - 1);
                    sift--;
                    if (sift == left) break;
                }
                while (order.IsLess(pivot, Unsafe.Add(ref keys, sift - 1)));
                Unsafe.Add(ref keys, sift) = pivot;

                // [sift..i-1] -> +1, i -> sift
                if (mid >= sift)
                {
                    if (mid < i) mid++;
                    else if (mid == i) mid = sift;
                }

                shifts += i - sift;
                if (shifts > MAX_SHIFTLIMIT) break;
            }
        }

        return i;
    }

    // [left..Ergebnis] == Pivot, (Ergebnis..right] > Pivot
    private static int PartitionEqualAsc<T, TOrder>(ref T keys, int left, int right, int mid, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        T pivot = Unsafe.Add(ref keys, mid);
        Unsafe.Add(ref keys, mid) = Unsafe.Add(ref keys, left);
        Unsafe.Add(ref keys, left) = pivot;

        int i = left + 1;
        int j = right;

        if (order.IsLess(pivot, Unsafe.Add(ref keys, j)))  // Unguarded loop(s)
        {
            j--;
            while (order.IsLess(pivot, Unsafe.Add(ref keys, j))) j--;    // Guard: Pivot auf left
            while (!order.IsLess(pivot, Unsafe.Add(ref keys, i))) i++;   // Guard: keys[right] > Pivot
        }
        else
        {
            for (i = left + 1; i <= right - 1; i++)                // keys[right] <= Pivot -> kein Guard
                if (order.IsLess(pivot, Unsafe.Add(ref keys, i))) break;
        }

        while (i < j)
        {
            Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
            i++; j--;
            while (!order.IsLess(pivot, Unsafe.Add(ref keys, i))) i++;
            while (order.IsLess(pivot, Unsafe.Add(ref keys, j))) j--;
        }

        Unsafe.Add(ref keys, left) = Unsafe.Add(ref keys, j);
        Unsafe.Add(ref keys, j) = pivot;

        return j;
    }

    private static int PartitionAsc<T, TOrder>(ref T keys, int left, int right, int mid, out bool alreadyPartitioned, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        T pivot = Unsafe.Add(ref keys, mid);
        Unsafe.Add(ref keys, mid) = Unsafe.Add(ref keys, left);
        Unsafe.Add(ref keys, left) = pivot;

        int i = left + 1;
        int j = right;

        // Pivot ist Median aus >= 3 Elementen -> mind. ein Element >= Pivot in (left..right]
        if (order.IsLess(Unsafe.Add(ref keys, i), pivot))  // Unguarded loop(s)
        {
            i++;
            while (order.IsLess(Unsafe.Add(ref keys, i), pivot)) i++;
            while (!order.IsLess(Unsafe.Add(ref keys, j), pivot)) j--;
        }
        else
        {
            for (j = right; j >= i; j--)
                if (order.IsLess(Unsafe.Add(ref keys, j), pivot)) break;
        }

        alreadyPartitioned = i >= j;

        if (!alreadyPartitioned)
        {
            if ((j - i) + 1 > BLOCK_SIZE)
            {
                Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
                i = i + 1 + BlockPartitionAsc(ref keys, i + 1, j - 1, in pivot, order);
            }
            else
            {
                do
                {
                    Swap(ref Unsafe.Add(ref keys, i), ref Unsafe.Add(ref keys, j));
                    i++; j--;
                    while (order.IsLess(Unsafe.Add(ref keys, i), pivot)) i++;
                    while (!order.IsLess(Unsafe.Add(ref keys, j), pivot)) j--;
                }
                while (i < j);
            }
        }

        mid = i - 1;
        Unsafe.Add(ref keys, left) = Unsafe.Add(ref keys, mid);
        Unsafe.Add(ref keys, mid) = pivot;

        return mid;
    }

    // Liefert die Anzahl Elemente < Pivot in [left..right]
    private static int BlockPartitionAsc<T, TOrder>(ref T keys, int left, int right, in T pivot, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        ref int offL = ref m_OffsetL[0];
        ref int offR = ref m_OffsetR[0];

        bool processComplete = false;
        int b, c, n;
        int blockL = BLOCK_SIZE;
        int blockR = BLOCK_SIZE;
        int startLeft = 0, endLeft = 0;
        int startRight = 0, endRight = 0;
        int i = left;
        int j = right;

        do
        {
            int partitionSize = (j - i) + 1;

            if (partitionSize <= 2 * BLOCK_SIZE)
            {
                if (startLeft < endLeft)
                {
                    blockR = partitionSize - BLOCK_SIZE;
                }
                else if (startRight < endRight)
                {
                    blockL = partitionSize - BLOCK_SIZE;
                }
                else
                {
                    blockL = partitionSize >> 1;
                    blockR = partitionSize - blockL;
                }
                processComplete = true;
            }

            if (startLeft == endLeft)
            {
                startLeft = 0; endLeft = 0;
                n = blockL & ~7;
                for (b = i; b <= i + n - 1; b += 8)
                {
                    Unsafe.Add(ref offL, endLeft) = b;     endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 1; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 1), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 2; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 2), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 3; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 3), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 4; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 4), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 5; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 5), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 6; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 6), pivot));
                    Unsafe.Add(ref offL, endLeft) = b + 7; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b + 7), pivot));
                }
                for (b = i + n; b <= i + blockL - 1; b++)
                {
                    Unsafe.Add(ref offL, endLeft) = b; endLeft += ToInt(!order.IsLess(Unsafe.Add(ref keys, b), pivot));
                }
            }

            if (startRight == endRight)
            {
                startRight = 0; endRight = 0;
                n = blockR & ~7;
                for (b = j; b >= j - n + 1; b -= 8)
                {
                    Unsafe.Add(ref offR, endRight) = b;     endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 1; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 1), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 2; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 2), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 3; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 3), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 4; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 4), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 5; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 5), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 6; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 6), pivot));
                    Unsafe.Add(ref offR, endRight) = b - 7; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b - 7), pivot));
                }
                for (b = j - n; b >= j - blockR + 1; b--)
                {
                    Unsafe.Add(ref offR, endRight) = b; endRight += ToInt(order.IsLess(Unsafe.Add(ref keys, b), pivot));
                }
            }

            c = endLeft - startLeft;
            if (c > endRight - startRight) c = endRight - startRight;

            if (c > 0)
            {
                SwapOffsets(ref keys, ref offL, startLeft, ref offR, startRight, c);
                startLeft += c;
                startRight += c;
            }

            if (startLeft == endLeft) i += blockL;
            if (startRight == endRight) j -= blockR;
        }
        while (!processComplete);

        if (startLeft < endLeft)
        {
            do
            {
                endLeft--;
                b = Unsafe.Add(ref offL, endLeft);
                Swap(ref Unsafe.Add(ref keys, b), ref Unsafe.Add(ref keys, j));
                j--;
            }
            while (startLeft < endLeft);

            return (j - left) + 1;
        }

        while (startRight < endRight)
        {
            endRight--;
            b = Unsafe.Add(ref offR, endRight);
            Swap(ref Unsafe.Add(ref keys, b), ref Unsafe.Add(ref keys, i));
            i++;
        }

        return i - left;
    }

    private static void SwapOffsets<T>(ref T p, ref int offsetL, int startL, ref int offsetR, int startR, int count)
    {
        int t;
        T tmp = Unsafe.Add(ref p, Unsafe.Add(ref offsetL, startL));
        Unsafe.Add(ref p, Unsafe.Add(ref offsetL, startL)) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, startR));
        count--;

        int left = startL + 1;
        int right = startR + 1;

        if (count >= 8)
        {
            int fence = left + (count & ~7);
            do
            {
                t = Unsafe.Add(ref offsetL, left);     Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right - 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right));
                t = Unsafe.Add(ref offsetL, left + 1); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right))     = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 1));
                t = Unsafe.Add(ref offsetL, left + 2); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 2));
                t = Unsafe.Add(ref offsetL, left + 3); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 2)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 3));
                t = Unsafe.Add(ref offsetL, left + 4); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 3)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 4));
                t = Unsafe.Add(ref offsetL, left + 5); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 4)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 5));
                t = Unsafe.Add(ref offsetL, left + 6); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 5)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 6));
                t = Unsafe.Add(ref offsetL, left + 7); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 6)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 7));
                left += 8; right += 8;
            }
            while (left < fence);
        }

        if ((count & 4) != 0)
        {
            t = Unsafe.Add(ref offsetL, left);     Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right - 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right));
            t = Unsafe.Add(ref offsetL, left + 1); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right))     = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 1));
            t = Unsafe.Add(ref offsetL, left + 2); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 2));
            t = Unsafe.Add(ref offsetL, left + 3); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 2)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 3));
            left += 4; right += 4;
        }

        if ((count & 2) != 0)
        {
            t = Unsafe.Add(ref offsetL, left);     Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right - 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right));
            t = Unsafe.Add(ref offsetL, left + 1); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right))     = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right + 1));
            left += 2; right += 2;
        }

        if ((count & 1) != 0)
        {
            t = Unsafe.Add(ref offsetL, left); Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right - 1)) = Unsafe.Add(ref p, t); Unsafe.Add(ref p, t) = Unsafe.Add(ref p, Unsafe.Add(ref offsetR, right));
        }

        Unsafe.Add(ref p, Unsafe.Add(ref offsetR, startR + count)) = tmp;
    }

    private static int FloorLog2(int n)
    {
        int result = 0;
        while (n > 1)
        {
            n >>= 1;
            result++;
        }
        return result;
    }

    // Ascending = nicht fallend, Descending = nicht steigend, None = unsortiert
    // Voraussetzung: left < right
    private static RunOrder DetectOrder<T, TOrder>(ref T keys, int left, int right, TOrder order)
        where TOrder : struct, IStrictOrder<T>
    {
        if (!order.IsLess(Unsafe.Add(ref keys, right), Unsafe.Add(ref keys, left)))
        {
            for (int i = left + 1; i <= right; i++)
                if (order.IsLess(Unsafe.Add(ref keys, i), Unsafe.Add(ref keys, i - 1))) return RunOrder.None;

            return RunOrder.Ascending;
        }

        for (int i = left + 1; i <= right; i++)
            if (order.IsLess(Unsafe.Add(ref keys, i - 1), Unsafe.Add(ref keys, i))) return RunOrder.None;

        return RunOrder.Descending;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref T ElementAt<T>(ref T source, int offset)
        => ref Unsafe.Add(ref source, offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TTo As<TFrom, TTo>(ref TFrom value)
        => Unsafe.As<TFrom, TTo>(ref value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ToInt(bool value) => As<bool, byte>(ref value);
}
