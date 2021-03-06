﻿using XRouter.Common;
using XRouter.Common.ComponentInterfaces;

namespace XRouter.Broker
{
    /// <summary>
    /// A proxy for accessing another component. It holds some metadata about
    /// the component. Concrete accessors can be used eg. for cache some frequently queried values.
    /// </summary>
    abstract class ComponentAccessor
    {
        public string ComponentName { get; private set; }

        public ComponentType ComponentType { get; private set; }

        public XmlReduction ConfigurationReduction { get; set; }

        private IComponentService component;

        protected ComponentAccessor(
            string componentName,
            IComponentService component,
            ApplicationConfiguration configuration)
        {
            ComponentName = componentName;
            ComponentType = configuration.GetComponentType(componentName);
            this.component = component;

            ConfigurationReduction = new XmlReduction();
        }

        public void UpdateConfig(ApplicationConfiguration config)
        {
            component.UpdateConfig(config);
        }
    }
}
