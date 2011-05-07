﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ObjectRemoter;
using XRouter.Common;

namespace XRouter.Broker
{
    // methods to be called by any component
    public interface IBrokerServiceForComponent : IRemotelyReferable
    {
        ApplicationConfiguration GetConfiguration(XmlReduction reduction);

        void UpdateComponentControllerAddress(string componentName, Uri controllerAddress);
        void UpdateComponentInfo(string componentName, Uri componentAddress, XmlReduction configReduction);
    }
}
