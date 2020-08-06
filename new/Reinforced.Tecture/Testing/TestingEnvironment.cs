﻿using System;
using System.Threading.Tasks;
using Reinforced.Tecture.Channels.Multiplexer;
using Reinforced.Tecture.Testing.Query;
using Reinforced.Tecture.Testing.Stories;

namespace Reinforced.Tecture.Testing
{
    /// <summary>
    /// Provides testing environment for your code and services
    /// </summary>
    public class TestingEnvironment
    {
        private readonly TestData _testData;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public TestingEnvironment(TestData testData)
        {
            _testData = testData;
        }

        internal readonly ChannelMultiplexer _mx = new ChannelMultiplexer();
        
        private void OnException(Exception ex)
        {
            throw new TestRunException(ex);
        }
        

        /// <summary>
        /// Tells story about particular piece of code
        /// </summary>
        /// <param name="code">Tecture code (without Save)</param>
        /// <returns>Storage story</returns>
        public StorageStory TellStory(Action<ITectureNoSave> code)
        {
            var tcd = new TestingCommandsDispatcher(_mx);
            var tec = new Entry.Tecture(_mx, tcd, true, _testData, null, OnException);
            code(tec);
            tcd.BeginStory();
            tec.Save();
            return tcd.EndStory(this);
        }

        /// <summary>
        /// Tells story about particular piece of code (async)
        /// </summary>
        /// <param name="code">Tecture code (without Save)</param>
        /// <returns>Storage story</returns>
        public async Task<StorageStory> TellStoryAsync(Func<ITectureNoSave,Task> code)
        {
            var tcd = new TestingCommandsDispatcher(_mx);
            var tec = new Entry.Tecture(_mx, tcd, true, _testData, null, OnException);
            await code(tec);
            tcd.BeginStory();
            await tec.SaveAsync();
            return tcd.EndStory(this);
        }
    }
}
