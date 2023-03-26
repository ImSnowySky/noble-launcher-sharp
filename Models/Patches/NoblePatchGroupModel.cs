using System.Collections.Generic;

namespace NobleLauncher.Models
{
    public class NoblePatchGroupModel<PatchType>
    {
        public List<PatchType> List;

        public NoblePatchGroupModel(IEnumerable<PatchType> Patches) {
            List = new List<PatchType>(Patches);
        }

        public PatchType GetPatchByID(int ID) {
            return List[ID];
        }
    }
}
