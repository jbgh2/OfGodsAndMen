﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

//From: http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
public static class ThreadSafeRandom
{
	[ThreadStatic] private static Random Local;
	
	public static Random ThisThreadsRandom
	{
		get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
	}
}

static class MyExtensions
{
	public static void Shuffle<T>(this IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}