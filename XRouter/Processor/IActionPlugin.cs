﻿using System;
using XRouter.Common;

namespace XRouter.Processor
{
    /// <summary>
    /// Represents a plugin which performs a message flow action with a token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Some built-in actions are avaiable in the XRouter.Processor.BuiltInActions
    /// namespace. New actions can be created by implementing this interface.
    /// </para>
    /// <para>
    /// An action plugin may be annotated by the <see
    /// cref="XRouter.Processor.ActionPluginAttribute"/> attribute for enhanced
    /// GUI editing of the action.
    /// </para>
    /// </remarks>
    public interface IActionPlugin : IDisposable
    {
        /// <summary>
        /// Initializes the action plugin given a reference to the processor
        /// on which it runs.
        /// </summary>
        /// <param name="processorService"></param>
        void Initialize(IProcessorServiceForAction processorService);

        /// <summary>
        /// Perfoms the action with the specified token.
        /// </summary>
        /// <param name="token"></param>
        void Evaluate(Token token);
    }
}
