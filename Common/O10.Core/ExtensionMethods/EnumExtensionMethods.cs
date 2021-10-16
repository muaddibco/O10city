using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Core.ExtensionMethods
{
	public static class EnumExtensionMethods
	{
		public static string GetDescription(this Enum genericEnum)
		{
			Type genericEnumType = genericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
				if ((attrs != null && attrs.Count() > 0))
				{
					return ((System.ComponentModel.DescriptionAttribute)attrs.ElementAt(0)).Description;
				}
			}
			return genericEnum.ToString();
		}

		public static string GetCategory(this Enum genericEnum)
		{
			Type genericEnumType = genericEnum.GetType();
			MemberInfo[] memberInfo = genericEnumType.GetMember(genericEnum.ToString());
			if ((memberInfo != null && memberInfo.Length > 0))
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.CategoryAttribute), false);
				if ((attrs != null && attrs.Count() > 0))
				{
					return ((System.ComponentModel.CategoryAttribute)attrs.ElementAt(0)).Category;
				}
			}
			return string.Empty;
		}

		public static Task AsyncParallelForEach<T>(this IEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
		{
			var options = new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism
			};
			if (scheduler != null)
				options.TaskScheduler = scheduler;

			var block = new ActionBlock<T>(body, options);

			foreach (var item in source)
				block.Post(item);

			block.Complete();
			return block.Completion;
		}

		public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
		{
			var options = new ExecutionDataflowBlockOptions
			{
				MaxDegreeOfParallelism = maxDegreeOfParallelism
			};
			if (scheduler != null)
				options.TaskScheduler = scheduler;

			var block = new ActionBlock<T>(body, options);

			await foreach (var item in source)
				block.Post(item);

			block.Complete();
			await block.Completion;
		}
	}
}
