using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using rpskills.OriginSys;
using rpskills.Commands;
using System.Linq;
using System.Text;

// TODO(chris): delete all FEAT(chris) annotations before merging
// NOTE(chris): all current WARN(chris) in this file indicates client-server
//              interactions. They depend on the network channel feature, which
//              must be created and debugged first.

namespace rpskills
{
    /// <summary>
    /// ModSystem is the base for any VintageStory code mods.
    /// </summary>
    public class OriginSystem : ModSystem
    {

        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::Constant::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        const string MOD_NAME = "rpskills";
        const string CHANNEL_CORE_RPSKILLS = "rpskills-core";
        const string CFG_ORIGIN = "chooseOrigin";


        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Shared:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        // FEAT(chris): need the lists
        List<Path> Paths;
        Dictionary<string, Path> PathsByName;
        List<Skill> Skills;
        Dictionary<string, Skill> SkillsByName;
        List<Origin> Origins;
        Dictionary<string, Origin> OriginsByName;



        bool didSelect;

        /// <summary>
        /// Utility for accessing common client/server functionality.
        /// </summary>
        ICoreAPI api;

        public override void Start(ICoreAPI api)
        {
            this.api = api;

            api.Network
                .RegisterChannel(CHANNEL_CORE_RPSKILLS)
                .RegisterMessageType<OriginSelectionPacket>()
                .RegisterMessageType<OriginSelectedState>();

        }

        private void loadCharacterOrigin()
        {
            this.Paths = this.api.Assets
                .Get<List<Path>>(AssetLocation.Create("rpskills:config/paths.json", "rpskills"));
            api.Logger.Debug("Loaded paths");
            PathsByName = new Dictionary<string, Path>();
            foreach (Path path in this.Paths)
            {
                this.PathsByName[path.Name] = path;
            }

            this.Skills = this.api.Assets
                .Get<List<Skill>>(AssetLocation.Create("rpskills:config/skills.json", "rpskills"));
            SkillsByName = new Dictionary<string, Skill>();
            foreach (Skill skill in this.Skills)
            {
                this.SkillsByName[skill.Name] = skill;
            }
            api.Logger.Debug("loaded skills");

            this.Origins = this.api.Assets
                .Get<List<Origin>>(AssetLocation.Create("rpskills:config/origins.json", "rpskills"));
            OriginsByName = new Dictionary<string, Origin>();
            foreach (Origin origin in this.Origins)
            {
                this.OriginsByName[origin.Name] = origin;
            }
            api.Logger.Debug("loaded origins");

            foreach (Skill skill in this.Skills)
            {
                api.Logger.Debug(skill.ToString());
            }


            this.api.Logger.Event("Origins System loaded!");
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Client:::::::::::::
        ::::::::::::::::::::::::::::::::
         */


        ICoreClientAPI capi;

        // TODO(chris): this was important for the Gui code
        GuiDialog charDlg;




        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            // tell client how to handle server sending origin information
            api.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SetMessageHandler<OriginSelectedState>(
                    new NetworkServerMessageHandler<OriginSelectedState>(
                        this.onSelectedState
                ));

            // WARN(chris): uncommenting may suck
            api.Event.IsPlayerReady += this.Event_IsPlayerReady;
            api.Event.PlayerJoin += this.Event_PlayerJoin;

            // FEAT(chris): primary functionality of the branch
            api.Event.BlockTexturesLoaded += this.loadCharacterOrigin;

            // NOTE(chris): the SendPlayerNowReady call is in the
            //              GuiDialogCharacterBase.OnGuiClose override for the
            //              CharacterSystem. The GuiDialog child is located
            //              at this point in the StartClientSide. See below:
            // this.charDlg = api.Gui.LoadedGuis
            //     .Find((GuiDialog dlg) => dlg is GuiDialogCharacterBase)
            //     as GuiDialogCharacterBase;


            // create client commands
            IChatCommand get = api.ChatCommands.Create("get");
            get.RequiresPlayer();
            get.RequiresPrivilege(Privilege.root);
            get.WithDescription("Read WatchedAttributes of the caller.");
            get.HandleWith(args => {
                string cmdargs = args.RawArgs.PopAll();
                string result = "given " + cmdargs + "\n";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (var attr in eplr.WatchedAttributes)
                {
                    result += attr.Key;
                    result += "\n";
                }

                return TextCommandResult.Success(result);
            });

            IChatCommand get_skill = get.BeginSubCommand("skill");
            get_skill.RequiresPrivilege(Privilege.root);
            get_skill.WithDescription("Read Origin Skills of the caller.");
            get_skill.HandleWith(args => {
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach(var attr in eplr.WatchedAttributes)
                {
                    if (!attr.Key.StartsWith("s_"))
                    {
                        continue;
                    }

                    result += attr.Key + ": " + attr.Value.ToString() + "\n";

                }

                return TextCommandResult.Success(result);
            });
            get_skill.EndSubCommand();


            get.Validate(); // name, priv, desc, handler




            IChatCommand set = api.ChatCommands.Create("set");
            set.RequiresPlayer();
            // set.WithArgs( populate with Skills )
            set.RequiresPrivilege("root");
            set.WithDescription("Resets Origin Skills of the caller.");
            set.HandleWith(args => {
                float new_val = 0f;
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (Skill skill in Skills)
                {
                    eplr.WatchedAttributes.SetFloat("s_" + skill.Name, new_val);
                }

                return TextCommandResult.Success(result);
            });

            IChatCommand set_skill = set.BeginSubCommand("skill");
            set_skill.RequiresPrivilege(Privilege.root);
            set_skill.WithDescription("Sets the given skill of the caller to a given value.");
            set_skill.HandleWith(args => {
                float new_val = 4f;
                string skill = "s_woodsman";
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                result += "set " + skill + " to lv " + new_val;
                eplr.WatchedAttributes.SetFloat(skill, new_val);

                return TextCommandResult.Success(result);
            });
            set_skill.EndSubCommand();


            set.Validate(); // name, priv, desc, handler



            IChatCommand del = api.ChatCommands.Create("del");
            del.RequiresPrivilege(Privilege.root);
            del.WithDescription("Deletes all Origin Skills from the caller's player data.");
            del.HandleWith(args => {
                string result = "";
                EntityPlayer eplr = args.Caller.Player.WorldData.EntityPlayer;

                foreach (Skill skill in Skills)
                {
                    eplr.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                }

                return TextCommandResult.Success(result);
            });


            del.Validate(); // name, priv, desc, handler
        }


