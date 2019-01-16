using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingBuffer<T>
{
    T[] backingValues;

    public int Count { get; private set; }

    private int backIndex = 0, frontIndex = 0;

    public RingBuffer(int bufferSize)
    {
        backingValues = new T[bufferSize];
        Count = 0;
    }

    public void Add(T item)
    {
        if (Count != backingValues.Length)
            Count++;
        else
            backIndex++;

        backingValues[frontIndex] = item;
        frontIndex++;
        frontIndex = frontIndex % backingValues.Length;
    }

    public void Clear()
    {
        frontIndex = 0;
        backIndex = 0;
        Count = 0;
    }

    public T this[int index]
    {
        get
        {
            return backingValues[index];
        }
    }
}
