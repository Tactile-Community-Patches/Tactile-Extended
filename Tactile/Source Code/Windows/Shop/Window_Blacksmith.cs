using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tactile.Graphics.Help;
using Tactile.Graphics.Text;
using Tactile.Map;
using Tactile.Windows.Command;
using TactileLibrary;

namespace Tactile
{
    enum Blacksmith_Messages { Intro, Question, Confirm, Cancel, Leave, Anything_Else, Confirm_Not_Enough, Confirm_Cant_Fix, Confirm_Full }
    class Window_Blacksmith : Window_Business
    {
        readonly static Blacksmith_Messages[] WAIT_TEXT = {
            Blacksmith_Messages.Intro, Blacksmith_Messages.Question, Blacksmith_Messages.Leave, Blacksmith_Messages.Cancel,
            Blacksmith_Messages.Confirm_Full, Blacksmith_Messages.Confirm_Not_Enough, Blacksmith_Messages.Confirm_Cant_Fix,
            Blacksmith_Messages.Confirm, Blacksmith_Messages.Cancel, Blacksmith_Messages.Leave };

        private int Actor_Id;
        private int Buy_Price_Mod;

        private Window_Command_Shop Window;
        protected Button_Description CancelButton;

        #region Accessors
        protected Game_Actor actor
        {
            get
            {
                if (!Global.game_actors.ContainsKey(Actor_Id))
                    return null;
                return Global.game_actors[Actor_Id];
            }
        }

        protected override bool trading
        {
            get { return base.trading; }
            set
            {
                base.trading = value;
                if (!this.trading)
                    Window.active = false;
            }
        }

        protected int row_max { get { return On_Buy ? Shop.items.Count : actor.num_items; } }
        #endregion

        public Window_Blacksmith() { }
        public Window_Blacksmith(int actor_id, Shop_Data shop)
        {
            Actor_Id = actor_id;
            Shop = shop;
            if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                Global.Audio.BgmFadeOut(60);
            Buy_Price_Mod = determine_buy_price_mod();
            initialize_images();
        }

        private int determine_buy_price_mod()
        {
            if (this.actor == null)
                return 1;
            return actor.buy_price_mod();
        }

        protected override int choice_offset()
        {
            switch ((Blacksmith_Messages)Message_Id)
            {
                case Blacksmith_Messages.Anything_Else:
                    return Shop.offsets[1];
                case Blacksmith_Messages.Confirm:
                    return Shop.offsets[2];
                default:
                    return Shop.offsets[0];
            }
        }

        #region Image Setup
        protected override void initialize_images()
        {
            Item_Rect = new Rectangle(40, 88, 248, 16 * ROWS);

            // Black Screen
            Black_Screen = new Sprite();
            Black_Screen.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Black_Screen.dest_rect = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
            Black_Screen.tint = new Color(0, 0, 0, 0);
            // Background
            Menu_Background background = new Menu_Background();
            background.texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/Status_Background");
            background.vel = new Vector2(-0.25f, 0);
            background.tile = new Vector2(3, 2);
            Background = background;
            // Darkened Bar
            Darkened_Bar = new Sprite();
            Darkened_Bar.texture = Global.Content.Load<Texture2D>(@"Graphics/White_Square");
            Darkened_Bar.dest_rect = new Rectangle(0, 8, Config.WINDOW_WIDTH, 48);
            Darkened_Bar.tint = new Color(0, 0, 0, 128);

            // Data
            refresh_repair();
            // Portrait BG
            Portrait_Bg = new Sprite();
            if (Global.game_system.preparations && string.IsNullOrEmpty(Shop.face))
            {
                Portrait_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Preparations_Screen");
                Portrait_Bg.src_rect = new Rectangle(136, 136, 48, 64);
                Portrait_Bg.loc = new Vector2(16, 0);

                Portrait_Label = new TextSprite();
                Portrait_Label.loc = Portrait_Bg.loc + new Vector2(8, 40);
                Portrait_Label.SetFont(Config.UI_FONT, Global.Content, "White");
                Portrait_Label.text = "Convoy";
            }
            else
            {
                Portrait_Bg.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Shop_Portrait_bg");
                Portrait_Bg.src_rect = new Rectangle(0, 0, 57, 57);
                Portrait_Bg.loc = new Vector2(12, 4);
            }
            // Gold Window
            Gold_Window = new Sprite();
            Gold_Window.texture = Global.Content.Load<Texture2D>(@"Graphics/Windowskins/Gold_Window");
            Gold_Window.loc = new Vector2(212, 48);
            // Gold_Data
            Gold_Data = new RightAdjustedText();
            Gold_Data.loc = new Vector2(216 + 48, 48);
            Gold_Data.SetFont(Config.UI_FONT, Global.Content, "Blue");
            redraw_gold();
            Gold_G = new TextSprite();
            Gold_G.loc = new Vector2(216 + 48, 48);
            Gold_G.SetFont(Config.UI_FONT + "L", Global.Content, "Yellow", Config.UI_FONT);
            Gold_G.text = "G";
            // Text
            Message = new Message_Box(64, 8, 160, 2, false, "White");
            set_text(Blacksmith_Messages.Intro);
            Message_Active = true;

            create_cancel_button();

            set_images();
        }