        private void onSelectedState(OriginSelectedState s)
        {
            this.api.Logger.Debug("Recieved status of heriatge selection: " + s.DidSelect);
            this.didSelect = s.DidSelect;
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Server:::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        ICoreServerAPI sapi;


        public override void StartServerSide(ICoreServerAPI api)
        {
            this.sapi = api;

            // NOTE(chris): this big block tells the server how to reply to
            //              incoming packets with respect to origin selection
            api.Network.GetChannel(CHANNEL_CORE_RPSKILLS)
                .SetMessageHandler<OriginSelectionPacket>(
                    new NetworkClientMessageHandler<OriginSelectionPacket>(
                        this.onOriginSelection
                    )
                );

            // NOTE(chris): tells the server what to do when a player connects
            api.Event.PlayerJoin += this.Event_PlayerJoinServer;

            // FEAT(chris): primary functionality of the branch
            api.Event.ServerRunPhase(
                EnumServerRunPhase.ModsAndConfigReady,
                new Action(this.loadCharacterOrigin)
            );


            // Register commands

            IChatCommand cmd = api.ChatCommands.Create("foo");
            cmd.RequiresPrivilege(Privilege.root);
            cmd.HandleWith((a) => {
                return TextCommandResult.Success("foo was completed!");
            });

        }

        /// <summary>
        /// how the server handles a player selecting a origin. this is the
        /// wrapper for character origin 'setter'
        /// </summary>
        /// <param name="fromPlayer">packet-emitting client</param>
        /// <param name="packet">origin selection data</param>
        /// <exception cref="NotImplementedException">You Should Not See This in dev</exception>
        private void onOriginSelection(IServerPlayer fromPlayer, OriginSelectionPacket packet)
        {
            // TODO(chris): remove negation after commands are working!!!
            bool didSelectBefore = SerializerUtil.Deserialize<bool>(
                fromPlayer.GetModdata(CFG_ORIGIN), false
            );

            if (didSelectBefore) {
                api.Logger.Debug("you've already chosen an origin");
                return;
            }

            api.Logger.Debug("successfully chosen " + packet.OriginName);

            if(packet.DidSelect) {
                fromPlayer.SetModdata(
                    CFG_ORIGIN,
                    SerializerUtil.Serialize<bool>(packet.DidSelect)
                );

                // NOTE(chris): the following list is pulled from
                //              CharacterSystem.onCharacterSelection

                // TODO(chris): use player.WatchedAttributes.SetString to store
                //              the origin name (setCharacterClass)

                foreach(Skill skill in Skills)
                {
                    fromPlayer.WorldData.EntityPlayer.WatchedAttributes.RemoveAttribute("s_" + skill.Name);
                    api.Logger.Debug("Setting " + skill.Name + "@" + skill.Level);
                    fromPlayer.WorldData.EntityPlayer.WatchedAttributes.SetFloat("s_" + skill.Name, skill.Level);
                }

                // TODO(chris): next, attributes are to be applied
                //              (applyTraitAttributes)

                // TODO(chris): change entity behavior using
                //              fromPlayer.Entity.GetBehavior<T>()


            }
            // TODO(chris): mark all changed WatchedAttributes as dirty

            fromPlayer.BroadcastPlayerData(true);
        }



        /*
        ::::::::::::::::::::::::::::::::
        :::::::::::::Events:::::::::::::
        ::::::::::::::::::::::::::::::::
         */



        /// <summary>
        /// here, we want to make sure all of the player origin data is
        /// selected and valid. If handling is set to `PreventDefault`, then
        /// the system must eventually call `Network.SendPlayerNowReady()`!
        /// </summary>
        /// <param name="handling">server's understanding of client readiness</param>
        /// <returns></returns>
        private bool Event_IsPlayerReady(ref EnumHandling handling)
        {
            if (this.didSelect)
            {
                return true;
            }
            // WARN(chris): IClientNetworkAPI will now expects a
            //              `SendPlayerNowReady` call before it will let a
            //              player join a world!!!
            handling = EnumHandling.PreventDefault;
            return false;
        }

        private void Event_PlayerJoin(IClientPlayer byPlayer)
        {
            if (this.didSelect && byPlayer.PlayerUID == this.capi.World.Player.PlayerUID)
            {
                return;
            }
            // TODO(chris): not my place...
            // CharacterSystem.Event_PlayerJoin for reference. Looks like
            // it sets up GUI stuff. The game is paused when the Guis are
            // made, and Action GuiDialogue.OnClose gets an anonomys
            // delegate to unpause the game. Below is the boilerplate:

            Action guiStuff_OnClose = delegate
            {
                this.capi.PauseGame(false);
            };

            this.capi.Event.EnqueueMainThreadTask(delegate
            {
                this.capi.PauseGame(true);
            }, "pausegame");

            // TODO(chris): please hide the following in the Gui code. The
            //              remaining code in this function shouldn't be here!!!

            // NOTE(chris): these next two lines should stay directly next to
            //              eachother in the Gui code, immediately after client
            //              selection is done.
            // WARN(chris): these are default values, use the Gui to get
            //              player-chosen values to put here.
            // tell the server what the player selected
            didSelect = true;
            OriginSelectionPacket p = new OriginSelectionPacket {
                DidSelect = didSelect,
                OriginName = this.OriginsByName["average"].Name,
            };
            capi.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SendPacket<OriginSelectionPacket>
                (
                    p
                );
            capi.Network.SendPlayerNowReady();

            guiStuff_OnClose.Invoke();
        }

        /// <summary>
        /// sends the state of the character with respect to rpskills to the
        /// client, over Network:RPSKILLS_CORE_CHANNEL.
        /// </summary>
        /// <param name="byPlayer">the joining player</param>
        public void Event_PlayerJoinServer(IServerPlayer byPlayer)
        {
            // WARN(chris): we are using Moddata(createCharacter) as a
            //              placeholder for now -- functionality is tied to
            //              VintageStory.GameContent.CharacterSystem
            // this.didSelect = SerializerUtil.Deserialize<bool>(
            //     byPlayer.GetModdata("createCharacter"), false
            // );
            this.didSelect = false;
            if (!this.didSelect) {
                api.Logger.Debug("Character creation has not happened yet.");
            } else {
                api.Logger.Debug("Character creation has happened!");
            }

            this.sapi.Network
                .GetChannel(CHANNEL_CORE_RPSKILLS)
                .SendPacket<OriginSelectedState>(
                    new OriginSelectedState
                    {
                        DidSelect = this.didSelect
                    },
                    new IServerPlayer[]
                    {
                        byPlayer
                    }
                );
            api.Logger.Debug("Package sent indicating selection status");
        }



        /*
        ::::::::::::::::::::::::::::::::
        ::::::::::::::Tidy::::::::::::::
        ::::::::::::::::::::::::::::::::
         */

        public override void Dispose()
        {
            base.Dispose();
            // harmony.UnpatchAll(MOD_NAME);
        }

    }
}
