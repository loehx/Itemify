using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Itemify.Core;
using Itemify.Core.Item;
using Itemify.Logging;
using Itemify.Shared.Utils;
using Itemify.Util;
using Xunit;

namespace Itemify.Tests
{
    public class ItemifyTests
    {
        private ILogWriter log;
        private Itemify itemify;

        private const int CONNECTION_POOL_SIZE = 50;
        private const int INSERT_ITEMS = 1000;

        public ItemifyTests()
        {
#if DEBUG
            var logData = new CustomLogData(l =>
            {
                Debug.WriteLine(l);
                Console.WriteLine(l);
            });

            this.log = new RegionBasedLogWriter(logData, nameof(BuildingStructureTest));
#else
            this.log = new EmptyLogWriter();
#endif

            var settings = new ItemifySettings(host: "localhost",
                port: 5432,
                username: "test",
                password: "test",
                database: "itemify_tests",
                connectionPoolSize: CONNECTION_POOL_SIZE,
                timeout: (int)TimeSpan.FromSeconds(20).TotalMilliseconds);

            //var settings = new ItemifySettings(host: "134.168.62.120",
            //    port: 5432,
            //    username: "postgres_dawid",
            //    password: "LustitiaDev",
            //    database: "postgres_dawid",
            //    connectionPoolSize: MAX_CONNECTIONS,
            //    timeout: (int)TimeSpan.FromSeconds(5).TotalMilliseconds);

            this.itemify = new Itemify(settings, this.log);
            this.itemify.ResetCompletely();
        }

        [Fact]
        public void StressTest_Save()
        {
            var items = newItems(INSERT_ITEMS, i => "device", i => 1).ToList();

            items.AsParallel().WithDegreeOfParallelism(CONNECTION_POOL_SIZE).ForAll(item =>
            {
                itemify.Save(item);
            });

            var actualItems = itemify.GetChildrenOfItemByReference(itemify.Root, "device").ToArray();
            Assert.Equal(items.Count, actualItems.Length);

            actualItems.ForEach(actualItem =>
            {
                var item = items.First(k => k.Guid == actualItem.Guid);

                Assert.NotNull(item);
                Assert.Equal(item.Name, actualItem.Name);
                Assert.Equal(item.Type, actualItem.Type);
                Assert.Equal(item.ValueNumber, actualItem.ValueNumber);
                Assert.Equal(item.ValueString, actualItem.ValueString);
                Assert.Equal(item.ValueDate, actualItem.ValueDate);
            });
        }

        [Fact]
        public void StressTest_SaveNew()
        {
            var items = newItems(INSERT_ITEMS, i => "device", i => 1).ToList();

            items.AsParallel().WithDegreeOfParallelism(CONNECTION_POOL_SIZE).ForAll(item =>
            {
                itemify.SaveNew(item);
            });

            var actualItems = itemify.GetChildrenOfItemByReference(itemify.Root, "device").ToArray();
            Assert.Equal(items.Count, actualItems.Length);

            actualItems.ForEach(actualItem =>
            {
                var item = items.First(k => k.Guid == actualItem.Guid);

                Assert.Equal(item.Name, actualItem.Name);
                Assert.Equal(item.Type, actualItem.Type);
                Assert.Equal(item.ValueNumber, actualItem.ValueNumber);
                Assert.Equal(item.ValueString, actualItem.ValueString);
                Assert.Equal(item.ValueDate, actualItem.ValueDate);
            });
        }

        [Theory]
        [InlineData("001", 1, INSERT_ITEMS)]
        [InlineData("002", 2, INSERT_ITEMS)]
        [InlineData("005", 5, INSERT_ITEMS)]
        [InlineData("010", 10, INSERT_ITEMS)]
        public void StressTest_SaveNew_GetSingleItems(string order, int maxParallel, int itemCount)
        {
            var items = newItems(itemCount, i => "device", i => 1).ToList();
            var threads = Hashtable.Synchronized(new Hashtable());

            var count = items.AsParallel()
                .WithMergeOptions(ParallelMergeOptions.AutoBuffered)
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(maxParallel)
                .Count(item =>
            {
                itemify.SaveNew(item);

                var actualItem = itemify.GetItemByReference(item);

                threads[Thread.CurrentThread.ManagedThreadId] = true;

                Assert.Equal(item.Name, actualItem.Name);
                Assert.Equal(item.Type, actualItem.Type);
                Assert.Equal(item.ValueNumber, actualItem.ValueNumber);
                Assert.Equal(item.ValueString, actualItem.ValueString);
                Assert.Equal(item.ValueDate, actualItem.ValueDate);

                return true;
            });

            // Detect if there where less threads active than expected.
            //Assert.Equal(maxParallel, threads.Count);

            Assert.Equal(items.Count, count);
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 3)]
        [InlineData(5, 2)]
        [InlineData(10, 1)]
        public void StressTest_CreateComplexItemTree(int maxDeepness, int itemCountPerLevel)
        {
            var adam = newItems(1, i => "Adam", i => 0).FirstOrDefault();
            itemify.SaveNew(adam);

            var items = createItemsRecursively(adam, itemCountPerLevel, 0, maxDeepness)
                .AsParallel()
                .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                .WithDegreeOfParallelism(10)
                .Select(item =>
                {
                    itemify.SaveNew(item);
                    return item;
                })
                .ToArray();

            var typeLevels = (maxDeepness + 1).Times()
                .Skip(1)
                .Select(i => "Adam" + i.Times().Select(k => " Junior").Join(""))
                .ToArray(); // eg. { "Adam", "Adam Junior", "Adam Junior Junior" };

            var typesFound = new Hashtable();
            var descendents = this.itemify.GetItemsByTypes(ItemResolving.Default.ResolveParentItem(), typeLevels).ToArray();

            Assert.Equal(items.Length, descendents.Length);

            descendents.ForEach(item =>
            {
                typesFound[item.Type] = true;

                var level = item.GetBody<int>();
                Assert.Equal(level - 1, item.Parent.AsItem()?.GetBody<int>());
            });

            Assert.Equal(typeLevels.Length, typesFound.Count);
        }

        private static IEnumerable<Item> createItemsRecursively(Item parentItem, int childrenCount, int level, int maxLevel)
        {
            if (level >= maxLevel) yield break;

            level++;

            var children = newItems(childrenCount, i => parentItem.Type + " Junior", i => level);

            foreach (var child in children)
            {
                child.Parent = parentItem;
                yield return child;

                foreach (var grandChild in createItemsRecursively(child, childrenCount, level, maxLevel))
                    yield return grandChild;
            }
        }

        private static IEnumerable<Item> newItems(int count, Func<int, string> typeGetter, Func<int, object> bodyGetter)
        {
            for (int i = 0; i < count; i++)
            {
                var item = new Item(Guid.NewGuid(), typeGetter(i));

                item.Name = "Test item #" + (i + 1);
                item.SetBody(bodyGetter(i));
                item.ValueNumber = i;
                item.ValueString = new string('+', 1024);

                yield return item;
            }
        }

    }
}