        protected void create_cancel_button()
        {
            CancelButton = Button_Description.button(Inputs.B,
                new Vector2(Config.WINDOW_WIDTH - 56, 8));
            CancelButton.description = "Cancel";
        }

        protected override bool is_wait_text(int message_id)
        {
            return WAIT_TEXT.Contains((Blacksmith_Messages)message_id);
        }

        private void set_text(Blacksmith_Messages message_id)
        {
            set_text((int)message_id);
        }

        private void refresh_repair(bool resetIndex = false)
        {
            /* //Debug
            Item_Data.Clear();
            int count = actor.num_items;
            for (int i = 0; i < count; i++)
            {
                Item_Data.Add(new Shop_Sell_Item());
                Item_Data[i].set_image(actor, actor.items[i], -1, item_cost(i));
                Item_Data[i].loc = Window.loc + new Vector2(8, 28 + i * 16);
            }*/

            if (resetIndex)
                Window = null;
            Window = new Window_Command_Shop(
                Window,
                new Vector2(Item_Rect.X - 8, Item_Rect.Y - 28),
                Item_Rect.Width + 16, ROWS,
                false, Actor_Id, Buy_Price_Mod,
                Enumerable.Range(0, actor.num_items)
                    .Select(i => this.actor.items[i])
                    .ToList());
            Window.active = Trading;
        }
        #endregion

        #region Processing
        protected override int item_cost()
        {
            return item_cost(Window.index);
        }
        private int item_cost(int index)
        {
            int price = 0;
            Item_Data item_data = this.item_data(index);
            return item_data.item_cost(On_Buy, Buy_Price_Mod);
        }

        private Item_Data item_data(int index)
        {
            {
                if (index == -1 || index >= actor.num_items)
                    return new Item_Data();
                return actor.items[index];
            }
        }

        private bool can_repair
        {
            get
            {
                    return actor.items[Window.index].Uses != actor.items[Window.index].max_uses;
            }
        }

        private bool can_fix { get { return actor.num_items > 0; } }
        #endregion

        protected override void play_shop_theme()
        {
            if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                Global.Audio.PlayBgm(Shop.song, forceRestart: true);
        }

        #region Update
        protected override void UpdateMenu(bool active)
        {
            update_black_screen();
            if (Delay > 0)
                Delay--; // Does this do anything //Yeti
            Background.update();
            bool input = !Closing && Delay == 0 && Black_Screen_Timer == 0;

            Window.update(input && Trading && !Accepting && Message.text_end);
            CancelButton.Update(input && Message.text_end);
            if (Choices != null)
            {
                Choices.Update(input && Message.text_end);
                update_cursor_location();
            }
            if (input)
            {
                if (Message_Active)
                    update_message();
                if (!Closing && Message.text_end)
                {
                    if (Trading && !Accepting && !Window.active)
                        Window.active = true;

                    else if (!Trading)
                        update_main_selection();
                    else
                        update_sending();
                }
            }

            Face.update();
        }

        protected override void UpdateAncillary()
        {
            if (Input.ControlSchemeSwitched)
                create_cancel_button();
        }

        protected override void update_message()
        {
            Message.update();
            if (Message.text_end && !Message.wait)
            {
                switch ((Blacksmith_Messages)Message_Id)
                {
                    case Blacksmith_Messages.Intro:
                        set_text(Blacksmith_Messages.Question);
                        break;
                    case Blacksmith_Messages.Confirm:
                        Message_Active = false;
                        set_choices(choice_offset(), "Yes", "No");
                        break;
                    case Blacksmith_Messages.Anything_Else:
                    case Blacksmith_Messages.Confirm_Not_Enough:
                        set_text(Blacksmith_Messages.Confirm);
                        break;
                    case Blacksmith_Messages.Confirm_Cant_Fix:
                        set_text(Blacksmith_Messages.Confirm_Cant_Fix);
                        break;
                    case Blacksmith_Messages.Confirm_Full:
                        set_text(Blacksmith_Messages.Confirm_Full);
                        break;
                    case Blacksmith_Messages.Cancel:
                        if (this.actor == null)
                        {
                            Message_Active = true;
                            Cursor.visible = false;
                            set_text(Blacksmith_Messages.Leave);
                            Message.finished = true;
                        }
                        else
                        {
                            this.trading = false;
                            set_text(Blacksmith_Messages.Anything_Else);
                        }
                        break;
                    case Blacksmith_Messages.Leave:
                        close();
                        Black_Screen_Timer = BLACK_SCREEN_HOLD_TIMER + (BLACK_SCREEN_FADE_TIMER * 2);
                        if (!string.IsNullOrEmpty(Shop.song) || !Global.game_system.preparations)
                            Global.Audio.BgmFadeOut(60);
                        break;
                }
            }
        }

