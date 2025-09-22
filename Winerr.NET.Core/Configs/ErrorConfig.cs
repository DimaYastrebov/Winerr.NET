using System.Collections.Generic;
using Winerr.NET.Core.Enums;

namespace Winerr.NET.Core.Configs
{
    public class ErrorConfig
    {
        public SystemStyle SystemStyle { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int IconId { get; set; }

        public List<ButtonConfig> Buttons { get; set; }

        public int? MaxWidth { get; set; }

        public ButtonAlignment ButtonAlignment { get; set; }

        public bool IsCrossEnabled { get; set; }

        public bool SortButtons { get; set; }

        public ErrorConfig()
        {
            SystemStyle = SystemStyle.Windows7Aero;
            Title = "";
            Content = "";
            IconId = 0;
            Buttons = new List<ButtonConfig>();
            MaxWidth = null;
            ButtonAlignment = ButtonAlignment.Right;
            IsCrossEnabled = true;
        }
    }
}
