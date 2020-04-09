﻿using System;
using Reinforced.Tecture.Entry;
using Reinforced.Tecture.Methodics.Orm.Commands.Add;
using Reinforced.Tecture.Methodics.Orm.Testing;
using Reinforced.Tecture.Methodics.Orm.Testing.Assumptions;
using Reinforced.Tecture.Playground.Entities;
using Reinforced.Tecture.Playground.Services;
using Reinforced.Tecture.Testing;

namespace Reinforced.Tecture.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var bld = new TectureBuilder();

            ITecture tc = bld.Build();

            tc.Do<OrdersService>().Operation();
            tc.Do<UserService>().Operation();

            tc.Save();
                

            var te = new TestingEnvironment()
                .Assume(x =>
                {
                    x.Orm(OrmAssumptions);
                })
                .WithOrmTesting();

            var story = te.TellStory(x =>
            {
                x.Do<UserService>().Operation();
            });

        }

        private static void OrmAssumptions(OrmAssuming obj)
        {
            obj.Assume<Add, Order>(x => x.Id = 50);
        }
    }
}