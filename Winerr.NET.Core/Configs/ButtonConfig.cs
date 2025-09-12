using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Winerr.NET.Core.Enums;

namespace Winerr.NET.Core.Configs
{
    public class ButtonConfig
    {
        public string Text { get; set; }
        public ButtonType Type { get; set; }
        public TextRenderConfig? TextConfig { get; set; }

        public ButtonConfig()
        {
            Text = "";
            Type = ButtonType.Default;
            TextConfig = new TextRenderConfig();
        }
    }
}
