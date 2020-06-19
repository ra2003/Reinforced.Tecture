﻿using System;
using Reinforced.Tecture.Channels;
using Reinforced.Tecture.Entry.Builders;
using Reinforced.Tecture.Transactions;

namespace Reinforced.Tecture.Entry
{
    public static class Extensions
    {

        public static TectureBuilder WithChannel<TChannel>(this TectureBuilder tb, Action<ChannelConfiguration<TChannel>> cfg) where TChannel : Channel
        {
            var cb = new ChannelConfigurationImpl<TChannel>(tb._mx);
            cfg(cb);
            return tb;
        }

        /// <summary>
        /// Points transaction manager to use
        /// </summary>
        /// <param name="tb">Tecture builder</param>
        /// <param name="manager">Transaction manager instance</param>
        /// <returns>Fluent</returns>
        public static TectureBuilder WithTransactions(this TectureBuilder tb, ITransactionManager manager)
        {
            tb._transactionManager = manager;
            return tb;
        }

        /// <summary>
        /// Adds exception handling method to be used by Tecture
        /// </summary>
        /// <param name="tb">Tecture builder</param>
        /// <param name="handler">Exception handler</param>
        /// <returns>Fluent</returns>
        public static TectureBuilder WithExceptionHandler(this TectureBuilder tb, Action<Exception> handler)
        {
            tb._excHandler = handler;
            return tb;
        }

    }
}
