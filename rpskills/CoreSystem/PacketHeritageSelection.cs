

using ProtoBuf;

namespace rpskills.CoreSys
{
    
    class HeritageSelectedState
    {
        public bool DidSelect;
    }

    /// <summary>
    /// Contains all data regarding Heritage selection.
    /// 
    /// See Vintagestory.GameContent.CharacterSelectionPacket for more details.
    /// </summary>
    // TODO(chris): What is this? I found it on CharacterSelectionPacket, and
    //              a constructor was implicitly defined. For now, I'll
    //              explicitly define.
    [ProtoContract]
    public class HeritageSelectionPacket
    {
        public bool DidSelect;
        public string HeritageName;

    }
}