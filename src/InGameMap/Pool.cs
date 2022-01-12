using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModPack.InGameMap
{
	public interface IPoolable
	{
		public void InitialCreation();

		public void OnReturnToPool();

		public void OnBorrowed(params object[] data);
	}

	public class Pool
    {
		public static T Borrow<T>(params object[] data) where T : IPoolable
        {
			return Pool<T>.Instance.Borrow(data);
        }

		public static void Return<T>(T obj) where T : IPoolable
        {
			Pool<T>.Instance.Return(obj);
        }
    }

	public class Pool<T> : Pool where T : IPoolable
	{
		internal static Pool<T> Instance
        {
			get => instance ??= new();
        }
		private static Pool<T> instance;

		public Pool()
        {
			instance = this;
        }

		private readonly HashSet<T> pooled = new();
		private readonly HashSet<T> borrowed = new();

		public T CreateNew()
		{
			T ret = (T)Activator.CreateInstance(typeof(T));
			ret.InitialCreation();
			return ret;
		}

		public T Borrow(params object[] data)
		{
			T first;
			if (pooled.Any())
				first = pooled.First();
			else
				first = CreateNew();

			pooled.Remove(first);
			borrowed.Add(first);

			first.OnBorrowed(data);

			return first;
		}

		public void Return(T obj)
		{
			borrowed.Remove(obj);
			pooled.Add(obj);

			obj.OnReturnToPool();
		}
	}
}
