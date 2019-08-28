using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuffer<T>
{
    T[] backingValues;

    public int Count { get; private set; }

    private int frontIndex = 0;

    public RingBuffer(int bufferSize)
    {
        backingValues = new T[bufferSize];
        Count = 0;
    }

    public void Add(T item)
    {
        if (Count != backingValues.Length)
            Count++;

        backingValues[frontIndex] = item;
        frontIndex++;
        frontIndex = frontIndex % backingValues.Length;
    }

    public void Clear()
    {
        frontIndex = 0;
        Count = 0;
    }

    // returns values with zero being the most recent value
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                return default;

            index = frontIndex - index;

            if (index < 0)
                index = backingValues.Length + index;

            return backingValues[index];
        }
    }
}
