﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Reinforced.Tecture.Features.Orm.Testing.Checks.Delete;
using Reinforced.Tecture.Testing.Generation;

namespace Reinforced.Tecture.Features.Orm.Testing.Checks.Update
{
    sealed class UpdateCheckDescription : CheckDescription<Commands.Update.Update>
    {
        public override MethodInfo Method =>
            UseMethod(() => UpdateChecks.Update<object>(null));

        protected override Type[] GetTypeArguments(Commands.Update.Update command)
        {
            return new Type[] { command.EntityType };
        }
    }
}