        protected override void update_main_selection()
        {
            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            bool cancel = Global.Input.triggered(Inputs.B) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                Message_Active = true;
                if (can_fix)
                {
                    set_text(Blacksmith_Messages.Confirm);
                    this.trading = true;
                }
                else
                    set_text(Blacksmith_Messages.Cancel);
                clear_choices();
            }
            else if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Message_Active = true;
                set_text(Traded ? Blacksmith_Messages.Leave : Blacksmith_Messages.Cancel);
                Message.finished = true;
                clear_choices();
            }
        }

        private void update_trading()
        {
            bool cancel =
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap);

            if (Window.getting_help())
            {
                Window.open_help();
            }
            else if (Window.is_help_active &&
                (Window.is_canceled() || cancel))
            {
                Window.close_help();
            }
            else if (Window.is_canceled() ||
                (this.actor == null && Window.is_selected()) ||
                Global.Input.mouse_click(MouseButtons.Right) ||
                cancel) //@Yeti
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                Message_Active = true;
                this.trading = false;
                if (this.actor == null)
                {
                    set_text(Blacksmith_Messages.Leave);
                    Message.finished = true;
                    clear_choices();
                }
                else
                {
                    set_text(Blacksmith_Messages.Anything_Else);
                }
            }
            else if (Window.is_selected())
            {
                int index = Window.selected_index().Index;
                Window.index = index;

                if (Window.is_help_active)
                    Window.close_help();

                Message_Active = true;
                Window.active = false;
            }
        }

        private void update_accepting()
        {
            On_Yes = Choices.ActiveNodeIndex == 0;

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            bool cancel = Global.Input.triggered(Inputs.B) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                if (!On_Yes)
                    cancel_accepting();
                else
                {
                    int index = Window.index;

                    Message_Active = true;
                    {
                        Accepting = false;
                        Traded = true;
                            fix_item();
                        Global.Audio.play_se("System Sounds", "Gold_Change");
                    }
                }
                clear_choices();
            }
            else if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_accepting();
                clear_choices();
            }
        }

        private void update_sending()
        {
            On_Yes = Choices.ActiveNodeIndex == 0;

            var selected = Choices.consume_triggered(
                Inputs.A, MouseButtons.Left, TouchGestures.Tap);
            bool cancel = Global.Input.triggered(Inputs.B) ||
                CancelButton.consume_trigger(MouseButtons.Left) ||
                CancelButton.consume_trigger(TouchGestures.Tap);

            if (selected.IsSomething)
            {
                Global.game_system.play_se(System_Sounds.Confirm);
                if (!On_Yes)
                {
                    cancel_accepting(Blacksmith_Messages.Cancel);
                    this.trading = false;
                }
                else
                {
                    accept_repair();
                }
                clear_choices();
            }
            else if (cancel)
            {
                Global.game_system.play_se(System_Sounds.Cancel);
                cancel_accepting(Blacksmith_Messages.Cancel);
                this.trading = false;
                clear_choices();
            }
        }

        private void accept_repair()
        {
                Accepting = false;
                Message_Active = true;
                Traded = true;
                fix_item();
                Global.Audio.play_se("System Sounds", "Gold_Change");
        }
        #endregion

        #region Shop Processing
        private void fix_item()
        {
            int index = Window.index;

            // Change gold
            Global.battalion.gold -= item_cost();
            redraw_gold();
            // Repairs Item
            actor.items[Window.index].Uses = actor.items[Window.index].max_uses;
            // Redraw text and images
            set_text(Blacksmith_Messages.Anything_Else);
            this.trading = false;
        }

        private void cancel_accepting()
        {
            Accepting = false;
            Message_Active = true;
            set_text(Blacksmith_Messages.Cancel);
        }
        private void cancel_accepting(Blacksmith_Messages message)
        {
            Accepting = false;
            Message_Active = true;
            set_text(Blacksmith_Messages.Cancel);
        }
        #endregion

        protected override void draw_background(SpriteBatch sprite_batch)
        {
            sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            Background.draw(sprite_batch);
            sprite_batch.End();
        }

        protected override void draw_window(SpriteBatch sprite_batch)
        {
            Window.draw(sprite_batch);

            if (!Closing &&
                (Blacksmith_Messages)Message_Id != Blacksmith_Messages.Leave &&
                (Blacksmith_Messages)Message_Id != Blacksmith_Messages.Cancel)
            {
                sprite_batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                CancelButton.Draw(sprite_batch);
                sprite_batch.End();
            }
        }
    }
}
