﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using XRouter.Common.MessageFlowConfig;
using XRouter.Common;
using System.Windows;

namespace XRouter.Gui.ConfigurationControls.Messageflow
{
    delegate void NodeSelectingHandler(object sender, NodeSelectingEventArgs e);
    class NodeSelectingEventArgs : EventArgs
    {
        public NodeConfiguration OriginalSelectedNode { get; private set; }
        public NodeConfiguration NewSelectedNode { get; private set; }
        public bool IsCancelled { get; set; }

        public NodeSelectingEventArgs(NodeConfiguration originalSelectedNode, NodeConfiguration newSelectedNode)
        {
            OriginalSelectedNode = originalSelectedNode;
            NewSelectedNode = newSelectedNode;
        }
    }

    delegate void NodeSelectedHandler(object sender, NodeSelectedEventArgs e);
    class NodeSelectedEventArgs : EventArgs
    {
        public NodeConfiguration OriginalSelectedNode { get; private set; }
        public NodeConfiguration NewSelectedNode { get; private set; }

        public NodeSelectedEventArgs(NodeConfiguration originalSelectedNode, NodeConfiguration newSelectedNode)
        {
            OriginalSelectedNode = originalSelectedNode;
            NewSelectedNode = newSelectedNode;
        }
    }

    class NodeSelectionManager
    {
        public event NodeSelectingHandler NodeSelecting = delegate { };
        public event NodeSelectedHandler NodeSelected = delegate { };

        public NodeConfiguration SelectedNode { get; private set; }

        internal MessageflowGraphPresenter MessageflowGraphPresenter { get; set; }

        internal ConfigurationManager ConfigManager { get; private set; }
        internal ApplicationConfiguration AppConfig { get { return ConfigManager.Configuration; } }

        private Border propertiesContainer;

        public NodeSelectionManager(Border propertiesContainer, ConfigurationManager configManager)
        {
            this.propertiesContainer = propertiesContainer;
            ConfigManager = configManager;
        }

        public void SelectNode(NodeConfiguration selectedNode)
        {
            NodeConfiguration originalSelectedNode = SelectedNode;

            NodeSelectingEventArgs nodeSelectingEventArgs = new NodeSelectingEventArgs(originalSelectedNode, selectedNode);
            NodeSelecting(this, nodeSelectingEventArgs);
            if (nodeSelectingEventArgs.IsCancelled) {
                return;
            }

            if (selectedNode is ActionNodeConfiguration) {
                propertiesContainer.Child = new NodePropertiesEditors.ActionNodePropertiesEditor((ActionNodeConfiguration)selectedNode, this);
            } else if (selectedNode is CbrNodeConfiguration) {
                propertiesContainer.Child = new NodePropertiesEditors.CbrNodePropertiesEditor((CbrNodeConfiguration)selectedNode, this);
            } else if (selectedNode is TerminatorNodeConfiguration) {
                propertiesContainer.Child = new NodePropertiesEditors.TerminatorNodePropertiesEditor((TerminatorNodeConfiguration)selectedNode, this);
            } else if (selectedNode is EntryNodeConfiguration) {
                propertiesContainer.Child = new NodePropertiesEditors.EntryNodePropertiesEditor((EntryNodeConfiguration)selectedNode, this);
            } else {
                propertiesContainer.Child = null;
            }

            SelectedNode = selectedNode;
            NodeSelectedEventArgs nodeSelectedEventArgs = new NodeSelectedEventArgs(originalSelectedNode, selectedNode);
            NodeSelected(this, nodeSelectedEventArgs);
        }

        internal void RenameNode(NodeConfiguration node, TextBox uiName)
        {
            string[] existingNames = MessageflowGraphPresenter.Messageflow.Nodes.Where(n => n != node).Select(n => n.Name).ToArray();
            if (existingNames.Contains(uiName.Text)) {
                MessageBox.Show("Given name already exists. Please make sure that the node has a unique name.", "Node renaming", MessageBoxButton.OK, MessageBoxImage.Error);
                uiName.Text = node.Name;
                return;
            }
            node.Name = uiName.Text;
        }
    }
}
