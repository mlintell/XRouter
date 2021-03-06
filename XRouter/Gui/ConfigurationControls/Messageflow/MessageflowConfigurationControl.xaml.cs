﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using SimpleDiagrammer;
using XRouter.Common;
using XRouter.Common.ComponentInterfaces;
using XRouter.Common.MessageFlowConfig;
using XRouter.Gui.Utils;
using XRouter.Manager;
using ObjectConfigurator;
using System.Xml.Linq;

namespace XRouter.Gui.ConfigurationControls.Messageflow
{
    /// <summary>
    /// Interaction logic for MessageflowConfigurationControl.xaml
    /// </summary>
    public partial class MessageflowConfigurationControl : UserControl, IConfigurationControl
    {
        public event Action ConfigChanged = delegate { };

        private ConfigurationTreeItem ConfigTreeItem { get; set; }
        private ConfigurationManager ConfigManager { get; set; }

        internal MessageFlowConfiguration Messageflow { get; private set; }
        internal NodeSelectionManager NodeSelectionManager { get; private set; }
        internal MessageflowGraphPresenter MessageflowGraphPresenter { get; private set; }

        private GraphCanvas graphCanvas;

        private ConfigurationEditor uiLayoutConfigEditor;

        public MessageflowConfigurationControl()
        {
            InitializeComponent();
        }

        void IConfigurationControl.Initialize(ConfigurationManager configManager, ConfigurationTreeItem configTreeItem)
        {
            ConfigManager = configManager;
            ConfigTreeItem = configTreeItem;

            NodeSelectionManager = new NodeSelectionManager(uiNodePropertiesContainer, ConfigManager);

            Messageflow = ConfigManager.Configuration.GetMessageFlow();
            MessageflowGraphPresenter = new MessageflowGraphPresenter(Messageflow, NodeSelectionManager);
            NodeSelectionManager.MessageflowGraphPresenter = MessageflowGraphPresenter;
            
            graphCanvas = MessageflowGraphPresenter.CreateGraphCanvas();
            uiDesignerContainer.Child = graphCanvas;
            uiDesignerContainer.ContextMenu = CreateGraphCanvasContextMenu();

            PrepareLayoutConfiguration();
        }

        private void PrepareLayoutConfiguration()
        {
            object layoutConfiguration = graphCanvas.CreateDefaultLayoutConfiguration();
            if (Messageflow.LayoutConfiguration != null) {
                Configurator.LoadConfiguration(layoutConfiguration, Messageflow.LayoutConfiguration);
            } else {
                Messageflow.LayoutConfiguration = new SerializableXDocument(Configurator.SaveConfiguration(layoutConfiguration));
            }
            graphCanvas.ApplyLayoutConfiguration(layoutConfiguration);

            uiLayoutConfigEditor = Configurator.CreateEditor(layoutConfiguration.GetType());
            uiLayoutConfigEditor.LoadConfiguration(Messageflow.LayoutConfiguration);
            uiLayoutConfigEditor.ConfigurationChanged += delegate {
                XDocument xNewConfiguration = uiLayoutConfigEditor.SaveConfiguration();
                bool isValid = true;
                try {
                    Configurator.LoadConfiguration(layoutConfiguration, xNewConfiguration);
                } catch {
                    isValid = false;
                }
                if (isValid) {
                    Messageflow.LayoutConfiguration = new SerializableXDocument(xNewConfiguration);
                    graphCanvas.ApplyLayoutConfiguration(layoutConfiguration);
                }
            };
            uiLayoutConfigurationContainer.Child = uiLayoutConfigEditor;
        }

