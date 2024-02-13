

using ProtoBuf;

namespace rpskills.OriginSys
{
    
    class OriginSelectedState
    {
        public bool DidSelect;
    }

    /// <summary>
    /// Contains all data regarding Origin selection.
    /// 
    /// See Vintagestory.GameContent.CharacterSelectionPacket for more details.
    /// </summary>
    // TODO(chris): What is this? I found it on CharacterSelectionPacket, and
    //              a constructor was implicitly defined. For now, I'll
    //              explicitly define.
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class OriginSelectionPacket
    {
        public bool DidSelect;
        public string OriginName;

    }
}