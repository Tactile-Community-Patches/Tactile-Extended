using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tactile.Graphics.Help;
using Tactile.Windows.UserInterface;
using Tactile.Windows.UserInterface.Title;

namespace Tactile.Menus.Title
{
    class GrowthModeSelectionMenu : BaseMenu
    {
        public Vector2 MenuLoc { get; private set; }
        private bool MenusHidden = false;
        private Growth_Mode_Info_Panel[] GrowthModePanels;
        private UINodeSet<Growth_Mode_Info_Panel> GrowthModeNodes;

        private Button_Description CancelButton;

        public override bool HidesParent { get { return false; } }

        public GrowthModeSelectionMenu()
        {
            GrowthModePanels = new Growth_Mode_Info_Panel[Enum_Values.GetEnumCount(typeof(Growth_Modes))];
            int offset = 0;
            for (int i = 0; i < GrowthModePanels.Length; i++)
            {
                GrowthModePanels[i] = new Growth_Mode_Info_Panel((Growth_Modes)i);
                GrowthModePanels[i].stereoscopic = Config.TITLE_MENU_DEPTH;
                GrowthModePanels[i].active = false;
                GrowthModePanels[i].loc = new Vector2(0, offset);
                offset += GrowthModePanels[i].height + 8;
            }

            MenuLoc = new Vector2(
                (Config.WINDOW_WIDTH - Growth_Mode_Info_Panel.WIDTH) / 2,
                (Config.WINDOW_HEIGHT - 16) / 2);
            MenuLoc -= new Vector2(0, (offset / 2) / 8 * 8);

            GrowthModeNodes = new UINodeSet<Growth_Mode_Info_Panel>(GrowthModePanels);
            GrowthModeNodes.set_active_node(GrowthModeNodes[(int)Growth_Modes.Random]);
            GrowthModeNodes.ActiveNode.active = true;

            CancelButton = Button_Description.button(Inputs.B,
                Config.WINDOW_WIDTH - 64);
            CancelButton.description = "Cancel";
            CancelButton.stereoscopic = Config.TITLE_MENU_DEPTH;
        }

        public Growth_Modes SelectedGrowthMode
        {
            get { return (Growth_Modes)GrowthModeNodes.ActiveNodeIndex; }
        }

        #region Events
        public event EventHandler<EventArgs> Selected;
        protected void OnSelected(EventArgs e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled(EventArgs e)
        {
            if (Canceled != null)
                Canceled(this, e);
        }
        #endregion

        protected override void Activate()
        {
            MenusHidden = false;
        }

        public void HideMenus()
        {
            MenusHidden = true;
        }

        protected override void UpdateMenu(bool active)
        {
            int index = GrowthModeNodes.ActiveNodeIndex;
            GrowthModeNodes.Update(active, -MenuLoc);
            if (index != GrowthModeNodes.ActiveNodeIndex)
            {
                GrowthModePanels[index].active = false;
                GrowthModeNodes.ActiveNode.active = true;
            }

            CancelButton.Update(active);

            if (active)
            {
                var styleIndex = GrowthModeNodes.consume_triggered(
                    Inputs.A, MouseButtons.Left, TouchGestures.Tap);
                if (styleIndex.IsSomething)
                {
                    Global.game_system.play_se(System_Sounds.Confirm);
                    GrowthModeNodes.set_active_node(GrowthModeNodes[styleIndex.Index]);
                    OnSelected(new EventArgs());
                }
                else if (Global.Input.triggered(Inputs.B) ||
                    Global.Input.KeyPressed(Keys.Escape) ||
                    CancelButton.consume_trigger(MouseButtons.Left) ||
                    CancelButton.consume_trigger(TouchGestures.Tap))
                {
                    Global.game_system.play_se(System_Sounds.Cancel);
                    OnCanceled(new EventArgs());
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!MenusHidden)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                GrowthModeNodes.Draw(spriteBatch, -MenuLoc);
                CancelButton.Draw(spriteBatch);
                spriteBatch.End();
            }
        }
    }
}