        private ContextMenu CreateGraphCanvasContextMenu()
        {
            ContextMenu result = new ContextMenu();

            MenuItem menuItemAddActionNode = new MenuItem {
                Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/XRouter.Gui;component/Resources/Actions-tool-animator-icon.png")), Height = 20 },
                Header = new TextBlock { Text = "Action", FontSize = 14, FontWeight = FontWeights.Bold }
            };
            menuItemAddActionNode.Click += delegate {
                AddNode("Action", result, delegate {
                    ActionNodeConfiguration newActionNode = new ActionNodeConfiguration();
                    #region Add a default action
                    ActionType actionType = ConfigManager.Configuration.GetActionTypes().First();
                    ActionConfiguration action = new ActionConfiguration(actionType.Name) {
                        Configuration = new SerializableXDocument(new XDocument(new XElement(XName.Get("objectConfig"))))
                    };
                    newActionNode.Actions.Add(action);
                    #endregion
                    return newActionNode;
                });
            };

            MenuItem menuItemAddCbrNode = new MenuItem {
                Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/XRouter.Gui;component/Resources/nfs-unmount-icon.png")), Height = 20 },
                Header = new TextBlock { Text = "CBR", FontSize = 14, FontWeight = FontWeights.Bold }
            };
            menuItemAddCbrNode.Click += delegate {
                AddNode("CBR", result, delegate { return new CbrNodeConfiguration(); });
            };

            MenuItem menuItemAddTerminatorNode = new MenuItem {
                Icon = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/XRouter.Gui;component/Resources/Button-exit-icon.png")), Height = 20 },
                Header = new TextBlock { Text = "Terminator", FontSize = 14, FontWeight = FontWeights.Bold }
            };
            menuItemAddTerminatorNode.Click += delegate {
                Point menuLocationOnCanvas = result.TranslatePoint(new Point(), graphCanvas.Canvas);
                AddNode("Terminator", result, delegate { return new TerminatorNodeConfiguration(); });
            };

            MenuItem menuItemAdd = new MenuItem {
                Header = new TextBlock { Text = "Add node...", FontSize = 14 }  
            };
            menuItemAdd.Items.Add(menuItemAddActionNode);
            menuItemAdd.Items.Add(menuItemAddCbrNode);
            menuItemAdd.Items.Add(menuItemAddTerminatorNode);

            result.Items.Add(menuItemAdd);
            return result;
        }

        private void AddNode(string baseName, ContextMenu menu, Func<NodeConfiguration> nodeFactory)
        {
            NodeConfiguration node = nodeFactory();

            #region Set unique name
            int index = 1;
            string[] existingNames = Messageflow.Nodes.Select(n => n.Name).ToArray();
            while (existingNames.Contains(baseName + index.ToString())) {
                index++;
            }
            node.Name  = baseName + index.ToString();
            #endregion

            Point menuLocationOnCanvas = menu.TranslatePoint(new Point(), graphCanvas.Canvas);
            node.Location = menuLocationOnCanvas - graphCanvas.CanvasLocationOffset;
            Messageflow.Nodes.Add(node);
            MessageflowGraphPresenter.RaiseGraphChanged();

            ThreadUtils.InvokeLater(TimeSpan.FromSeconds(0.5), delegate {
                NodeSelectionManager.SelectNode(node);
            });
        }

        void IConfigurationControl.Save()
        {
            Messageflow.PromoteToNewVersion();
            ConfigManager.Configuration.UpdateMessageFlow(Messageflow);
        }

        private void uiImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Messageflow configuration|*.xmf";
            dialog.DefaultExt = ".xmf";
            dialog.CheckFileExists = true;
            if (dialog.ShowDialog() == true) {
                using (var fs = new FileStream(dialog.FileName, FileMode.Open)) {
                    Messageflow = MessageFlowConfiguration.Read(fs);
                }
                NodeSelectionManager = new NodeSelectionManager(uiNodePropertiesContainer, ConfigManager);
                uiNodePropertiesContainer.Child = null;
                MessageflowGraphPresenter = new MessageflowGraphPresenter(Messageflow, NodeSelectionManager);
                NodeSelectionManager.MessageflowGraphPresenter = MessageflowGraphPresenter;
                graphCanvas = MessageflowGraphPresenter.CreateGraphCanvas();
                uiDesignerContainer.Child = graphCanvas;

                PrepareLayoutConfiguration();
            }
        }

        private void uiExport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Messageflow configuration|*.xmf";
            dialog.DefaultExt = ".xmf";
            dialog.OverwritePrompt = true;
            if (dialog.ShowDialog() == true) {
                using (var fs = new FileStream(dialog.FileName, FileMode.Create)) {
                    Messageflow.Write(fs);
                }
            }
        }
    }
}
