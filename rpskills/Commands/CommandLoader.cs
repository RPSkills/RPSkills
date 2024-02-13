using System;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using rpskills.Utils;

namespace rpskills.Commands
{

/* commands.json
[
    {
        "Name": "ping",
        "Description": "ping pong",
        "Privilege": "root",
        "Handler": "ping"
    }
]
*/

    // TODO(chris): loading commands doesn't need an instance, but a command
    //              requires the context of an instance.
    public static class Loader
    {
        private static readonly ICoreAPI api;
        private static readonly Type type;
    }

    public class CommandLoader {

        private readonly ICoreAPI api;
        private readonly Type type;

        public CommandLoader(ICoreAPI api)
        {
            this.api = api;

            this.type = GetType();

            CreateCommand("ping");
        }

        private Option<MethodInfo> BindMethod(string name)
        {
            return BindMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private Option<MethodInfo> BindMethod(string name, BindingFlags flags)
        {
            MethodInfo info = null;

            try {
                info = type.GetMethod(
                    name,
                    flags,
                    new[] {typeof(TextCommandCallingArgs)}
                );
            } catch (AmbiguousMatchException e) {
                api.Logger.Error("[bc] Found more than one possible method binding for command: [c]");
                api.Logger.Error(e);
            } catch (ArgumentNullException e) {
                api.Logger.Error("[bc] Command [c] could not be made! Check important values related to rpskills.OriginSys.CommandLoader.");
                api.Logger.Error(e);
            } catch (Exception e) {
                api.Logger.Error("[bc] An unspecified exception occured!");
                api.Logger.Error(e);
            }

            return new Option<MethodInfo>(info);
        }

        private bool CreateCommand(string data)
        {
            // making OnCommandDelegate given a string
            Option<MethodInfo> method = BindMethod(data);
            if (!method.HasValue)
            {
                api.Logger.Error("[bc] Method could not be bound to command [c].");
                return false;
            }
            OnCommandDelegate commandDelegate = method.Unwrap().CreateDelegate<OnCommandDelegate>(this);

            // constructing the command
            IChatCommand cmd = api.ChatCommands.Create(data);
            cmd.RequiresPrivilege(Privilege.root);
            cmd.HandleWith(commandDelegate);

            return true;
        }

        TextCommandResult ping(TextCommandCallingArgs args)
        {
            return TextCommandResult.Success("pong");
        }
    }
}