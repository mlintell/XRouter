﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using XRouter.Common;

namespace XRouter.Gui
{
    /// <summary>
    /// Interaction logic for TokensViewControl.xaml
    /// </summary>
    public partial class TokensViewControl : UserControl
    {
        private static readonly int PageSize = 25;

        private ConfigurationManager ConfigManager { get; set; }

        private int currentPage = 1;

        public TokensViewControl()
        {
            InitializeComponent();
        }

        internal void Initialize(ConfigurationManager configManager)
        {
            ConfigManager = configManager;

            uiSelectedTokenHeader.Visibility = Visibility.Collapsed;
            uiSelectedToken.Visibility = Visibility.Collapsed;

            // Remove column ContentWithEnters
            uiTokens.AutoGeneratedColumns += delegate {
                uiTokens.Columns.RemoveAt(uiTokens.Columns.Count - 1);
            };
        }

        public void UpdateTokens()
        {
            currentPage = 1;
            LoadTokens();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (uiTokens.Items.Count == 0) {
                return;
            }
            currentPage++;
            LoadTokens();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage == 1) {
                return;
            }
            currentPage--;
            LoadTokens();
        }

        private void LoadTokens()
        {
            Token[] tokens = ConfigManager.ConsoleServer.GetTokens(PageSize, currentPage);
            TokenRow[] rows = tokens.Select(t => new TokenRow(t)).ToArray();
            uiTokens.ItemsSource = rows;
            uiPageNumber.Text = currentPage.ToString();
        }

        private class TokenRow
        {
            public string Guid { get; private set; }

            public string State { get; private set; }

            public string Content { get; private set; }

            public string ContentWithEnters { get; private set; }

            public TokenRow(Token token)
            {
                Guid = token.Guid.ToString();
                State = token.State.ToString();
                ContentWithEnters = token.Content.XDocument.ToString();
                Content = ContentWithEnters.Replace(Environment.NewLine, " ");
            }
        }

        private void uiTokens_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TokenRow selectedRow = uiTokens.SelectedItem as TokenRow;
            if (selectedRow != null) {
                uiSelectedTokenHeader.Visibility = Visibility.Visible;
                uiSelectedToken.Visibility = Visibility.Visible;
                uiSelectedToken.Text = selectedRow.ContentWithEnters;
            } else {
                uiSelectedTokenHeader.Visibility = Visibility.Collapsed;
                uiSelectedToken.Visibility = Visibility.Collapsed;
                uiSelectedToken.Text = string.Empty;
            }
        }
    }
}

// This prevents strange compilation error with missing namespace
namespace Microsoft.Windows.Controls.Primitives
{
}