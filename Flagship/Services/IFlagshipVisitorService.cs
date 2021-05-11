using System.Collections.Generic;
using System.Threading.Tasks;
using Flagship.Model;
using Flagship.Model.Hits;

namespace Flagship.Services
{
    public interface IFlagshipVisitorService
    {
        Task SynchronizeModifications(Visitor visitor);

        T GetModification<T>(Visitor visitor, string key, T defaultValue = default, bool activate = true);

        ModificationInfo GetModificationInfo(Visitor visitor, string key);

        Task ActivateModification(Visitor visitor, string key);

        Task SendHit(Visitor visitor, HitType type, BaseHit hit);

        Task SendHit<T>(string visitorId, T hit) where T : BaseHit;
    }
}
