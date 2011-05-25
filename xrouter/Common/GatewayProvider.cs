﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XRouter.Common.ComponentInterfaces;

namespace XRouter.Common
{
    public class GatewayProvider
    {
        public string Name { get; private set; }

        public IGatewayService Gateway{ get; private set; }

        public GatewayProvider(string name, IGatewayService gateway)
        {
            Name = name;
            Gateway = gateway;
        }
    }
}
