﻿#pragma checksum "C:\Users\User\Documents\GitHub\HFR8\HFR7\ListTopics.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "5B501ECF6769AE5E3185129B1204A48D"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace HFR7 {
    
    
    public partial class ListTopics : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal Microsoft.Phone.Controls.PhoneApplicationPage ListTopicPage;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Media.ImageBrush backgroundImageBrush;
        
        internal Microsoft.Phone.Controls.Pivot topicsPivot;
        
        internal Microsoft.Phone.Controls.LongListSelector topicsList;
        
        internal Microsoft.Phone.Controls.LongListSelector drapList;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbar_refresh;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbar_lastpage;
        
        internal Microsoft.Phone.Shell.ApplicationBarIconButton appbar_nextpage;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/HFR7;component/ListTopics.xaml", System.UriKind.Relative));
            this.ListTopicPage = ((Microsoft.Phone.Controls.PhoneApplicationPage)(this.FindName("ListTopicPage")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.backgroundImageBrush = ((System.Windows.Media.ImageBrush)(this.FindName("backgroundImageBrush")));
            this.topicsPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("topicsPivot")));
            this.topicsList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("topicsList")));
            this.drapList = ((Microsoft.Phone.Controls.LongListSelector)(this.FindName("drapList")));
            this.appbar_refresh = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbar_refresh")));
            this.appbar_lastpage = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbar_lastpage")));
            this.appbar_nextpage = ((Microsoft.Phone.Shell.ApplicationBarIconButton)(this.FindName("appbar_nextpage")));
        }
    }
}

