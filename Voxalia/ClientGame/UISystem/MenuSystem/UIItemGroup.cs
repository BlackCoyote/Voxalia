//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ClientGame.UISystem.MenuSystem
{
    public class UIItemGroup : UIMenuItem
    {
        public List<UIMenuItem> MenuItems = new List<UIMenuItem>();

        public void Add(UIMenuItem item)
        {
            MenuItems.Add(item);
            Menus.Add(item);
        }

        public void Remove(UIMenuItem item)
        {
            MenuItems.Remove(item);
            Menus.MenuItems.Remove(item);
        }

        public void Clear()
        {
            for (int i = MenuItems.Count - 1; i >= 0; i--)
            {
                Remove(MenuItems[i]);
            }
        }

        public override bool Contains(int x, int y)
        {
            return false;
        }

        public override void Init()
        {
        }

        public override void MouseEnter()
        {
        }

        public override void MouseLeave()
        {
        }

        public override void MouseLeftDown()
        {
        }

        public override void MouseLeftUp()
        {
        }

        public override void Render(double delta, int xoff, int yoff)
        {
        }
    }
}
